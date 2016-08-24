using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestGroup
    {
        public RestGroup()
        {
            FilterRules = new List<RestRule>();
            FilterGroups = new List<RestGroup>();
        }
        [JsonProperty(PropertyName = "rules")]
        public List<RestRule> FilterRules { get; private set; }
        [JsonProperty(PropertyName = "groups")]
        public List<RestGroup> FilterGroups { get; private set; }
    }
}
