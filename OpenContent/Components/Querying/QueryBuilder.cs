using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene.Config;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Satrabel.OpenContent.Components.Querying
{
    public class QueryBuilder
    {
        private readonly FieldConfig _indexConfig;

        public bool DefaultNoResults { get; private set; }
        public Select Select { get; }

        public QueryBuilder(FieldConfig config)
        {
            _indexConfig = config;
            Select = new Select
            {
                PageSize = 100
            };
        }

        public QueryBuilder Build(JObject query, bool addWorkflowFilter, int userId, string cultureCode, IList<UserRoleInfo> roles, NameValueCollection queryString = null)
        {
            if (query.IsEmpty())
            {
                BuildFilter(addWorkflowFilter, cultureCode, roles, queryString);
            }
            else
            {
                BuildPage(query);
                BuildFilter(query, addWorkflowFilter, userId, cultureCode, roles, queryString);
                BuildSort(query, cultureCode);
            }
            return this;
        }

        private QueryBuilder BuildPage(JObject query)
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

        private QueryBuilder BuildFilter(JObject query, bool addWorkflowFilter, int userId, string cultureCode, IList<UserRoleInfo> roles, NameValueCollection queryString = null)
        {
            var workFlowFilter = Select.Filter;
            var vExcludeCurrentItem = query["ExcludeCurrentItem"] as JValue;
            bool excludeCurrentItem = false;
            if (vExcludeCurrentItem != null && vExcludeCurrentItem.Type == JTokenType.Boolean)
            {
                excludeCurrentItem = (bool)vExcludeCurrentItem.Value;
            }
            if (excludeCurrentItem && queryString?["id"] != null)
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
                    if (item.Value is JValue) // text, int
                    {
                        var fieldConfig = FieldConfigUtils.GetField(_indexConfig, item.Name);
                        var val = item.Value.ToString();
                        if (fieldConfig != null && fieldConfig.IndexType == "boolean")
                        {
                            bool bval = false;
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
                        else if (fieldConfig != null && fieldConfig.IndexType == "float")
                        {
                            float fval = 0;
                            if (float.TryParse(val, out fval))
                            {
                                workFlowFilter.AddRule(new FilterRule()
                                {
                                    Field = item.Name,
                                    FieldType = FieldTypeEnum.FLOAT,
                                    Value = new FloatRuleValue(fval)
                                });
                            }
                        }
                        else if (!string.IsNullOrEmpty(val))
                        {
                            workFlowFilter.AddRule(FieldConfigUtils.CreateFilterRule(_indexConfig, cultureCode,
                                item.Name,
                                OperatorEnum.START_WITH,
                                new StringRuleValue(val)
                            ));
                        }
                    }
                    else if (item.Value is JArray) // enum
                    {
                        var arr = (JArray)item.Value;
                        if (arr.Children().Any(i => i is JValue))
                        {
                            var arrGroup = new FilterGroup();

                            arrGroup.AddRule(FieldConfigUtils.CreateFilterRule(_indexConfig, cultureCode,
                                item.Name,
                                OperatorEnum.IN,
                                arr.Children().Where(i => i is JValue).Select(s => new StringRuleValue(s.ToString()))
                            ));
                            workFlowFilter.FilterGroups.Add(arrGroup);
                        }
                        else if (queryString?[item.Name] != null)
                        {
                            workFlowFilter.AddRule(FieldConfigUtils.CreateFilterRule(_indexConfig, cultureCode,
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
                        if ((startDays?.Value != null) || (endDays?.Value != null))
                        {
                            var startDate = DateTime.MinValue;
                            var endDate = DateTime.MaxValue;
                            var useTime = false;
                            try
                            {
                                useTime = (bool)(valObj["UseTime"] as JValue).Value;
                            }
                            catch (Exception)
                            {
                            }
                            DateTime currentDateTime = useTime ? DateTime.Now : DateTime.Today;
                            try
                            {
                                startDate = currentDateTime.AddDays(-(long)startDays.Value);
                            }
                            catch (Exception)
                            {
                            }
                            try
                            {
                                endDate = currentDateTime.AddDays((long)endDays.Value);
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
                AddWorkflowFilter(workFlowFilter, userId);
                AddRolesFilter(workFlowFilter, roles);
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
                    if (_indexConfig?.Fields != null && _indexConfig.Fields.Any(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var indexConfig = _indexConfig.Fields.Single(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
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

        private QueryBuilder BuildFilter(bool addWorkflowFilter, string cultureCode, IList<UserRoleInfo> roles, NameValueCollection queryString = null)
        {
            BuildQueryStringFilter(queryString, Select.Filter);
            if (addWorkflowFilter)
            {
                AddWorkflowFilter(Select.Filter);
                AddRolesFilter(Select.Filter, roles);
            }
            return this;
        }

        private void AddWorkflowFilter(FilterGroup filter, int userid = -1)
        {

            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(App.Config.FieldNamePublishStatus))
            {

                var group = new FilterGroup() { Condition = ConditionEnum.OR };
                filter.FilterGroups.Add(group);

                group.AddRule(new FilterRule()
                {
                    Field = App.Config.FieldNamePublishStatus,
                    Value = new StringRuleValue("published"),
                    FieldType = FieldTypeEnum.KEY
                });

                // also show own draft items
                if (userid > 0)
                {
                    var draftgroup = new FilterGroup() { Condition = ConditionEnum.AND };
                    group.FilterGroups.Add(draftgroup);
                    draftgroup.AddRule(new FilterRule()
                    {
                        Field = App.Config.FieldNamePublishStatus,
                        Value = new StringRuleValue("draft"),
                        FieldType = FieldTypeEnum.KEY
                    });
                    draftgroup.AddRule(new FilterRule()
                    {
                        Field = "userid",
                        Value = new StringRuleValue(userid.ToString()),
                        FieldOperator = OperatorEnum.EQUAL
                    });
                }



            }
            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(App.Config.FieldNamePublishStartDate))
            {
                //DateTime startDate = DateTime.MinValue;
                //DateTime endDate = DateTime.Today;
                filter.AddRule(new FilterRule()
                {
                    Field = App.Config.FieldNamePublishStartDate,
                    Value = new DateTimeRuleValue(DateTime.Today),
                    FieldOperator = OperatorEnum.LESS_THEN_OR_EQUALS,
                    FieldType = FieldTypeEnum.DATETIME
                });
            }
            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(App.Config.FieldNamePublishEndDate))
            {
                //DateTime startDate = DateTime.Today;
                //DateTime endDate = DateTime.MaxValue;
                filter.AddRule(new FilterRule()
                {
                    Field = App.Config.FieldNamePublishEndDate,
                    Value = new DateTimeRuleValue(DateTime.Today),
                    FieldOperator = OperatorEnum.GREATER_THEN_OR_EQUALS,
                    FieldType = FieldTypeEnum.DATETIME
                });
            }
        }

        private void AddRolesFilter(FilterGroup filter, IList<UserRoleInfo> roles)
        {
            string fieldName = "";
            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey("userrole"))
            {
                fieldName = "userrole";
            }
            else if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey("userroles"))
            {
                fieldName = "userroles";
            }
            if (!string.IsNullOrEmpty(fieldName))
            {
                List<string> roleLst;
                if (roles.Any())
                {
                    roleLst = roles.Select(r => r.RoleId.ToString()).ToList();
                }
                else
                {
                    roleLst = new List<string>();
                    roleLst.Add("Unauthenticated");
                }
                roleLst.Add("AllUsers");
                filter.AddRule(new FilterRule()
                {
                    Field = fieldName,
                    FieldOperator = OperatorEnum.IN,
                    MultiValue = roleLst.OrderBy(r => r).Select(r => new StringRuleValue(r)),
                    FieldType = FieldTypeEnum.KEY
                });
            }
        }

        private bool SortfieldMultiLanguage(string fieldName)
        {

            if (_indexConfig?.Fields != null && _indexConfig.Fields.ContainsKey(fieldName))
            {
                var config = _indexConfig.Fields[fieldName].Items == null ? _indexConfig.Fields[fieldName] : _indexConfig.Fields[fieldName].Items;
                return config.MultiLanguage;
            }
            return false;
        }

        private QueryBuilder BuildSort(JObject query, string cultureCode)
        {
            var sort = Select.Sort;
            var sortArray = query["Sort"] as JArray;
            if (sortArray != null && sortArray.Any())
            {
                foreach (JObject item in sortArray)
                {
                    string fieldName = item["Field"].ToString();
                    string fieldOrder = item["Order"].ToString();
                    sort.Add(FieldConfigUtils.CreateSortRule(_indexConfig, cultureCode, fieldName, fieldOrder == "desc"));
                }
            }
            else
            {
                sort.Add(new SortRule()
                {
                    Field = "createdondate",
                    FieldType = FieldTypeEnum.DATETIME,
                    Descending = true
                });
            }
            return this;
        }

        //public QueryBuilder BuildSort(string sorts, string cultureCode)
        //{
        //    var sort = Select.Sort;
        //    if (!string.IsNullOrEmpty(sorts))
        //    {
        //        var sortArray = sorts.Split(',');

        //        foreach (var item in sortArray)
        //        {
        //            bool reverse = false;
        //            var sortElements = item.Split(' ');
        //            string fieldName = sortElements[0];
        //            if (sortElements.Length > 1 && sortElements[1].ToLower() == "desc")
        //            {
        //                reverse = true;
        //            }
        //            sort.Add(FieldConfigUtils.CreateSortRule(_indexConfig, cultureCode,
        //                fieldName,
        //                reverse
        //            ));
        //        }
        //    }
        //    return this;
        //}
    }
}