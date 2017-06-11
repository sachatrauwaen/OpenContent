namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class BooleanRuleValue : RuleValue
    {
        private readonly bool _value;
        public BooleanRuleValue(bool value)
        {
            _value = value;
        }
        public override bool AsBoolean => _value;
        public override string AsString => _value.ToString();
    }
}