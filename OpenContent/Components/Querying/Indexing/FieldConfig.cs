using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Indexing
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

        /// <summary>
        /// Gets or sets the type of the index.
        /// Can be any of text, date, time, datetime, boolean, float, int, double, html, key(used for: url, file or folder name, image)
        /// </summary>
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
