using System.Linq;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Rest
{
    public static class RestQueryBuilder
    {
        public static Select MergeQuery(FieldConfig config, Select select, RestSelect restSelect, string cultureCode)
        {
            var query = select.Query;
            select.PageSize = restSelect.PageSize;
            select.PageIndex = restSelect.PageIndex;
            if (restSelect.Query?.FilterRules != null)
            {
                foreach (var rule in restSelect.Query.FilterRules)
                {
                    if (rule.FieldOperator == OperatorEnum.IN)
                    {
                        if (rule.MultiValue != null)
                        {
                            query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                rule.Field,
                                rule.FieldOperator,
                                rule.MultiValue.Select(v => new StringRuleValue(v.ToString()))
                            ));
                        }
                    }
                    else if (rule.FieldOperator == OperatorEnum.BETWEEN)
                    {
                        var val1 = GenerateValue(rule.LowerValue);
                        var val2 = GenerateValue(rule.UpperValue);
                        query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                 rule.Field,
                                 rule.FieldOperator,
                                 val1, val2
                             ));
                    }
                    else
                    {
                        // EQUAL
                        if (rule.Value != null)
                        {
                            RuleValue val;

                            val = GenerateValue(rule.Value);

                            query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                rule.Field,
                                rule.FieldOperator,
                                val
                            ));
                        }
                    }
                }
            }

            if (restSelect.Sort != null && restSelect.Sort.Any())
            {
                select.Sort.Clear();
                foreach (var sort in restSelect.Sort)
                {
                    select.Sort.Add(FieldConfigUtils.CreateSortRule(config, cultureCode,
                        sort.Field,
                        sort.Descending
                    ));
                }
            }
            return select;
        }

        private static RuleValue GenerateValue(JValue jvalue)
        {
            
            RuleValue val;
            if (jvalue.Type == Newtonsoft.Json.Linq.JTokenType.Boolean)
            {
                val = new BooleanRuleValue((bool)jvalue.Value);
            }
            else if (jvalue.Type == Newtonsoft.Json.Linq.JTokenType.Integer)
            {
                val = new IntegerRuleValue((int)jvalue.Value);
            }
            else if (jvalue.Type == Newtonsoft.Json.Linq.JTokenType.Float)
            {
                val = new FloatRuleValue((float)jvalue.Value);
            }
            else
            {
                val = new StringRuleValue(jvalue.ToString());
            }

            return val;
        }
    }

}