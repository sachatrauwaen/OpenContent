using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components
{
    public class ImageStatus
    {
        [JsonProperty(PropertyName = "default")]
        public string Default { get; set; }
    }
}