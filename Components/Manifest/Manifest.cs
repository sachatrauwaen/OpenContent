using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class Manifest
    {
        private Dictionary<string, AdditionalDataManifest> _additionalData;

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
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
        public Dictionary<string, AdditionalDataManifest> AdditionalData
        {
            get { return _additionalData; }
            set { _additionalData = new Dictionary<string, AdditionalDataManifest>(value, StringComparer.OrdinalIgnoreCase); }
        }

        [JsonProperty(PropertyName = "dataSource")]
        public string DataSource { get; set; }

        [JsonProperty(PropertyName = "dataSourceConfig")]
        public JObject DataSourceConfig { get; set; }

        //

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
        public AdditionalDataManifest GetAdditionalData(string key)
        {
            return AdditionalData[key.ToLowerInvariant()];
        }

        public bool AdditionalDataExists(string key = "")
        {
            if (key == "")
                return AdditionalData != null;
            else
                return AdditionalData.ContainsKey(key.ToLowerInvariant());
        }
    }
}