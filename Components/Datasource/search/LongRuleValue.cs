using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class LongRuleValue : RuleValue
    {
        private long Value;
        public LongRuleValue(long value)
        {
            Value = value;
        }
        public override long AsLong
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