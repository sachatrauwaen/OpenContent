using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class PartialTemplate
    {
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }
    }
}