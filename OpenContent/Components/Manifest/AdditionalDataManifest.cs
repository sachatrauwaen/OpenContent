using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public enum RelatedDataSourceType
    {
        AdditionalData,
        MainData
    }

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

        [JsonProperty(PropertyName = "dataModuleId")]
        public int DataModuleId { get; set; }

        [JsonProperty(PropertyName = "dataTabId")]
        public int DataTabId { get; set; }

        public RelatedDataSourceType SourceRelatedDataSource => DataModuleId > 0 ? RelatedDataSourceType.MainData : RelatedDataSourceType.AdditionalData;
    }
}