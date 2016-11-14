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

        public ModuleInfo ViewModule { get; private set; }

        public int TabID { get; private set; }
        public int ModuleID { get; private set; }
        public int TabModuleID { get; private set; }
        public string TemplateKey { get; private set; }
        public OpenContentSettings Settings { get; private set; }

    }
}