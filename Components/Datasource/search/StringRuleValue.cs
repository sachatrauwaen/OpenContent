using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class StringRuleValue : RuleValue
    {
        private string Value;
        public StringRuleValue(string value)
        {
            Value = value;
        }
        public override string AsString
        {
            get
            {
                return Value;
            }
        }
    }
}