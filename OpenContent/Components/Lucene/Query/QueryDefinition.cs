using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DotNetNuke.Entities.Users;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Indexing;

namespace Satrabel.OpenContent.Components.Lucene
{
    public class QueryDefinition
    {
        private readonly FieldConfig _indexConfig;
        private bool _defaultNoResults;
        private Query _query;

        public QueryDefinition(FieldConfig config)
        {
            this._indexConfig = config;
            Query = new MatchAllDocsQuery();
            Sort = Sort.RELEVANCE;
            PageSize = 100;
        }
        public Query Filter { get; set; }
        public Sort Sort { get; set; }
        public Query Query
        {
            get
            {
                return _query;
            }
            set
            {
                if (_defaultNoResults && value != null && (new MatchAllDocsQuery()).ToString() == value.ToString())
                {
                    _query = new BooleanQuery();
                }
                else
                {
                    _query = value;
                }
            }
        }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public QueryDefinition Build(JObject query, bool addWorkflowFilter, IList<UserRoleInfo> roles, NameValueCollection queryString = null)
        {
            BuildPage(query);
            BuildFilter(query, addWorkflowFilter, roles, queryString);
            BuildSort(query);
            return this;
        }

        public QueryDefinition BuildPage(JObject query)
        {
            int maxResults = 0;
            var sMaxResults = query["MaxResults"];
            if (sMaxResults != null && int.TryParse(sMaxResults.ToString(), out maxResults))
            {
                PageSize = maxResults;
            }

            var sDefaultNoResults = query["DefaultNoResults"] as JValue;
            if (sDefaultNoResults != null && sDefaultNoResults.Type == JTokenType.Boolean)
            {
                _defaultNoResults = (bool)sDefaultNoResults.Value;
            }

            return this;
        }
        public QueryDefinition BuildFilter(JObject query, bool addWorkflowFilter, IList<UserRoleInfo> roles, NameValueCollection queryString = null)
        {
            BooleanQuery q = new BooleanQuery();

            var vExcludeCurrentItem = query["ExcludeCurrentItem"] as JValue;
            bool excludeCurrentItem = false;
            if (vExcludeCurrentItem != null && vExcludeCurrentItem.Type == JTokenType.Boolean)
            {
                excludeCurrentItem = (bool)vExcludeCurrentItem.Value;
            }
            if (excludeCurrentItem && queryString != null && queryString["id"] != null)
            {
                q.Add(new MatchAllDocsQuery(), Occur.MUST);
                q.Add(new TermQuery(new Term("$id", queryString["id"])), Occur.MUST_NOT);
            }
            var filter = query["Filter"] as JObject;
            if (filter != null)
            {
                foreach (var item in filter.Properties())
                {
                    var indexConfig = _indexConfig != null && _indexConfig.Fields != null &&
                                      _indexConfig.Fields.ContainsKey(item.Name)
                        ? _indexConfig.Fields[item.Name]
                        : null;
                    if (item.Value is JValue) // text
                    {
                        var val = item.Value.ToString();
                        if (indexConfig != null && indexConfig.IndexType == "boolean")
                        {
                            bool bval;
                            if (bool.TryParse(val, out bval))
                            {
                                int ival = bval ? 1 : 0;
                                q.Add(NumericRangeQuery.NewIntRange(item.Name, ival, ival, true, true), Occur.MUST);
                            }
                        }
                        else if (!string.IsNullOrEmpty(val))
                        {
                            q.Add(new WildcardQuery(new Term(item.Name, val)), Occur.MUST);
                        }
                    }
                    else if (item.Value is JArray) // enum
                    {
                        var arr = (JArray)item.Value;
                        if (arr.Children().Any())
                        {
                            BooleanQuery arrQ = new BooleanQuery();
                            foreach (var arrItem in arr.Children())
                            {
                                if (arrItem is JValue)
                                {
                                    var val = (JValue)arrItem;
                                    arrQ.Add(new TermQuery(new Term(item.Name, val.ToString())), Occur.SHOULD); // or
                                }
                            }
                            q.Add(arrQ, Occur.MUST);
                        }
                        else if (queryString?[item.Name] != null)
                        {
                            q.Add(new TermQuery(new Term(item.Name, queryString[item.Name])), Occur.MUST);
                        }
                    }
                    else if (item.Value is JObject) // range
                    {
                        var valObj = (JObject)item.Value;
                        var startDays = valObj["StartDays"] as JValue;
                        var endDays = valObj["EndDays"] as JValue;
                        if ((startDays?.Value != null) || (endDays?.Value != null))
                        {
                            var startDate = DateTime.MinValue;
                            var endDate = DateTime.MaxValue;
                            try
                            {
                                startDate = DateTime.Today.AddDays(-(long)startDays.Value);
                            }
                            catch (Exception)
                            {
                            }
                            try
                            {
                                endDate = DateTime.Today.AddDays((long)endDays.Value);
                            }
                            catch (Exception)
                            {
                            }
                            q.Add(NumericRangeQuery.NewLongRange(item.Name, startDate.Ticks, endDate.Ticks, true, true), Occur.MUST);

                            //q.Add(new TermRangeQuery(item.Name, DateTools.DateToString(startDate, DateTools.Resolution.SECOND), DateTools.DateToString(endDate, DateTools.Resolution.SECOND), true, true), Occur.MUST);

                        }
                    }
                }
            }
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(q);
                AddRoleFilter(q, roles);
            }
            Filter = q.Clauses.Count > 0 ? q : null;
            return this;
        }

