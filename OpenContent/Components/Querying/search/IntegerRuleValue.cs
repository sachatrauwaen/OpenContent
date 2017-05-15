namespace Satrabel.OpenContent.Components.Querying.Search
{
    public class IntegerRuleValue : RuleValue
    {
        private int Value;
        public IntegerRuleValue(int value)
        {
            Value = value;
        }
        public override int AsInteger
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