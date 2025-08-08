using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class CollectionDefManifest
    {
        [JsonProperty(PropertyName = "detailMetaTitle")]
        public string DetailMetaTitle { get; set; }

        [JsonProperty(PropertyName = "detailMetaDescription")]
        public string DetailMetaDescription { get; set; }

        [JsonProperty(PropertyName = "detailMeta")]
        public string DetailMeta { get; set; }

        [JsonProperty(PropertyName = "detailUrl")]
        public string DetailUrl { get; set; }
    }
}