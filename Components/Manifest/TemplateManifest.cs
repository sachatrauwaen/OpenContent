using System.Security;
using Newtonsoft.Json;

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
        
        [JsonProperty(PropertyName = "clientSideData")]
        public bool ClientSideData { get; set; }
        
        public bool IsListTemplate
        {
            get
            {
                return Type == "multiple";
            }
        }

        public TemplateKey Key { get; private set; }
        public FolderUri ManifestDir { get; private set; }

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
            ManifestDir = templateKey.TemplateDir;
            Key = templateKey;
        }

    }
}