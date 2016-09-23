using System;

namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class DateTimeRuleValue : RuleValue
    {
        private DateTime Value;
        public DateTimeRuleValue(DateTime value)
        {
            Value = value;
        }
        public override DateTime AsDateTime
        {
            get
            {
                return Value;
            }
        }
        public override string AsString
        {
            get
            {
                return Value.ToString();
            }
        }
    }
}