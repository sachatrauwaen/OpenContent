using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;

namespace Satrabel.OpenContent.Components.Settings
{

    public class DnnGlobalSettingsRepository : IGlobalSettingsRepository
    {
        private readonly int _portalId;

        private const string SETTINGS_KEY_MAX_VERSIONS = "OpenContent_MaxVersions";
        private const int SETTINGS_DEFAULT_MAX_VERSIONS = 5;
        private const string SETTINGS_EDIT_LAYOUT = "OpenContent_EditLayout";
        private const AlpacaLayoutEnum SETTINGS_DEFAULT_EDIT_LAYOUT = AlpacaLayoutEnum.DNN;
        private const string SETTINGS_LOAD_BOOTSTRAP = "OpenContent_LoadBootstrap";
        private const bool SETTINGS_DEFAULT_LOAD_BOOTSTRAP = true;
        private const string SETTINGS_GOOGLE_API_KEY = "OpenContent_GoogleApiKey";
        private const string SETTINGS_LEGACY_HANDLEBARS = "OpenContent_LegacyHandlebars";
        private const bool SETTINGS_DEFAULT_LEGACY_HANDLEBARS = false;
        private const string SETTINGS_GITHUB_REPOSITORY = "OpenContent_GithubRepository";
        private const string DEFAULT_GITHUB_REPOSITORY = "sachatrauwaen/OpenContent-Templates";

        public DnnGlobalSettingsRepository(int portalId)
        {
            _portalId = portalId;
        }

        public int GetMaxVersions()
        {
            if (_portalId == -1) return SETTINGS_DEFAULT_MAX_VERSIONS;
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
            if (_portalId == -1) return SETTINGS_DEFAULT_EDIT_LAYOUT;
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
            if (_portalId == -1) return SETTINGS_DEFAULT_LOAD_BOOTSTRAP;
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
            if (_portalId == -1) return string.Empty;
            return PortalController.GetPortalSetting(SETTINGS_GOOGLE_API_KEY, _portalId, string.Empty);
        }
        public void SetGoogleApiKey(string googleMapsApiKey)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_GOOGLE_API_KEY, googleMapsApiKey, true);
        }

        public bool GetLegacyHandlebars()
        {
            if (_portalId == -1) return SETTINGS_DEFAULT_LEGACY_HANDLEBARS;
            var LegacyHandlebarsSetting = PortalController.GetPortalSetting(SETTINGS_LEGACY_HANDLEBARS, _portalId, string.Empty);
            bool LegacyHandlebars;
            if (!string.IsNullOrWhiteSpace(LegacyHandlebarsSetting) && bool.TryParse(LegacyHandlebarsSetting, out LegacyHandlebars))
                return LegacyHandlebars;
            return SETTINGS_DEFAULT_LEGACY_HANDLEBARS;
        }

        public void SetLegacyHandlebars(bool LegacyHandlebars)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_LEGACY_HANDLEBARS, LegacyHandlebars.ToString(), true);
        }

        private const string SETTINGS_AUTO_ATTACH = "OpenContent_AutoAttach";
        private const bool SETTINGS_DEFAULT_AUTO_ATTACH = false;
        public bool GetAutoAttach()
        {
            if (_portalId == -1) return SETTINGS_DEFAULT_AUTO_ATTACH;
            var setting = PortalController.GetPortalSetting(SETTINGS_AUTO_ATTACH, _portalId, string.Empty);
            bool result;
            if (!string.IsNullOrWhiteSpace(setting) && bool.TryParse(setting, out result))
                return result;
            return SETTINGS_DEFAULT_AUTO_ATTACH;
        }

        public void SetAutoAttach(string value)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_AUTO_ATTACH, value, true);
        }

        private const string SETTINGS_LOGGING = "OpenContent_Logging";
        private const string SETTINGS_DEFAULT_LOGGING = "none";
        public string GetLoggingScope()
        {
            if (_portalId == -1) return SETTINGS_DEFAULT_LOGGING;
            return PortalController.GetPortalSetting(SETTINGS_LOGGING, _portalId, SETTINGS_DEFAULT_LOGGING);
        }

        public void SetLoggingScope(string value)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_LOGGING, value, true);
        }


        private const string SETTINGS_EDITOR_ROLE_ID = "OpenContent_EditorsRoleId";
        public string GetEditorRoleId()
        {
            if (_portalId == -1) return "";
            return PortalController.GetPortalSetting(SETTINGS_EDITOR_ROLE_ID, _portalId, "");
        }

        public void SetEditorRoleId(string value)
        {
            if (string.IsNullOrEmpty(value))
                PortalController.DeletePortalSetting(_portalId, SETTINGS_EDITOR_ROLE_ID);
            else
                PortalController.UpdatePortalSetting(_portalId, SETTINGS_EDITOR_ROLE_ID, value, true);
        }


        private const string SETTINGS_SAVE_XML = "OpenContent_SaveXml";
        private const bool SETTINGS_DEFAULT_SAVE_XML = false;
        public bool IsSaveXml()
        {
            var saveXmlSetting = PortalController.GetPortalSetting(SETTINGS_SAVE_XML, _portalId, string.Empty);
            bool saveXml;
            if (!string.IsNullOrWhiteSpace(saveXmlSetting) && bool.TryParse(saveXmlSetting, out saveXml))
                return saveXml;
            return SETTINGS_DEFAULT_SAVE_XML;
        }

        public void SetSaveXml(bool saveXml)
        {
            PortalController.UpdatePortalSetting(_portalId, SETTINGS_SAVE_XML, saveXml.ToString(), true);
        }

        public string GetGithubRepository()
        {
            if (_portalId == -1) return "";
            return PortalController.GetPortalSetting(SETTINGS_GITHUB_REPOSITORY, _portalId, DEFAULT_GITHUB_REPOSITORY);
        }

        public void SetGithubRepository(string value)
        {
            if (string.IsNullOrEmpty(value))
                PortalController.DeletePortalSetting(_portalId, SETTINGS_GITHUB_REPOSITORY);
            else
                PortalController.UpdatePortalSetting(_portalId, SETTINGS_GITHUB_REPOSITORY, value, true);
        }
    }
}