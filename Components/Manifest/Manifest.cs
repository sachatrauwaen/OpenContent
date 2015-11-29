using System.Collections.Generic;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class Manifest
    {
        [JsonProperty(PropertyName = "developmentPath")]
        public bool DevelopmentPath { get; set; }
        [JsonProperty(PropertyName = "editWitoutPostback")]
        public bool EditWitoutPostback { get; set; }
        [JsonProperty(PropertyName = "templates")]
        public Dictionary<string, TemplateManifest> Templates { get; set; }
        [JsonProperty(PropertyName = "additionalEditControl")]
        public string AdditionalEditControl { get; set; }
        [JsonProperty(PropertyName = "editRole")]
        public string EditRole { get; set; }

        public bool HasTemplates { get { return (Templates != null); } }
        public FolderUri ManifestDir { get; set; }

        public TemplateManifest GetTemplateManifest(TemplateKey templateKey)
        {
            if (Templates != null && Templates.ContainsKey(templateKey.Key))
            {
                return Templates[templateKey.Key];
            }
            return null;
        }
    }
}