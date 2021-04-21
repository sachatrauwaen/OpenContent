using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;


namespace Satrabel.OpenContent.Components
{
    public class OpenContentSettings
    {
        private readonly string _query;

        public OpenContentSettings(ComponentSettingsInfo moduleSettings)
        {
            FirstTimeInitialisation = string.IsNullOrEmpty(moduleSettings.Template);
            if (!FirstTimeInitialisation)
            {
                TemplateKey = new TemplateKey(new FileUri(moduleSettings.Template));
                TemplateManifest templateManifest;
                Manifest = ManifestUtils.GetManifest(TemplateKey, out templateManifest);
                Template = templateManifest;
            }
            PortalId = moduleSettings.PortalId;
            TabId = moduleSettings.TabId;
            ModuleId = moduleSettings.ModuleId;

            Data = moduleSettings.Data;
            _query = moduleSettings.Query;

            DetailTabId = moduleSettings.DetailTabId;
        }

        internal TemplateKey TemplateKey { get; }

        public int PortalId { get; }
        public int TabId { get; }

        /// <summary>
        /// Gets the module identifier which is the main moduleId.  0 if no other module
        /// </summary>
        public int ModuleId { get; }

        public TemplateManifest Template { get; }

        /// <summary>
        /// Gets the manifest. Will be Null if no template is defined yet
        /// </summary>
        public Manifest.Manifest Manifest { get; }

        public FolderUri TemplateDir => TemplateKey?.TemplateDir;

        public string Data { get; }

        public JObject Query => !string.IsNullOrEmpty(_query) ? JObject.Parse(_query) : new JObject();

        public bool IsOtherModule => TabId > 0 && ModuleId > 0;
        public bool IsOtherPortal => PortalId > 0 &&  TabId > 0 && ModuleId > 0;

        public bool TemplateAvailable => TemplateKey != null;

        public int DetailTabId { get; }

        public bool FirstTimeInitialisation { get; }
    }
}