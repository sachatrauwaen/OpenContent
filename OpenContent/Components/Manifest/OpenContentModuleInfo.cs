using DotNetNuke.Entities.Modules;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentModuleInfo
    {
        private ModuleInfo _dataModule;
        private OpenContentSettings _settings;

        public OpenContentModuleInfo(ModuleInfo viewModule)
        {
            ViewModule = viewModule;
            TabId = ViewModule.TabID;
            ModuleId = ViewModule.ModuleID;
        }

        public OpenContentModuleInfo(int moduleId, int tabId)
        {
            ModuleController mc = new ModuleController();
            ViewModule = mc.GetModule(moduleId, tabId, false);
            TabId = ViewModule.TabID;
            ModuleId = ViewModule.ModuleID;
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

        public int TabId { get; }
        public int ModuleId { get; }

        public OpenContentSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = ViewModule.OpenContentSettings();
                return _settings;
            }
        }
        public int GetDetailTabId()
        {
            return Settings.DetailTabId > 0 ? Settings.DetailTabId : (Settings.TabId > 0 ? Settings.TabId : ViewModule.TabID);
        }

        public bool IsListTemplate()
        {
            return Settings.Template != null && Settings.Template.IsListTemplate;
        }

    }
}