using System;
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
        private const string SettingsGoogleApiKey = "OpenContent_GoogleApiKey";
        private const string SettingsFastHandlebars = "OpenContent_FastHandlebars";
        private const bool SettingsDefaultFastHandlebars = false;

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

        public string GetGoogleApiKey()
        {
            return PortalController.GetPortalSetting(SettingsGoogleApiKey, _portalId, string.Empty);
        }
        public void SetGoogleApiKey(string googleMapsApiKey)
        {
            PortalController.UpdatePortalSetting(_portalId, SettingsGoogleApiKey, googleMapsApiKey, true);
        }

        public bool GetFastHandlebars()
        {
            var fastHandlebarsSetting = PortalController.GetPortalSetting(SettingsFastHandlebars, _portalId, string.Empty);
            bool fastHandlebars;
            if (!string.IsNullOrWhiteSpace(fastHandlebarsSetting) && bool.TryParse(fastHandlebarsSetting, out fastHandlebars))
                return fastHandlebars;
            return SettingsDefaultFastHandlebars;
        }

        public void SetFastHandlebars(bool fastHandlebars)
        {
            PortalController.UpdatePortalSetting(_portalId, SettingsFastHandlebars, fastHandlebars.ToString(), true);
        }
    }

    
}