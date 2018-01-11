using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.FileIndexer;

namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public class FieldConfigUtils
    {
        internal static FieldConfig GetField(FieldConfig fieldconfig, string field)
        {
            var fieldParts = field.Split('.');
            while (true)
            {
                if (fieldParts.Length == 0) return null;

                var returnConfig = ExtractField(fieldconfig, fieldParts[0]);
                if (fieldParts.Length == 1)
                {
                    return returnConfig;
                }

                fieldParts = fieldParts.Skip(1).ToArray();
                fieldconfig = returnConfig;
            }
        }

        private static FieldConfig ExtractField(FieldConfig config, string field)
        {
            if (config?.Fields != null && config.Fields.ContainsKey(field))
            {
                return config.Fields[field].Items == null ? config.Fields[field] : config.Fields[field].Items;
            }
            return null;
        }

        public static FilterRule CreateFilterRule(FieldConfig config, string cultureCode, string field, OperatorEnum fieldOperator, IEnumerable<RuleValue> multiValue)
        {
            var fieldConfig = GetField(config, field);
            var cultureSuffix = fieldConfig != null && fieldConfig.MultiLanguage ? "." + cultureCode : string.Empty;
            var indexType = GetFieldType(fieldConfig != null ? fieldConfig.IndexType : string.Empty);
            if (fieldConfig == null || fieldConfig.Index == false)
            {
                throw new Exception($"You want me to search in field {field} but it is not indexed. Please fix your index.json.");
            }
            var rule = new FilterRule()
            {
                Field = field + cultureSuffix,
                FieldOperator = fieldOperator,
                FieldType = indexType,
                MultiValue = multiValue,
            };
            return rule;
        }
        public static FilterRule CreateFilterRule(FieldConfig config, string cultureCode, string field, OperatorEnum fieldOperator, RuleValue value)
        {
            var fieldConfig = GetField(config, field);
            var cultureSuffix = fieldConfig != null && fieldConfig.MultiLanguage ? "." + cultureCode : string.Empty;
            var indexType = GetFieldType(fieldConfig != null ? fieldConfig.IndexType : string.Empty);
            if (fieldConfig == null || fieldConfig.Index == false)
            {
                throw new Exception($"You want me to search in field {field} but it is not indexed. Please fix your index.json.");
            }
            var rule = new FilterRule()
            {
                Field = field + cultureSuffix,
                FieldOperator = fieldOperator,
                FieldType = indexType,
                Value = value,
            };
            return rule;
        }
        public static FilterRule CreateFilterRule(FieldConfig config, string cultureCode, string field, OperatorEnum fieldOperator, RuleValue lowerValue, RuleValue upperValue)
        {
            var fieldConfig = GetField(config, field);
            var cultureSuffix = fieldConfig != null && fieldConfig.MultiLanguage ? "." + cultureCode : string.Empty;
            var indexType = GetFieldType(fieldConfig != null ? fieldConfig.IndexType : string.Empty);
            if (fieldConfig == null || fieldConfig.Index == false)
            {
                throw new Exception($"You want me to search in field {field} but it is not indexed. Please fix your index.json.");
            }
            var rule = new FilterRule()
            {
                Field = field + cultureSuffix,
                FieldOperator = fieldOperator,
                FieldType = indexType,
                LowerValue = lowerValue,
                UpperValue = upperValue
            };
            return rule;
        }
        public static SortRule CreateSortRule(FieldConfig config, string cultureCode, string field, bool descending)
        {
            var fieldConfig = GetField(config, field);
            var cultureSuffix = fieldConfig != null && fieldConfig.MultiLanguage ? "." + cultureCode : string.Empty;
            var indexType = GetFieldType(fieldConfig != null ? fieldConfig.IndexType : string.Empty);
            var rule = new SortRule()
            {
                Field = field + cultureSuffix,
                FieldType = indexType,
                Descending = descending
            };
            return rule;
        }
        public static FieldTypeEnum GetFieldType(string indexType)
        {
            if (!string.IsNullOrEmpty(indexType))
            {
                switch (indexType)
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