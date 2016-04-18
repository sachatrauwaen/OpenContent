using Satrabel.OpenContent.Components.Datasource.search;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.JPList
{
    public static class JplistQueryBuilder
    {
        public static Select MergeJpListQuery(Select select, List<StatusDTO> statuses)
        {
            var query = select.Query;
            foreach (StatusDTO status in statuses)
            {
                switch (status.action)
                {
                    case "paging":
                        {
                            int number = 100000;
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
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        FieldOperator = OperatorEnum.START_WITH,
                                        Value = new StringRuleValue(status.data.value),
                                    });
                                }
                                else
                                {
                                    var group = new FilterGroup() { Condition = ConditionEnum.OR };
                                    foreach (var n in names)
                                    {
                                        group.AddRule(new FilterRule()
                                        {
                                            Field = n,
                                            FieldOperator = OperatorEnum.START_WITH,
                                            Value = new StringRuleValue(status.data.value),
                                        });
                                    }
                                    query.FilterGroups.Add(group);
                                }
                            }
                            else if ((status.type == "checkbox-group-filter" || status.type == "button-filter-group" || status.type == "combined")
                                        && status.data != null && !string.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "pathGroup" && status.data.pathGroup != null && status.data.pathGroup.Count > 0)
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        FieldOperator = OperatorEnum.IN,
                                        MultiValue = status.data.pathGroup.Select(s => new StringRuleValue(s)),
                                    });
                                }
                            }
                            else if (status.type == "filter-select" && status.data != null && !string.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "path" && !string.IsNullOrEmpty(status.data.path))
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        Value = new StringRuleValue(status.data.path),
                                    });
                                }
                            }
                            break;
                        }

                    case "sort":
                        {
                            select.Sort.Clear();
                            select.Sort.Add(new SortRule()
                            {
                                Field = status.data.path,
                                Descending = status.data.order == "desc",
                                //FieldType = FieldTypeEnum.
                            });
                            break;
                        }
                }
            }
            return select;
        }
    }
}