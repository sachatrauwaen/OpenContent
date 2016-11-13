using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class AdditionalDataManifest
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "scope")]
        public string ScopeType { get; set; }
        [JsonProperty(PropertyName = "storageKey")]
        public string StorageKey { get; set; }
        [JsonProperty(PropertyName = "modelKey")]
        public string ModelKey { get; set; }
        [JsonProperty(PropertyName = "templateFolder")]
        public string TemplateFolder { get; set; }
    }
}