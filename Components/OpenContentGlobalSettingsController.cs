using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentGlobalSettingsController
    {
        private readonly int _portalId;

        private const string SettingsKeyMaxVersions = "OpenContent_MaxVersions";
        private const int SettingsDefaultMaxVersions = 5;

        public OpenContentGlobalSettingsController(int portalId)
        {
            _portalId = portalId;
        }

        public int GetMaxVersions()
        {
            var maxVersionsSetting = PortalController.GetPortalSetting(SettingsKeyMaxVersions, _portalId, string.Empty);
            int maxVersions;
            if (!string.IsNullOrWhiteSpace(maxVersionsSetting) && int.TryParse(maxVersionsSetting, out maxVersions))
                return maxVersions;
            return SettingsDefaultMaxVersions;
        }

        public void SetMaxVersions(int maxVersions)
        {
            PortalController.UpdatePortalSetting(_portalId, "OpenContent_MaxVersions", maxVersions.ToString(), true);
        }

    }
}