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

        [JsonProperty(PropertyName = "socialGroupFilter")]
        public bool SocialGroupFilter { get; set; }

        [JsonProperty(PropertyName = "journalTypeName")]
        public string JournalTypeName { get; set; }

        [JsonProperty(PropertyName = "journalAudience")]
        public string JournalAudience { get; set; }

        [JsonProperty(PropertyName = "journalContentTitle")]
        public string JournalContentTitle { get; set; }

        [JsonProperty(PropertyName = "journalContent")]
        public string JournalContent { get; set; }

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

        [JsonProperty(PropertyName = "mainMeta")]
        public string MainMeta { get; set; }

        [JsonProperty(PropertyName = "additionalData")]
        public Dictionary<string, AdditionalDataManifest> AdditionalDataDefinition
        {
            get
            {
                return _additionalData;
            }
            set
            {
                _additionalData = new Dictionary<string, AdditionalDataManifest>(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        [JsonProperty(PropertyName = "dataSource")]
        public string DataSource { get; set; }

        [JsonProperty(PropertyName = "dataSourceConfig")]
        public JObject DataSourceConfig { get; set; }

        [JsonProperty(PropertyName = "disableEdit")]
        public bool DisableEdit { get; set; }

        [JsonProperty(PropertyName = "permissions")]
        public JObject Permissions { get; set; }

        [JsonProperty(PropertyName = "advanced")]
        public bool Advanced { get; set; }

        public bool HasTemplates => (Templates != null);
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
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!AdditionalDataDefinition.ContainsKey(key.ToLowerInvariant()))
                throw new Exception($"Manifest in {this.ManifestDir.UrlFolder} does not contain a AdditionalData definition with key [{key}]");

            return AdditionalDataDefinition[key.ToLowerInvariant()];
        }

        public bool AdditionalDataDefined(string key = "")
        {
            if (key == "")
                return AdditionalDataDefinition != null;
            else
                return AdditionalDataDefinition.ContainsKey(key.ToLowerInvariant());
        }
    }
}