        public QueryDefinition BuildFilter(bool addWorkflowFilter, IList<UserRoleInfo> roles, NameValueCollection queryString = null)
        {
            BooleanQuery q = new BooleanQuery();
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(q);
                AddRoleFilter(q, roles);
            }
            Filter = q.Clauses.Count > 0 ? q : null;
            return this;
        }

        private void AddWorkflowFilter(BooleanQuery q)
        {

            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(AppConfig.FieldNamePublishStatus))
            {
                q.Add(new TermQuery(new Term(AppConfig.FieldNamePublishStatus, "published")), Occur.MUST); // and
            }
            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(AppConfig.FieldNamePublishStartDate))
            {
                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.Today;
                q.Add(NumericRangeQuery.NewLongRange(AppConfig.FieldNamePublishStartDate, startDate.Ticks, endDate.Ticks, true, true), Occur.MUST);
            }
            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(AppConfig.FieldNamePublishEndDate))
            {
                DateTime startDate = DateTime.Today;
                DateTime endDate = DateTime.MaxValue;
                q.Add(NumericRangeQuery.NewLongRange(AppConfig.FieldNamePublishEndDate, startDate.Ticks, endDate.Ticks, true, true), Occur.MUST);
            }
        }

        private void AddRoleFilter(BooleanQuery q, IList<UserRoleInfo> roles)
        {

            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey("userRole"))
            {
                if (roles.Any())
                {
                    BooleanQuery arrQ = new BooleanQuery();
                    foreach (var role in roles)
                    {
                        arrQ.Add(new TermQuery(new Term("userRole", role.RoleID.ToString())), Occur.SHOULD); // or
                    }
                    q.Add(arrQ, Occur.MUST);
                }
                else
                {
                    q.Add(new TermQuery(new Term("userRole", "Unauthenticated")), Occur.MUST); // and
                }
            }
        }

        public QueryDefinition BuildSort(JObject query)
        {
            var sortArray = query["Sort"] as JArray;
            var sort = Sort.RELEVANCE;
            if (sortArray != null && sortArray.Any())
            {
                var sortFields = new List<SortField>();
                foreach (JObject item in sortArray)
                {
                    bool reverse = false;
                    string fieldName = item["Field"].ToString();
                    string fieldOrder = item["Order"].ToString();
                    if (fieldOrder == "desc")
                    {
                        reverse = true;
                    }
                    int sortfieldtype = SortField.STRING;
                    string sortFieldPrefix = "";
                    Sortfieldtype(fieldName, ref sortfieldtype, ref sortFieldPrefix);
                    sortFields.Add(new SortField(sortFieldPrefix + fieldName, sortfieldtype, reverse));
                }
                sort = new Sort(sortFields.ToArray());
            }
            Sort = sort;
            return this;
        }

        private void Sortfieldtype(string fieldName, ref int sortfieldtype, ref string sortFieldPrefix)
        {
            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(fieldName))
            {
                //var config = IndexConfig.Items == null ? IndexConfig.Fields[fieldName] : IndexConfig.Items;
                FieldConfig config;
                if (_indexConfig.Items == null)
                {
                    config = _indexConfig.Fields[fieldName];
                    if (config.Items != null)
                    {
                        //this seems to be an array
                        config = config.Items;
                    }
                }
                else
                    config = _indexConfig.Items;
                if (config.IndexType == "date" || config.IndexType == "datetime" || config.IndexType == "time")
                {
                    sortfieldtype = SortField.LONG;
                }
                else if (config.IndexType == "boolean")
                {
                    sortfieldtype = SortField.INT;
                }
                else if (config.IndexType == "int")
                {
                    sortfieldtype = SortField.LONG;
                }
                else if (config.IndexType == "long")
                {
                    sortfieldtype = SortField.LONG;
                }
                else if (config.IndexType == "float" || config.IndexType == "double")
                {
                    sortfieldtype = SortField.FLOAT;
                }
                else if (config.IndexType == "double")
                {
                    sortfieldtype = SortField.DOUBLE;
                }
                else if (config.IndexType == "key")
                {
                    sortfieldtype = SortField.STRING;
                }
                else if (config.IndexType == "text" || config.IndexType == "html")
                {
                    sortfieldtype = SortField.STRING;
                    sortFieldPrefix = "@";
                }
                else
                {
                    sortfieldtype = SortField.STRING;
                    sortFieldPrefix = "@";
                }
            }

        }
        public QueryDefinition BuildSort(string sorts)
        {
            var sort = Sort.RELEVANCE;
            if (!string.IsNullOrEmpty(sorts))
            {
                var sortArray = sorts.Split(',');
                var sortFields = new List<SortField>();
                foreach (var item in sortArray)
                {
                    bool reverse = false;
                    var sortElements = item.Split(' ');
                    string fieldName = sortElements[0];
                    if (sortElements.Length > 1 && sortElements[1].ToLower() == "desc")
                    {
                        reverse = true;
                    }
                    int sortfieldtype = SortField.STRING;
                    string sortFieldPrefix = "";
                    Sortfieldtype(fieldName, ref sortfieldtype, ref sortFieldPrefix);
                    sortFields.Add(new SortField(sortFieldPrefix + fieldName, sortfieldtype, reverse));
                }
                sort = new Sort(sortFields.ToArray());
            }
            Sort = sort;
            return this;
        }
    }
}