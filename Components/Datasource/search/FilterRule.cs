using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class FilterRule
    {
        public FilterRule()
        {
            FieldOperator = OperatorEnum.EQUAL;
            FieldType = FieldTypeEnum.STRING;

        }
        public string Field { get; set; }
        public FieldTypeEnum FieldType { get; set; }
        public OperatorEnum FieldOperator { get; set; }
        public RuleValue Value { get; set; }
        public RuleValue LowerValue { get; set; }
        public RuleValue UpperValue { get; set; }
        public float Boost { get; set; }
        public IEnumerable<RuleValue> MultiValue { get; set; }
    }
}