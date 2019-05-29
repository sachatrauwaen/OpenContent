namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class LongRuleValue : RuleValue
    {
        private readonly long _value;
        public LongRuleValue(long value)
        {
            _value = value;
        }
        public override long AsLong => _value;
        public override string AsString => _value.ToString();
    }
}