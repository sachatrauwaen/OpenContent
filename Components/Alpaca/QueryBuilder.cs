using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.search;
using Satrabel.OpenContent.Components.Lucene.Config;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Alpaca
{
    public class QueryBuilder
    {
        private readonly FieldConfig IndexConfig;

        public bool DefaultNoResults { get; private set; }
        public Select Select { get; private set; }

        public QueryBuilder(FieldConfig config)
        {
            this.IndexConfig = config;
            Select = new Select();
            Select.PageSize = 100;
        }

        public QueryBuilder Build(JObject query, bool addWorkflowFilter, NameValueCollection QueryString = null)
        {
            BuildPage(query);
            BuildFilter(query, addWorkflowFilter, QueryString);
            BuildSort(query);
            return this;
        }

        public QueryBuilder BuildPage(JObject query)
        {
            int maxResults = 0;
            var sMaxResults = query["MaxResults"];
            if (sMaxResults != null && int.TryParse(sMaxResults.ToString(), out maxResults))
            {
                Select.PageSize = maxResults;
            }
            var sDefaultNoResults = query["DefaultNoResults"] as JValue;
            if (sDefaultNoResults != null && sDefaultNoResults.Type == JTokenType.Boolean)
            {
                DefaultNoResults = (bool)sDefaultNoResults.Value;
            }
            return this;
        }
        public QueryBuilder BuildFilter(JObject query, bool addWorkflowFilter, NameValueCollection QueryString = null)
        {
            var Filter = Select.Filter;
            var vExcludeCurrentItem = query["ExcludeCurrentItem"] as JValue;
            bool ExcludeCurrentItem = false;
            if (vExcludeCurrentItem != null && vExcludeCurrentItem.Type == JTokenType.Boolean)
            {
                ExcludeCurrentItem = (bool)vExcludeCurrentItem.Value;
            }
            if (ExcludeCurrentItem && QueryString != null && QueryString["id"] != null)
            {
                Filter.AddRule(new FilterRule()
                {
                    Field = "id",
                    Value = new StringRuleValue(QueryString["id"]),
                    FieldOperator = OperatorEnum.NOT_EQUAL
                });
            }
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
                        if (indexConfig != null && indexConfig.IndexType == "boolean")
                        {
                            bool bval;
                            if (bool.TryParse(val, out bval))
                            {
                                Filter.AddRule(new FilterRule()
                                {
                                    Field = item.Name,
                                    Value = new BooleanRuleValue(bval)
                                });
                            }
                        }
                        else if (!string.IsNullOrEmpty(val))
                        {
                            Filter.AddRule(new FilterRule()
                            {
                                Field = item.Name,
                                Value = new StringRuleValue(val),
                                FieldOperator = OperatorEnum.START_WITH
                            });
                        }
                    }
                    else if (item.Value is JArray) // enum
                    {
                        var arr = (JArray)item.Value;
                        if (arr.Children().Any())
                        {
                            var arrGroup = new FilterGroup();

                            foreach (var arrItem in arr.Children())
                            {
                                if (arrItem is JValue)
                                {
                                    var val = (JValue)arrItem;
                                    arrGroup.AddRule(new FilterRule()
                                    {
                                        Field = item.Name,
                                        Value = new StringRuleValue(val.ToString())
                                    });
                                }
                            }
                            Filter.FilterGroups.Add(arrGroup);
                        }
                        else if (QueryString != null && QueryString[item.Name] != null)
                        {
                            Filter.AddRule(new FilterRule()
                            {
                                Field = item.Name,
                                Value = new StringRuleValue(QueryString[item.Name])
                            });
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
                            Filter.AddRule(new FilterRule()
                            {
                                Field = item.Name,
                                LowerValue = new DateTimeRuleValue(startDate),
                                UpperValue = new DateTimeRuleValue(endDate),
                                FieldOperator = OperatorEnum.BETWEEN
                            });
                        }
                    }
                }
            }
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(Filter);
            }
            //Filter = Filter.FilterRules.Any() || Filter.FilterGroups.Any() > 0 ? q : null;
            return this;
        }
        public QueryBuilder BuildFilter(bool addWorkflowFilter, NameValueCollection QueryString = null)
        {
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(Select.Filter);
            }
            //Filter = q.Clauses.Count > 0 ? q : null;
            return this;
        }

        private void AddWorkflowFilter(FilterGroup filter)
        {

            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishstatus"))
            {
                filter.AddRule(new FilterRule()
                {
                    Field = "publishstatus",
                    Value = new StringRuleValue("published")
                });
            }
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishstartdate"))
            {
                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.Today;
                filter.AddRule(new FilterRule()
                {
                    Field = "publishstartdate",
                    Value = new DateTimeRuleValue(DateTime.Today),
                    FieldOperator = OperatorEnum.LESS_THEN_OR_EQUALS
                });
            }
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishenddate"))
            {
                DateTime startDate = DateTime.Today;
                DateTime endDate = DateTime.MaxValue;
                filter.AddRule(new FilterRule()
                {
                    Field = "publishenddate",
                    Value = new DateTimeRuleValue(DateTime.Today),
                    FieldOperator = OperatorEnum.GREATER_THEN_OR_EQUALS
                });
            }
        }

        public QueryBuilder BuildSort(JObject query)
        {
            var Sort = Select.Sort;
            var sortArray = query["Sort"] as JArray;
            if (sortArray != null && sortArray.Any())
            {
                foreach (JObject item in sortArray)
                {
                    bool reverse = false;
                    string fieldName = item["Field"].ToString();
                    string fieldOrder = item["Order"].ToString();
                    if (fieldOrder == "desc")
                    {
                        reverse = true;
                    }
                    Sort.Add(new SortRule()
                    {
                        Field = fieldName,
                        FieldType = Sortfieldtype(fieldName),
                        Descending = reverse
                    });
                }
            }
            return this;
        }

        private FieldTypeEnum Sortfieldtype(string fieldName)
        {
            var sortfieldtype = FieldTypeEnum.STRING;
            if (IndexConfig != null && IndexConfig.Fields.ContainsKey(fieldName))
            {
                var config = IndexConfig.Items == null ? IndexConfig.Fields[fieldName] : IndexConfig.Items;
                if (config.IndexType == "datetime" || config.IndexType == "date" || config.IndexType == "time")
                {
                    sortfieldtype = FieldTypeEnum.DATETIME;
                }
                else if (config.IndexType == "boolean")
                {
                    sortfieldtype = FieldTypeEnum.INTEGER;
                }
                else if (config.IndexType == "int")
                {
                    sortfieldtype = FieldTypeEnum.LONG;
                }
                else if (config.IndexType == "long")
                {
                    sortfieldtype = FieldTypeEnum.LONG;
                }
                else if (config.IndexType == "float" || config.IndexType == "double")
                {
                    sortfieldtype = FieldTypeEnum.FLOAT;
                }
                else if (config.IndexType == "double")
                {
                    sortfieldtype = FieldTypeEnum.INTEGER; // ????
                }
                else if (config.IndexType == "key")
                {
                    sortfieldtype = FieldTypeEnum.KEY;
                }
                else if (config.IndexType == "text")
                {
                    sortfieldtype = FieldTypeEnum.TEXT;
                }
                else if (config.IndexType == "html")
                {
                    sortfieldtype = FieldTypeEnum.HTML;
                }

            }
            return sortfieldtype;
        }

        public QueryBuilder BuildSort(string Sorts)
        {
            var Sort = Select.Sort;
            if (!string.IsNullOrEmpty(Sorts))
            {
                var sortArray = Sorts.Split(',');

                foreach (var item in sortArray)
                {
                    bool reverse = false;
                    var sortElements = item.Split(' ');
                    string fieldName = sortElements[0];
                    if (sortElements.Length > 1 && sortElements[1].ToLower() == "desc")
                    {
                        reverse = true;
                    }
                    Sort.Add(new SortRule()
                    {
                        Field = fieldName,
                        FieldType = Sortfieldtype(fieldName),
                        Descending = reverse
                    });
                }
            }
            return this;
        }
    }
}