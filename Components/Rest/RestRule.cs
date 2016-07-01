using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestRule
    {
        public RestRule()
        {
            FieldOperator = OperatorEnum.EQUAL;
            FieldType = FieldTypeEnum.STRING;
        }
        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }
        [JsonProperty(PropertyName="type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FieldTypeEnum FieldType { get; set; }
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
