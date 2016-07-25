using System;
using Satrabel.OpenContent.Components.Datasource.search;
using Satrabel.OpenContent.Components.Lucene.Config;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.JPList
{
    public static class JplistQueryBuilder
    {
        public static Select MergeJpListQuery(FieldConfig config, Select select, List<StatusDTO> statuses)
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
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.name,
                                        FieldOperator = OperatorEnum.START_WITH,
                                        Value = new StringRuleValue(status.data.value),
                                        FieldType = Sortfieldtype(config, status.name)
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
                                            FieldType = Sortfieldtype(config, n)
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
                                        FieldType = Sortfieldtype(config, status.name)
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
                                        FieldType = Sortfieldtype(config, status.name)
                                    });
                                }
                            }
                            else if (status.type == "autocomplete" && status.data != null && !string.IsNullOrEmpty(status.data.path) && !string.IsNullOrEmpty(status.data.value))
                            {
                                var names = status.data.path.Split(',');
                                if (names.Length == 1)
                                {
                                    query.AddRule(new FilterRule()
                                    {
                                        Field = status.data.path,
                                        FieldOperator = OperatorEnum.START_WITH,
                                        Value = new StringRuleValue(status.data.value),
                                        FieldType = Sortfieldtype(config, status.data.path)
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
                                            FieldType = Sortfieldtype(config, n)
                                        });
                                    }
                                    query.FilterGroups.Add(group);
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
                                FieldType = Sortfieldtype(config, status.data.path)
                            });
                            break;
                        }
                }
            }
            return select;
        }

        private static FieldTypeEnum Sortfieldtype(FieldConfig indexConfig, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new Exception("Sort field is empty");
            if (indexConfig != null && indexConfig.Fields != null && indexConfig.Fields.ContainsKey(fieldName))
            {
                //var config = indexConfig.Items == null ? indexConfig.Fields[fieldName] : indexConfig.Items;
                FieldConfig config;
                if (indexConfig.Items == null)
                {
                    config = indexConfig.Fields[fieldName];
                    if (config.Items != null)
                    {
                        //this seems to be an array
                        config = config.Items;
                    }
                }
                else
                    config = indexConfig.Items;

                switch (config.IndexType)
                {
                    case "datetime":
                    case "date":
                    case "time":
                        return FieldTypeEnum.DATETIME;
                    case "boolean":
                        return FieldTypeEnum.BOOLEAN;
                    case "int":
                        return FieldTypeEnum.INTEGER;
                    case "long":
                        return FieldTypeEnum.LONG;
                    case "float":
                    case "double":
                        return FieldTypeEnum.FLOAT;
                    case "key":
                        return FieldTypeEnum.KEY;
                    case "text":
                        return FieldTypeEnum.TEXT;
                    case "html":
                        return FieldTypeEnum.HTML;
                    default:
                        return FieldTypeEnum.STRING;
                }
            }
            return FieldTypeEnum.STRING;
        }

    }
}