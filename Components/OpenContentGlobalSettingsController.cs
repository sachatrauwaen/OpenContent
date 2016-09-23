using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Alpaca;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentGlobalSettingsController
    {
        private readonly int _portalId;

        private const string SettingsKeyMaxVersions = "OpenContent_MaxVersions";
        private const int SettingsDefaultMaxVersions = 5;
        private const string SettingsEditLayout = "OpenContent_EditLayout";
        private const AlpacaLayoutEnum SettingsDefaultEditLayout = AlpacaLayoutEnum.DNN;
        private const string SettingsLoadBootstrap = "OpenContent_LoadBootstrap";
        private const bool SettingsDefaultLoadBootstrap = true;

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

        public AlpacaLayoutEnum GetEditLayout()
        {
            var editLayoutSetting = PortalController.GetPortalSetting(SettingsEditLayout, _portalId, string.Empty);
            int editLayout;
            if (!string.IsNullOrWhiteSpace(editLayoutSetting) && int.TryParse(editLayoutSetting, out editLayout))
                return (AlpacaLayoutEnum)editLayout;
            return SettingsDefaultEditLayout;
        }

        public void SetEditLayout(AlpacaLayoutEnum layout)
        {
            PortalController.UpdatePortalSetting(_portalId, "OpenContent_EditLayout", ((int)layout).ToString(), true);
        }
        public bool GetLoadBootstrap()
        {
            var loadBootstrapSetting = PortalController.GetPortalSetting(SettingsLoadBootstrap, _portalId, string.Empty);
            bool loadBootstrap;
            if (!string.IsNullOrWhiteSpace(loadBootstrapSetting) && bool.TryParse(loadBootstrapSetting, out loadBootstrap))
                return loadBootstrap;
            return SettingsDefaultLoadBootstrap;
        }

        public void SetLoadBootstrap(bool loadBootstrap)
        {
            PortalController.UpdatePortalSetting(_portalId, SettingsLoadBootstrap, loadBootstrap.ToString(), true);
        }
    }

    
}