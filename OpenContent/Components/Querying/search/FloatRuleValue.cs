namespace Satrabel.OpenContent.Components.Querying.Search
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