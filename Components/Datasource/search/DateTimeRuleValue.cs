using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
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