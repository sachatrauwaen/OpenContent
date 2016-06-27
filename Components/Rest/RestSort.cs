using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Satrabel.OpenContent.Components.Datasource.search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestSort
    {
        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }
        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FieldTypeEnum FieldType { get; set; }
        [JsonProperty(PropertyName = "descending")]
        public bool Descending { get; set; }
    }
}
