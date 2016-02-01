using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Alpaca
{
    public class OptionsConfig
    {
        public OptionsConfig(bool typeobject = false)
        {
            if (typeobject)
            {
                Fields = new Dictionary<string, OptionsConfig>();
            }
        }
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "fields", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, OptionsConfig> Fields { get; set; }
        [JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
        public OptionsConfig Items { get; set; }
        [JsonProperty(PropertyName = "removeDefaultNone", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RemoveDefaultNone { get; set; }
        [JsonProperty(PropertyName = "dataService", NullValueHandling = NullValueHandling.Ignore)]
        public JObject DataService { get; set; }
        

    }
}
