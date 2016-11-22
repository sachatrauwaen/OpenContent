namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class BooleanRuleValue : RuleValue
    {
        private bool Value;
        public BooleanRuleValue(bool value)
        {
            Value = value;
        }
        public override bool AsBoolean
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