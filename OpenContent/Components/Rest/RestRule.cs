using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Satrabel.OpenContent.Components.Querying.Search;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestRule
    {
        public RestRule()
        {
            FieldOperator = OperatorEnum.EQUAL;
        }
        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "operator")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OperatorEnum FieldOperator { get; set; }

        [JsonProperty(PropertyName = "value")]
        public JValue Value { get; set; }

        [JsonProperty(PropertyName = "lowerValue")]
        public JValue LowerValue { get; set; }

        [JsonProperty(PropertyName = "upperValue")]
        public JValue UpperValue { get; set; }
        //public float Boost { get; set; }

        [JsonProperty(PropertyName = "multiValue")]
        public List<JValue> MultiValue { get; set; }
    }
}
