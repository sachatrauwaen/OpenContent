using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class TemplateInfo
    {
        public TemplateInfo()
        {
            SettingsJson = "";
            DataJson = "";
            OutputString = "";
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
    }
}