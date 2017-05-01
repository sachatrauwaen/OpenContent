using DotNetNuke.Entities.Modules;
using System.Collections;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentModuleInfo
    {
        private ModuleInfo _dataModule;
        private OpenContentSettings _settings;
        private readonly IDictionary _moduleSettings;

        public OpenContentModuleInfo(ModuleInfo viewModule, IDictionary moduleSettings = null)
        {
            ViewModule = viewModule;
            if (moduleSettings == null)
                _moduleSettings = viewModule.ModuleSettings;
            else
                _moduleSettings = moduleSettings;
        }

        public OpenContentModuleInfo(int moduleId, int tabId)
        {
            ModuleController mc = new ModuleController();
            ViewModule = mc.GetModule(moduleId, tabId, false);
            _moduleSettings = ViewModule.ModuleSettings;
        }

        public ModuleInfo ViewModule { get; }
        public ModuleInfo DataModule
        {
            get
            {
                if (Settings.ModuleId > 0 && _dataModule == null)
                {
                    ModuleController mc = new ModuleController();
                    _dataModule = mc.GetModule(Settings.ModuleId, Settings.TabId, false);
                }
                else if (_dataModule == null)
                {
                    _dataModule = ViewModule;
                }
                return _dataModule;
            }
        }

        public int TabId => ViewModule.TabID;
        public int ModuleId => ViewModule.ModuleID;

        public OpenContentSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new OpenContentSettings(_moduleSettings);
                return _settings;
            }
        }

        public int GetDetailTabId()
        {
            return Settings.DetailTabId > 0 ? Settings.DetailTabId : (Settings.TabId > 0 ? Settings.TabId : ViewModule.TabID);
        }

        public bool IsListMode()
        {
            return Settings.Template != null && Settings.Template.IsListTemplate;
        }

    }
}