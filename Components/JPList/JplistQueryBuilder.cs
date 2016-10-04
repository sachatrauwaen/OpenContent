using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene.Config;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.JPList
{
    public static class JplistQueryBuilder
    {
        public static Select MergeJpListQuery(FieldConfig config, Select select, List<StatusDTO> statuses, string cultureCode)
        {
            var query = select.Query;
            foreach (StatusDTO status in statuses)
            {
                switch (status.action)
                {
                    case "paging":
                        {
                            int number;
                            //  string value (it could be number or "all")
                            int.TryParse(status.data.number, out number);
                            select.PageSize = number;
                            select.PageIndex = status.data.currentPage;
                            break;
                        }
                    case "filter":
                        {
                            if (status.type == "textbox" && status.data != null && !string.IsNullOrEmpty(status.name) && !string.IsNullOrEmpty(status.data.value))
                            {
                                var names = status.name.Split(',');
                                if (names.Length == 1)
                                {
                                    query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                        status.name,
                                        OperatorEnum.START_WITH,
                                        new StringRuleValue(status.data.value)
                                    ));
                                }
                                else
                                {
                                    var group = new FilterGroup() { Condition = ConditionEnum.OR };
                                    foreach (var n in names)
                                    {
                                        group.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                            n,
                                            OperatorEnum.START_WITH,
                                            new StringRuleValue(status.data.value)
                                        ));
                                    }
                                    query.FilterGroups.Add(group);
                                }
                            }
                            else if ((status.type == "checkbox-group-filter" || status.type == "button-filter-group" || status.type == "combined")
                                        && status.data != null && !string.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "pathGroup" && status.data.pathGroup != null && status.data.pathGroup.Count > 0)
                                {
                                    query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                        status.name,
                                        OperatorEnum.IN,
                                        status.data.pathGroup.Select(s => new StringRuleValue(s))
                                    ));
                                }
                            }
                            else if (status.type == "filter-select" && status.data != null && !string.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "path" && !string.IsNullOrEmpty(status.data.path) && (status.data.path != "*") )
                                {
                                    query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                        status.name,
                                        OperatorEnum.EQUAL,
                                        new StringRuleValue(status.data.path)
                                    ));
                                }
                            }
                            else if (status.type == "autocomplete" && status.data != null && !string.IsNullOrEmpty(status.data.path) && !string.IsNullOrEmpty(status.data.value))
                            {
                                var names = status.data.path.Split(',');
                                if (names.Length == 1)
                                {
                                    query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                        status.data.path,
                                        OperatorEnum.START_WITH,
                                        new StringRuleValue(status.data.value)
                                    ));
                                }
                                else
                                {
                                    var group = new FilterGroup() { Condition = ConditionEnum.OR };
                                    foreach (var n in names)
                                    {
                                        group.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                            n,
                                            OperatorEnum.START_WITH,
                                            new StringRuleValue(status.data.value)
                                        ));
                                    }
                                    query.FilterGroups.Add(group);
                                }
                            }
                            break;
                        }

                    case "sort":
                        {
                            select.Sort.Clear();
                            select.Sort.Add(FieldConfigUtils.CreateSortRule(config, cultureCode,
                                status.data.path,
                                status.data.order == "desc"
                            ));
                            break;
                        }
                }
            }
            return select;
        }
    }
}