using System.Linq;
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
                        // not yet implemented
                    }
                    else
                    {
                        if (rule.Value != null)
                        {
                            RuleValue val;
                            if(rule.Value.Type == Newtonsoft.Json.Linq.JTokenType.Boolean)
                            {
                                val = new BooleanRuleValue((bool)rule.Value.Value);
                            }
                            else if (rule.Value.Type == Newtonsoft.Json.Linq.JTokenType.Integer)
                            {
                                val = new IntegerRuleValue((int)rule.Value.Value);
                            }
                            else if (rule.Value.Type == Newtonsoft.Json.Linq.JTokenType.Float)
                            {
                                val = new FloatRuleValue((float)rule.Value.Value);
                            }
                            else
                            {
                                val = new StringRuleValue(rule.Value.ToString());
                            }

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

    }

}