using System;
using DotNetNuke.Entities.Modules;
using System.Collections;
using System.Runtime.InteropServices;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Thin object to identify an OpenContentModule
    /// </summary>
    public class OpenContentModuleInfo
    {
        private ModuleInfo _moduleInfo;

        [Obsolete("This method is obsolete since aug 2017; use another constructor instead")]
        public OpenContentModuleInfo(ModuleInfo activeModule, Hashtable moduleSettings)
        {
            _moduleInfo = activeModule;
            ModuleId = activeModule.ModuleID;
            PortalId = activeModule.PortalID;
            TabId = activeModule.TabID;
        }

        public OpenContentModuleInfo(ModuleInfo activeModule)
        {
            _moduleInfo = activeModule;
            ModuleId = activeModule.ModuleID;
            PortalId = activeModule.PortalID;
            TabId = activeModule.TabID;
        }

        public OpenContentModuleInfo(int portalId, int tabId, int moduleId)
        {
            ModuleId = moduleId;
            PortalId = portalId;
            TabId = tabId;
        }

        public int ModuleId { get; }
        public int TabId { get; }
        public int PortalId { get; }
        public ModuleInfo ModuleInfo
        {
            get
            {
                if (_moduleInfo == null)
                {
                    _moduleInfo = DnnUtils.GetDnnModule(TabId, ModuleId);
                    if (_moduleInfo == null)
                    {
                        throw new Exception($"No Module found with tabId {TabId} and moduleId {ModuleId}. Review your 'OtherModule' settings");
                    }
                }
                return _moduleInfo;
            }
        }
        public string ModuleTitle => ModuleInfo.ModuleTitle;

        public int TabModuleId => ModuleInfo.TabModuleID;

    }
}