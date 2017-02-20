using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class TemplateManifest
    {

        private Manifest _manifest;

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "main")]
        public TemplateFiles Main { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public TemplateFiles Detail { get; set; }

        [JsonProperty(PropertyName = "views")]
        public Dictionary<string, TemplateFiles> Views { get; set; }

        /// <summary>
        /// Indicated that WEBAPI calls will be executed to fetch the data.
        /// </summary>
        [JsonProperty(PropertyName = "clientSideData")]
        public bool ClientSideData { get; set; }

        [JsonProperty(PropertyName = "collection")]
        public string Collection { get; set; } = "Items";

        public bool IsListTemplate
        {
            get
            {
                return Type == "multiple";
            }
        }

        public TemplateKey Key { get; private set; }
        public FolderUri ManifestFolderUri { get; private set; }

        public Manifest Manifest
        {
            get
            {
                if (_manifest == null)
                {
                    _manifest = ManifestUtils.GetManifest(Key);
                }
                return _manifest;
            }
        }

        public void SetSource(TemplateKey templateKey)
        {
            ManifestFolderUri = templateKey.TemplateDir;
            Key = templateKey;
        }
    }
}