using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestSort
    {
        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public bool Descending { get; set; }
    }
}
