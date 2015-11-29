using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class TemplateManifest
    {
        private FileUri _uri;
        private Manifest _manifest;

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "main")]
        public TemplateFiles Main { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public TemplateFiles Detail { get; set; }

        [JsonProperty(PropertyName = "clientSide")]
        public bool ClientSide { get; set; }

        public bool IsListTemplate
        {
            get
            {
                return Type == "multiple";
            }
        }

        public FolderUri ManifestDir { get; set; }

        public FileUri Uri
        {
            get
            {
                if (_uri == null)
                {
                    _uri = new FileUri(ManifestDir, Main.Template);
                }
                return _uri;
            }
        }
        public Manifest Manifest
        {
            get
            {
                if (_manifest == null)
                {
                    _manifest = ManifestUtils.GetManifest(ManifestDir);
                }
                return _manifest;
            }
        }
    }
}