using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentControllerFactory
    {
        
        public OpenContentGlobalSettingsController OpenContentGlobalSettingsController(int portalId)
        {
                return new OpenContentGlobalSettingsController(portalId);
        }

        private OpenContentControllerFactory() { }

        private static OpenContentControllerFactory _instance;
        public static OpenContentControllerFactory Instance
        {
            get
            {
                return _instance ?? (_instance = new OpenContentControllerFactory());
            }
        }
    }
}