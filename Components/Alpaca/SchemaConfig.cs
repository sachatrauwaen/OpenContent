using System.Collections.Generic;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Alpaca
{
    public class SchemaConfig
    {
        public SchemaConfig(bool typeobject = false)
        {
            if (typeobject)
            {
                Type = "object";
                Properties = new Dictionary<string, SchemaConfig>();
            }
        }
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "enum", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Enum { get; set; }
        [JsonProperty(PropertyName = "properties", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SchemaConfig> Properties { get; set; }
        [JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
        public SchemaConfig Items { get; set; }

    }
}