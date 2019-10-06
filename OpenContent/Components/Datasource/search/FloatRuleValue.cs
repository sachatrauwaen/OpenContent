namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class FloatRuleValue : RuleValue
    {
        private readonly float _value;
        public FloatRuleValue(float value)
        {
            _value = value;
        }
        public override float AsFloat => _value;
        public override string AsString => _value.ToString();
    }
}