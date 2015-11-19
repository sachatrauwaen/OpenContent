using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Satrabel.OpenContent.Components.Uri;

namespace Satrabel.OpenContent.Components
{
    public class TemplateInfo
    {
        public TemplateInfo()
        {
            SettingsJson = "";
            DataJson = "";
            OutputString = "";
            this.TemplateManifest = null;
            this.Manifest = null;
            Files = null;
        }
        public FileUri Template { get; set; }
        public string SettingsJson { get; set; }
        public string DataJson { get; set; }
        public string OutputString { get; set; }
        public bool DataExist { get; set; }
        public IEnumerable<OpenContentInfo> DataList { get; set; }
        public int ItemId { get; set; }
        public int TabId { get; set; }
        public int ModuleId { get; set; }

        public bool IsOtherModule
        {
            get
            {
                return TabId > 0 && ModuleId > 0;
            }
        }
        public FileUri OtherModuleTemplate { get; set; }
        public string OtherModuleSettingsJson { get; set; }

        public ModuleInfo Module { get; set; }
        public TemplateManifest TemplateManifest { get; set; }
        public Manifest Manifest { get; set; }
        public TemplateFiles Files { get; set; }
    }
}