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

        public QueryBuilder Build(JObject query, bool addWorkflowFilter, int userId, string cultureCode, NameValueCollection queryString = null)
        {
            BuildPage(query);
            BuildFilter(query, addWorkflowFilter, userId, cultureCode, queryString);
            BuildSort(query, cultureCode);
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
        public QueryBuilder BuildFilter(JObject query, bool addWorkflowFilter, int userId, string cultureCode, NameValueCollection queryString = null)
        {
            var workFlowFilter = Select.Filter;
            var vExcludeCurrentItem = query["ExcludeCurrentItem"] as JValue;
            bool excludeCurrentItem = false;
            if (vExcludeCurrentItem != null && vExcludeCurrentItem.Type == JTokenType.Boolean)
            {
                excludeCurrentItem = (bool)vExcludeCurrentItem.Value;
            }
            if (excludeCurrentItem && queryString != null && queryString["id"] != null)
            {
                workFlowFilter.AddRule(new FilterRule()
                {
                    Field = "id",
                    Value = new StringRuleValue(queryString["id"]),
                    FieldOperator = OperatorEnum.NOT_EQUAL
                });
            }
            var vCurrentUserItems = query["CurrentUserItems"] as JValue;
            bool currentUserItems = false;
            if (vCurrentUserItems != null && vCurrentUserItems.Type == JTokenType.Boolean)
            {
                currentUserItems = (bool)vCurrentUserItems.Value;
            }
            if (currentUserItems)
            {
                workFlowFilter.AddRule(new FilterRule()
                {
                    Field = "userid",
                    Value = new StringRuleValue(userId.ToString()),
                    FieldOperator = OperatorEnum.EQUAL
                });
            }
            var filter = query["Filter"] as JObject;
            if (filter != null)
            {
                foreach (var item in filter.Properties())
                {
                    var fieldConfig = FieldConfigUtils.GetField(IndexConfig , item.Name);
                    if (item.Value is JValue) // text
                    {
                        var val = item.Value.ToString();
                        if (fieldConfig != null && fieldConfig.IndexType == "boolean")
                        {
                            bool bval;
                            if (bool.TryParse(val, out bval))
                            {
                                workFlowFilter.AddRule(new FilterRule()
                                {
                                    Field = item.Name,
                                    FieldType = FieldTypeEnum.BOOLEAN,
                                    Value = new BooleanRuleValue(bval)
                                });
                            }
                        }
                        else if (!string.IsNullOrEmpty(val))
                        {
                            workFlowFilter.AddRule(FieldConfigUtils.CreateFilterRule(IndexConfig, cultureCode,                            
                                item.Name,
                                OperatorEnum.START_WITH,
                                new StringRuleValue(val)
                            ));
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
                                    arrGroup.AddRule(FieldConfigUtils.CreateFilterRule(IndexConfig, cultureCode,
                                        item.Name,                                        
                                        OperatorEnum.EQUAL,
                                        new StringRuleValue(val.ToString())
                                    ));
                                }
                            }
                            workFlowFilter.FilterGroups.Add(arrGroup);
                        }
                        else if (queryString != null && queryString[item.Name] != null)
                        {
                            workFlowFilter.AddRule(FieldConfigUtils.CreateFilterRule(IndexConfig, cultureCode,
                            
                                item.Name,
                                OperatorEnum.EQUAL,
                                new StringRuleValue(queryString[item.Name])
                            ));
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
                            workFlowFilter.AddRule(new FilterRule()
                            {
                                Field = item.Name,
                                FieldType = FieldTypeEnum.DATETIME,
                                LowerValue = new DateTimeRuleValue(startDate),
                                UpperValue = new DateTimeRuleValue(endDate),
                                FieldOperator = OperatorEnum.BETWEEN
                            });
                        }
                    }
                }
            }
            BuildQueryStringFilter(queryString, workFlowFilter);
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(workFlowFilter);
            }
            //Filter = Filter.FilterRules.Any() || Filter.FilterGroups.Any() > 0 ? q : null;
            return this;
        }

        private void BuildQueryStringFilter(NameValueCollection queryString, FilterGroup workFlowFilter)
        {
            if (queryString != null)
            {
                foreach (string key in queryString)
                {
                    if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.Any(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var indexConfig = IndexConfig.Fields.Single(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        string val = queryString[key];
                        workFlowFilter.AddRule(new FilterRule()
                        {
                            Field = indexConfig.Key,
                            Value = new StringRuleValue(val),
                            FieldOperator = OperatorEnum.EQUAL,
                            FieldType = FieldConfigUtils.GetFieldType(indexConfig.Value != null ? indexConfig.Value.IndexType : string.Empty)
                        });
                    }
                }
            }
        }

        public QueryBuilder BuildFilter(bool addWorkflowFilter, string cultureCode, NameValueCollection queryString = null)
        {
            BuildQueryStringFilter(queryString, Select.Filter);
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(Select.Filter);
            }
            return this;
        }

        private void AddWorkflowFilter(FilterGroup filter)
        {

            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishstatus"))
            {
                filter.AddRule(new FilterRule()
                {
                    Field = "publishstatus",
                    Value = new StringRuleValue("published"),
                    FieldType = FieldTypeEnum.KEY
                });
            }
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishstartdate"))
            {
                //DateTime startDate = DateTime.MinValue;
                //DateTime endDate = DateTime.Today;
                filter.AddRule(new FilterRule()
                {
                    Field = "publishstartdate",
                    Value = new DateTimeRuleValue(DateTime.Today),
                    FieldOperator = OperatorEnum.LESS_THEN_OR_EQUALS,
                    FieldType = FieldTypeEnum.DATETIME
                });
            }
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("publishenddate"))
            {
                //DateTime startDate = DateTime.Today;
                //DateTime endDate = DateTime.MaxValue;
                filter.AddRule(new FilterRule()
                {
                    Field = "publishenddate",
                    Value = new DateTimeRuleValue(DateTime.Today),
                    FieldOperator = OperatorEnum.GREATER_THEN_OR_EQUALS,
                    FieldType = FieldTypeEnum.DATETIME
                });
            }
        }

        public QueryBuilder BuildSort(JObject query, string cultureCode)
        {
            var Sort = Select.Sort;
            var sortArray = query["Sort"] as JArray;
            if (sortArray != null && sortArray.Any())
            {
                foreach (JObject item in sortArray)
                {
                    string fieldName = item["Field"].ToString();
                    string fieldOrder = item["Order"].ToString();
                    Sort.Add(FieldConfigUtils.CreateSortRule(IndexConfig, cultureCode,
                        fieldName,
                        fieldOrder == "desc"
                    ));
                }
            }
            return this;
        }
        public QueryBuilder BuildSort(string Sorts, string cultureCode)
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
                    Sort.Add(FieldConfigUtils.CreateSortRule(IndexConfig, cultureCode,
                        fieldName,
                        reverse
                    ));
                }
            }
            return this;
        }

        private bool SortfieldMultiLanguage(string fieldName)
        {
            
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey(fieldName))
            {
                var config = IndexConfig.Fields[fieldName].Items == null ? IndexConfig.Fields[fieldName] : IndexConfig.Fields[fieldName].Items;
                return config.MultiLanguage;
            }
            return false;
        }
    }
}