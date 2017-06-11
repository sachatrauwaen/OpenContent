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
    }
}