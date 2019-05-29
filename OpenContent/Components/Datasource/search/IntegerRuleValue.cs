namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class IntegerRuleValue : RuleValue
    {
        private readonly int _value;
        public IntegerRuleValue(int value)
        {
            _value = value;
        }
        public override int AsInteger => _value;
        public override string AsString => _value.ToString();
    }
}