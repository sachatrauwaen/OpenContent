using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class PartialTemplate
    {
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }

        /// <summary>
        /// Specifies if the partialTemplate will be loaded serverside (rendered) of clientside (injected in source of the page)
        /// </summary>
        [JsonProperty(PropertyName = "clientSide")]
        public bool ClientSide { get; set; }

    }
}