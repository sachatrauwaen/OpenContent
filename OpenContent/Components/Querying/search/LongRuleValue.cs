namespace Satrabel.OpenContent.Components.Querying.Search
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