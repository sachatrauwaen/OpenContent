using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public class FieldConfig
    {
        public FieldConfig(bool typeobject = false)
        {
            if (typeobject)
            {
                Fields = new Dictionary<string, FieldConfig>();
            }
        }

        [JsonProperty(PropertyName = "indexType", NullValueHandling = NullValueHandling.Ignore)]
        public string IndexType { get; set; }

        [JsonProperty(PropertyName = "index", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(false)]
        public bool Index { get; set; }

        [JsonProperty(PropertyName = "sort", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(false)]
        public bool Sort { get; set; }

        [JsonProperty(PropertyName = "multiLanguage", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(false)]
        public bool MultiLanguage { get; set; }

        [JsonProperty(PropertyName = "fields", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, FieldConfig> Fields { get; set; }

        [JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
        public FieldConfig Items { get; set; }

    }
}
