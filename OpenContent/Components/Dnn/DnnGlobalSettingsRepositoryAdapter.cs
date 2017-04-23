using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;

namespace Satrabel.OpenContent.Components.Dnn
{
    public interface IGlobalSettingsRepositoryAdapter
    {
        int GetMaxVersions();
        void SetMaxVersions(int maxVersions);
        AlpacaLayoutEnum GetEditLayout();
        void SetEditLayout(AlpacaLayoutEnum editLayout);
        bool GetLoadBootstrap();
        void SetLoadBootstrap(bool @checked);
        string GetGoogleApiKey();
        void SetGoogleApiKey(string text);
        bool GetFastHandlebars();
        void SetFastHandlebars(bool @checked);
    }

    public class DnnGlobalSettingsRepositoryAdapter : IGlobalSettingsRepositoryAdapter
    {
        private readonly int _portalId;

        private const string SETTINGS_KEY_MAX_VERSIONS = "OpenContent_MaxVersions";
        private const int SETTINGS_DEFAULT_MAX_VERSIONS = 5;
        private const string SETTINGS_EDIT_LAYOUT = "OpenContent_EditLayout";
        private const AlpacaLayoutEnum SETTINGS_DEFAULT_EDIT_LAYOUT = AlpacaLayoutEnum.DNN;
        private const string SETTINGS_LOAD_BOOTSTRAP = "OpenContent_LoadBootstrap";
        private const bool SETTINGS_DEFAULT_LOAD_BOOTSTRAP = true;
        private const string SETTINGS_GOOGLE_API_KEY = "OpenContent_GoogleApiKey";
        private const string SETTINGS_FAST_HANDLEBARS = "OpenContent_FastHandlebars";
        private const bool SETTINGS_DEFAULT_FAST_HANDLEBARS = false;

        public DnnGlobalSettingsRepositoryAdapter(int portalId)
        {
            _portalId = portalId;
        }

        public int GetMaxVersions()
        {
            var maxVersionsSetting = PortalController.GetPortalSetting(SETTINGS_KEY_MAX_VERSIONS, _portalId, string.Empty);
            int maxVersions;
            if (!string.IsNullOrWhiteSpace(maxVersionsSetting) && int.TryParse(maxVersionsSetting, out maxVersions))
                return maxVersions;
            return SETTINGS_DEFAULT_MAX_VERSIONS;
        }

        public void SetMaxVersions(int maxVersions)
        {
            PortalController.UpdatePortalSetting(_portalId, "OpenContent_MaxVersions", maxVersions.ToString(), true);
        }

        public AlpacaLayoutEnum GetEditLayout()
        {
            var editLayoutSetting = PortalController.GetPortalSetting(SETTINGS_EDIT_LAYOUT, _portalId, string.Empty);
            int editLayout;
            if (!string.IsNullOrWhiteSpace(editLayoutSetting) && int.TryParse(editLayoutSetting, out editLayout))
                return (AlpacaLayoutEnum)editLayout;
            return SETTINGS_DEFAULT_EDIT_LAYOUT;
        }

        public void SetEditLayout(AlpacaLayoutEnum layout)
        {
            PortalController.UpdatePortalSetting(_portalId, "OpenContent_EditLayout", ((int)layout).ToString(), true);
        }
        public bool GetLoadBootstrap()
        {
            var loadBootstrapSetting = PortalController.GetPortalSetting(SETTINGS_LOAD_BOOTSTRAP, _portalId, string.Empty);
            bool loadBootstrap;
            if (!string.IsNullOrWhiteSpace(loadBootstrapSetting) && bool.TryParse(loadBootstrapSetting, out loadBootstrap))
                return loadBootstrap;
            return SETTINGS_DEFAULT_LOAD_BOOTSTRAP;
        }

        public void SetLoadBootstrap(bool loadBootstrap)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_LOAD_BOOTSTRAP, loadBootstrap.ToString(), true);
        }

        public string GetGoogleApiKey()
        {
            return PortalController.GetPortalSetting(SETTINGS_GOOGLE_API_KEY, _portalId, string.Empty);
        }
        public void SetGoogleApiKey(string googleMapsApiKey)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_GOOGLE_API_KEY, googleMapsApiKey, true);
        }

        public bool GetFastHandlebars()
        {
            var fastHandlebarsSetting = PortalController.GetPortalSetting(SETTINGS_FAST_HANDLEBARS, _portalId, string.Empty);
            bool fastHandlebars;
            if (!string.IsNullOrWhiteSpace(fastHandlebarsSetting) && bool.TryParse(fastHandlebarsSetting, out fastHandlebars))
                return fastHandlebars;
            return SETTINGS_DEFAULT_FAST_HANDLEBARS;
        }

        public void SetFastHandlebars(bool fastHandlebars)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_FAST_HANDLEBARS, fastHandlebars.ToString(), true);
        }
    }
}