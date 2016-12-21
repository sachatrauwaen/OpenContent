using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class CollectionManifest
    {
        [JsonProperty(PropertyName = "schema")]
        public bool Schema { get; set; }
        [JsonProperty(PropertyName = "options")]
        public bool Options { get; set; }
        [JsonProperty(PropertyName = "createByUser")]
        public bool CreateByUser { get; set; }
        [JsonProperty(PropertyName = "includes")]
        public List<string> Includes { get; set; }
    }
}