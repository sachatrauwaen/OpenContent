using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components
{
    public class PartialTemplate
    {
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }
    }
}