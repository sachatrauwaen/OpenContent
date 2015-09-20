using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentSettings
    {
        public OpenContentSettings(Hashtable moduleSettings)
        {
            var template = moduleSettings["template"] as string;
            if (!string.IsNullOrEmpty(template))
            {
                Template = new FileUri(template);
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
        }

        public int TabId { get; set; }
        public int ModuleId { get; set; }
        public FileUri Template { get; set; }
        public string Data { get; set; }
        public bool IsOtherModule
        {
            get
            {
                return TabId > 0 && ModuleId > 0;
            }
        }
    }
}