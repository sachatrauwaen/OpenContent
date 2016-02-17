using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Manifest;


namespace Satrabel.OpenContent.Components
{
    public class OpenContentSettings
    {
        public OpenContentSettings(IDictionary moduleSettings)
        {
            var template = moduleSettings["template"] as string;    //templatepath+file  or  //manifestpath+key
            if (!string.IsNullOrEmpty(template))
            {
                var templateUri = new FileUri(template);
                TemplateKey = new TemplateKey(templateUri);
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
            Query = moduleSettings["query"] as string;
            var sDetailTabId = moduleSettings["detailtabid"] as string;
            DetailTabId = -1;
            if (!string.IsNullOrEmpty(sDetailTabId))
            {
                DetailTabId = int.Parse(sDetailTabId);
            }
        }

        internal TemplateKey TemplateKey { get; private set; }


        public int TabId { get; private set; }
        internal int ModuleId { get; private set; }

        public TemplateManifest Template { get; private set; }
        public Manifest.Manifest Manifest { get; private set; }

        public FolderUri TemplateDir { get { return TemplateKey.TemplateDir; } }
        //public TemplateKey TemplateKey { get { return Template == null ? "" : Template.FileNameWithoutExtension; } }

        //internal FileUri Template { get; private set; }

        public string Data { get; private set; }
        public string Query { get; private set; }
        public bool IsOtherModule
        {
            get
            {
                return TabId > 0 && ModuleId > 0;
            }
        }

        public bool TemplateAvailable { get { return TemplateKey != null; } }



        public int DetailTabId { get; private set; }
    }
}