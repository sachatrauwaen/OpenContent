using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class FilterGroup
    {
        public FilterGroup()
        {
            Condition = ConditionEnum.AND;
            FilterRules = new List<FilterRule>();
            FilterGroups = new List<FilterGroup>();
        }
        public ConditionEnum Condition { get; set; }
        public List<FilterRule> FilterRules { get; private set; }
        public List<FilterGroup> FilterGroups { get; private set; }

        public void AddRule(FilterRule rule)
        {
            FilterRules.Add(rule);
        }
        public void AddRule(FilterGroup rule)
        {
            FilterGroups.Add(rule);
        }
    }
}