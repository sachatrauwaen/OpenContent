using System;
using DotNetNuke.Entities.Modules;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Thin object to identify an OpenContentModule
    /// </summary>
    public class OpenContentModuleInfo
    {
        [Obsolete("This method is obsolete since aug 2017; use another constructor instead")]
        public OpenContentModuleInfo(ModuleInfo activeModule)
        {
            ModuleId = activeModule.ModuleID;
            ModuleTitle = activeModule.ModuleTitle;
            TabModuleId = activeModule.TabModuleID;
            PortalId = activeModule.PortalID;
            TabId = activeModule.TabID;
        }

        public OpenContentModuleInfo(int portalId, int tabId, int moduleId, string moduleTitle, int tabModuleId)
        {
            ModuleId = moduleId;
            ModuleTitle = moduleTitle;
            TabModuleId = tabModuleId;
            PortalId = portalId;
            TabId = tabId;
        }

        public int ModuleId { get; }
        public int TabId { get; }
        public int PortalId { get; }
        public string ModuleTitle { get; }
        public int TabModuleId { get; }
    }
}