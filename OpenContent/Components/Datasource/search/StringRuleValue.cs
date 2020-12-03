using System;

namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class StringRuleValue : RuleValue
    {
        private readonly string _value;
        public StringRuleValue(string value)
        {
            _value = value;
        }
        public override string AsString => _value;

        public override float AsFloat => float.Parse(_value);
        public override int AsInteger => int.Parse(_value);
        public override bool AsBoolean => bool.Parse(_value);
        public override long AsLong => long.Parse(_value);

        public override DateTime AsDateTime => DateTime.Parse(_value, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }
}