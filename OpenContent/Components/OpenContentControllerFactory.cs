using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentControllerFactory
    {
        private OpenContentControllerFactory() { }
        private static OpenContentControllerFactory _instance;

        public static OpenContentControllerFactory Instance => _instance ?? (_instance = new OpenContentControllerFactory());


        private OpenContentGlobalSettingsController _globalSettingsController;
        public OpenContentGlobalSettingsController GlobalSettingsController
        {
            get
            {
                return _globalSettingsController ??
                (_globalSettingsController = new OpenContentGlobalSettingsController(PortalSettings.Current.PortalId));
            }
        }
    }
}