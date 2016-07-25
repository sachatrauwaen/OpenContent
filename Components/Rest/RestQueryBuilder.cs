using Satrabel.OpenContent.Components.Datasource.search;
using Satrabel.OpenContent.Components.Lucene.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestQueryBuilder
    {
        public static Select MergeQuery(FieldConfig config, Select select, RestSelect restSelect)
        {
            var query = select.Query;
            select.PageSize = restSelect.PageSize;
            select.PageIndex = restSelect.PageIndex;
            foreach (var rule in restSelect.Query.FilterRules)
            {
                query.AddRule(new FilterRule()
                                    {
                                        Field = rule.Field,
                                        FieldOperator = rule.FieldOperator,
                                        FieldType = rule.FieldType,
                                        Value = new StringRuleValue(rule.Value.ToString()),
                                    });
            }
            if (restSelect.Sort.Any())
            {
                select.Sort.Clear();
                foreach (var sort in restSelect.Sort)
                {
                    select.Sort.Add(new SortRule()
                    {
                        Field = sort.Field,
                        Descending = sort.Descending,
                        FieldType = Sortfieldtype(config, sort.Field)
                    });

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