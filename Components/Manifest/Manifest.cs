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

        [JsonProperty(PropertyName = "index")]
        public bool Index { get; set; }

        [JsonProperty(PropertyName = "detailMetaTitle")]
        public string DetailMetaTitle { get; set; }

        [JsonProperty(PropertyName = "detailMetaDescription")]
        public string DetailMetaDescription { get; set; }

        [JsonProperty(PropertyName = "detailMeta")]
        public string DetailMeta { get; set; }

        [JsonProperty(PropertyName = "detailUrl")]
        public string DetailUrl { get; set; }

        [JsonProperty(PropertyName = "additionalData")]
        public Dictionary<string, AdditionalDataManifest> AdditionalData { get; set; }
 
        public bool HasTemplates { get { return (Templates != null); } }
        public FolderUri ManifestDir { get; set; }
        public TemplateManifest GetTemplateManifest(FileUri template)
        {
            if (Templates != null && Templates.ContainsKey(template.FileNameWithoutExtension))
            {
                return Templates[template.FileNameWithoutExtension];
            }
            return null;
        }
        public TemplateManifest GetTemplateManifest(TemplateKey templateKey)
        {
            if (Templates != null && Templates.ContainsKey(templateKey.ShortKey))
            {
                return Templates[templateKey.ShortKey];
            }
            return null;
        }
        public TemplateManifest GetTemplateManifest(string templateKey)
        {
            if (Templates != null && Templates.ContainsKey(templateKey))
            {
                return Templates[templateKey];
            }
            return null;
        }
    }
}