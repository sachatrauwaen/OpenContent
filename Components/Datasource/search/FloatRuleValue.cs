using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class FloatRuleValue : RuleValue
    {
        private float Value;
        public FloatRuleValue(float value)
        {
            Value = value;
        }
        public override float AsFloat
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