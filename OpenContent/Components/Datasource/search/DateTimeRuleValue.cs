using System;

namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class DateTimeRuleValue : RuleValue
    {
        private readonly DateTime _value;
        public DateTimeRuleValue(DateTime value)
        {
            _value = value;
        }
        public override DateTime AsDateTime => _value;
        public override string AsString => _value.ToString();
    }
}