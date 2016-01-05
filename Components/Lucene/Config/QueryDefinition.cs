using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using Lucene.Net.Index;

namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public class QueryDefinition
    {
        private readonly FieldConfig IndexConfig;
        public QueryDefinition(FieldConfig config)
        {
            this.IndexConfig = config;
            Query = new MatchAllDocsQuery();
            Sort = Sort.RELEVANCE;
            PageSize = 100;
        }
        public Query Filter { get; set; }
        public Sort Sort { get; set; }
        public Query Query { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public QueryDefinition Build(JObject query, bool addWorkflowFilter, NameValueCollection QueryString = null)
        {
            BuildPage(query);
            BuildFilter(query, addWorkflowFilter ,QueryString);
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
            return this;
        }
        public QueryDefinition BuildFilter(JObject query, bool addWorkflowFilter, NameValueCollection QueryString = null)
        {
            BooleanQuery q = new BooleanQuery();
            var filter = query["Filter"] as JObject;
            if (filter != null)
            {
                foreach (var item in filter.Properties())
                {
                    var indexConfig = IndexConfig != null && IndexConfig.Fields != null &&
                                      IndexConfig.Fields.ContainsKey(item.Name)
                        ? IndexConfig.Fields[item.Name]
                        : null;
                    if (item.Value is JValue) // text
                    {
                        var val = item.Value.ToString();
                        if (indexConfig.IndexType == "boolean")
                        {
                            bool bval;
                            if (bool.TryParse(val, out bval))
                            {
                                int ival = bval ? 1 : 0;
                                q.Add(NumericRangeQuery.NewIntRange(item.Name, ival, ival, true, true), Occur.MUST);
                            }
                        }
                        else
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
                        else if (QueryString != null && QueryString[item.Name] != null)
                        {
                            q.Add(new TermQuery(new Term(item.Name, QueryString[item.Name])), Occur.MUST);
                        }
                    }
                    else if (item.Value is JObject) // range
                    {
                        var valObj = (JObject)item.Value;
                        var startDays = valObj["StartDays"] as JValue;
                        var endDays = valObj["EndDays"] as JValue;
                        if ((startDays != null && startDays.Value != null) || (endDays != null && endDays.Value != null))
                        {
                            var startDate = DateTime.MinValue;
                            var endDate = DateTime.MaxValue;
                            try
                            {
                                startDate = DateTime.Today.AddDays((long)startDays.Value);
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
            }
            Filter = q.Clauses.Count > 0 ? q : null;
            return this;
        }

        public QueryDefinition BuildFilter(bool addWorkflowFilter, NameValueCollection QueryString = null)
        {
            BooleanQuery q = new BooleanQuery();
            if (addWorkflowFilter)
            {
               AddWorkflowFilter(q);
            }
            Filter = q.Clauses.Count > 0 ? q : null;
            return this;
        }

        private void AddWorkflowFilter(BooleanQuery q)
        {

            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("status"))
            {
                q.Add(new TermQuery(new Term("status", "published")), Occur.MUST); // and
            }
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishstartdate"))
            {
                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.Today;
                q.Add(NumericRangeQuery.NewLongRange("publishstartdate", startDate.Ticks, endDate.Ticks, true, true), Occur.MUST);
            }
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishenddate"))
            {
                DateTime startDate = DateTime.Today;
                DateTime endDate = DateTime.MaxValue;
                q.Add(NumericRangeQuery.NewLongRange("publishenddate", startDate.Ticks, endDate.Ticks, true, true), Occur.MUST);
            }
        }

        public QueryDefinition BuildSort(JObject query)
        {
            var sortArray = query["Sort"] as JArray;
            var sort = Sort.RELEVANCE;
            if (sortArray != null)
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
            if (IndexConfig != null && IndexConfig.Fields.ContainsKey(fieldName))
            {
                var config = IndexConfig.Items == null ? IndexConfig.Fields[fieldName] : IndexConfig.Items;
                if (config.IndexType == "datetime" || config.IndexType == "date" || config.IndexType == "time")
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

        public QueryDefinition BuildSort(string Sorts)
        {
            var sort = Sort.RELEVANCE;
            if (!string.IsNullOrEmpty(Sorts))
            {
                var sortArray = Sorts.Split(',');
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