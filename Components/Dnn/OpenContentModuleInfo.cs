using DotNetNuke.Entities.Modules;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentModuleInfo
    {

        public OpenContentModuleInfo(ModuleInfo viewModule)
        {
            ViewModule = viewModule;
            Settings = new OpenContentSettings(viewModule.ModuleSettings);
            TemplateKey = Settings.Template == null ? string.Empty : Settings.Template.Key.ToString();
            TabID = viewModule.TabID;
            ModuleID = viewModule.ModuleID;
            TabModuleID = viewModule.TabModuleID;
        }

        public OpenContentModuleInfo(int moduleId, int tabId)
        {
            ModuleController mc = new ModuleController();
            ViewModule = mc.GetModule(moduleId, tabId, false);
            TabID = ViewModule.TabID;
            ModuleID = ViewModule.ModuleID;
            TabModuleID = ViewModule.TabModuleID;
        }

        public ModuleInfo ViewModule { get;  }

        public int TabID { get; }
        public int ModuleID { get; }
        public int TabModuleID { get; }
        public string TemplateKey { get; }
        public OpenContentSettings Settings { get; }

    }
}