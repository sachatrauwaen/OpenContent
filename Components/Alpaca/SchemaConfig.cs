using System.Collections.Generic;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Alpaca
{
    public class SchemaConfig
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "enum")]
        public List<string> Enum { get; set; }
        [JsonProperty(PropertyName = "properties")]
        public Dictionary<string, SchemaConfig> Properties { get; set; }

    }
}