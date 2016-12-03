using System.Collections;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;


namespace Satrabel.OpenContent.Components
{
    public static class OpenContentSettingsExtentions
    {

        public static int GetModuleId(this OpenContentSettings settings, int defaultModuleId)
        {
            return settings.IsOtherModule ? settings.ModuleId : defaultModuleId;
        }


    }

    public class OpenContentSettings
    {
        private readonly string _query;

        public OpenContentSettings(IDictionary moduleSettings)
        {
            var template = moduleSettings["template"] as string;    //templatepath+file  or  //manifestpath+key
            FirstTimeInitialisation = string.IsNullOrEmpty(template);
            if (!FirstTimeInitialisation)
            {
                TemplateKey = new TemplateKey(new FileUri(template));
                TemplateManifest templateManifest;
                Manifest = ManifestUtils.GetManifest(TemplateKey, out templateManifest);
                Template = templateManifest;
            }

            var sTabId = moduleSettings["tabid"] as string;
            var sModuleId = moduleSettings["moduleid"] as string;
            TabId = -1;
            ModuleId = -1;
            if (sTabId != null && sModuleId != null)
            {
                TabId = int.Parse(sTabId);
                ModuleId = int.Parse(sModuleId);
            }

            Data = moduleSettings["data"] as string;
            _query = moduleSettings["query"] as string;
            var sDetailTabId = moduleSettings["detailtabid"] as string;
            DetailTabId = -1;
            if (!string.IsNullOrEmpty(sDetailTabId))
            {
                DetailTabId = int.Parse(sDetailTabId);
            }
        }

        internal TemplateKey TemplateKey { get; private set; }

        public int TabId { get; private set; }
        /// <summary>
        /// Gets the module identifier which is the main moduleId.  0 if no other module
        /// </summary>
        public int ModuleId { get; private set; }

        public TemplateManifest Template { get; private set; }

        /// <summary>
        /// Gets the manifest. Will be Null if no template is defined yet
        /// </summary>
        /// <value>
        /// The manifest.
        /// </value>
        public Manifest.Manifest Manifest { get; private set; }

        public FolderUri TemplateDir { get { return TemplateKey.TemplateDir; } }

        public string Data { get; private set; }

        public JObject Query
        {
            get
            {
                return !string.IsNullOrEmpty(_query) ? JObject.Parse(_query) : new JObject();
            }
        }

        public bool IsOtherModule
        {
            get
            {
                return TabId > 0 && ModuleId > 0;
            }
        }

        public bool TemplateAvailable { get { return TemplateKey != null; } }

        public int DetailTabId { get; private set; }
        public bool FirstTimeInitialisation { get; private set; }
    }
}