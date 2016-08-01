using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentControllerFactory
    {
        private OpenContentGlobalSettingsController _openContentGlobalSettingsController;
        public OpenContentGlobalSettingsController OpenContentGlobalSettingsController
        {
            get
            {
                return _openContentGlobalSettingsController ??
                (_openContentGlobalSettingsController = new OpenContentGlobalSettingsController(PortalSettings.Current.PortalId));
            }
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