using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class TemplateManifest
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "main")]
        public TemplateFiles Main { get; set; }
        [JsonProperty(PropertyName = "detail")]
        public TemplateFiles Detail { get; set; }
        [JsonProperty(PropertyName = "clientSideData")]
        public bool ClientSideData { get; set; }

        public bool IsListTemplate
        {
            get
            {
                return Type == "multiple";
            }
        }

    }
}