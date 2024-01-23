using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Satrabel.OpenContent.Components.Datasource.Search;
using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestGroup
    {
        public RestGroup()
        {
            FilterRules = new List<RestRule>();
            FilterGroups = new List<RestGroup>();
            Condition = ConditionEnum.AND;
        }
        [JsonProperty(PropertyName = "rules")]
        public List<RestRule> FilterRules { get; private set; }
        [JsonProperty(PropertyName = "groups")]
        public List<RestGroup> FilterGroups { get; private set; }

        [JsonProperty(PropertyName = "condition")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ConditionEnum Condition { get; set; }
    }
}
