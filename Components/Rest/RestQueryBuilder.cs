using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene.Config;
using System.Linq;

namespace Satrabel.OpenContent.Components.Rest
{
    public static class RestQueryBuilder
    {
        public static Select MergeQuery(FieldConfig config, Select select, RestSelect restSelect, string cultureCode)
        {
            var query = select.Query;
            select.PageSize = restSelect.PageSize;
            select.PageIndex = restSelect.PageIndex;
            if (restSelect.Query != null && restSelect.Query.FilterRules != null)
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
                            query.AddRule(FieldConfigUtils.CreateFilterRule(config, cultureCode,
                                rule.Field,
                                rule.FieldOperator,
                                new StringRuleValue(rule.Value.ToString())
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