using DotNetNuke.Entities.Modules;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Satrabel.OpenContent.Components.Dnn;

namespace Satrabel.OpenContent.Components
{
    public interface IOpenContentModuleInfo
    {
    }

    public class OpenContentModuleInfo
    {
        public OpenContentModuleInfo(int portalId, int tabId, int moduleId, string moduleTitle, int tabModuleId)
        {
            ModuleId = moduleId;
            ModuleTitle = moduleTitle;
            TabModuleId = tabModuleId;
            PortalId = portalId;
            TabId = tabId;
        }

        public int ModuleId { get; }
        public int TabId { get; }
        public int PortalId { get; }
        public string ModuleTitle { get; }
        public int TabModuleId { get; }
    }

    public class OpenContentModuleConfig : IOpenContentModuleInfo
    {
        private OpenContentModuleInfo _dataModule;
        private OpenContentSettings _settings;
        private readonly IDictionary _moduleSettings;
        private readonly PortalSettings _portalSettings;

        private OpenContentModuleConfig(ModuleInfo viewModule, PortalSettings ps)
        {
            ViewModule = viewModule.CreateOpenContentModuleInfo();
            _moduleSettings = viewModule.ModuleSettings;
            _portalSettings = ps;
        }

        public static OpenContentModuleConfig Create(int moduleId, int tabId, PortalSettings ps)
        {
            var viewModule = DnnUtils.GetDnnModule(tabId, moduleId);
            return Create(viewModule, ps);
        }

        public static OpenContentModuleConfig Create(ModuleInfo viewModule, PortalSettings portalSettings)
        {
            return new OpenContentModuleConfig(viewModule, portalSettings);
        }

        public OpenContentModuleInfo ViewModule { get; }
        public OpenContentModuleInfo DataModule
        {
            get
            {
                if (Settings.ModuleId > 0 && _dataModule == null)
                {
                    _dataModule = DnnUtils.CreateOpenContentModuleInfo(Settings.TabId, Settings.ModuleId);
                }
                else if (_dataModule == null)
                {
                    _dataModule = ViewModule;
                }
                return _dataModule;
            }
        }

        public OpenContentSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new OpenContentSettings(ComponentSettingsInfo.Create(_moduleSettings));
                return _settings;
            }
        }

        #region Methods

        public int GetDetailTabId()
        {
            return Settings.DetailTabId > 0 ? Settings.DetailTabId : (Settings.TabId > 0 ? Settings.TabId : ViewModule.TabId);
        }

        public bool IsListMode()
        {
            return Settings.Template != null && Settings.Template.IsListTemplate;
        }

        public bool IsInRole(string editrole)
        {
            return _portalSettings.UserInfo.IsInRole(editrole);
        }

        public string GetUrl(int detailTabId, string getCurrentCultureCode)
        {
            return DnnUrlUtils.NavigateUrl(detailTabId, _portalSettings, getCurrentCultureCode);
        }

        internal string GetUrl(int detailTabId, string v1, string v2)
        {
            return DnnUrlUtils.NavigateUrl(detailTabId, _portalSettings, v1, v2);
        }

        public string EditUrl(string id, string itemId, int viewModuleModuleId)
        {
            return DnnUrlUtils.EditUrl(id, itemId, viewModuleModuleId, _portalSettings);
        }

        internal string EditUrl(int moduleId)
        {
            return DnnUrlUtils.EditUrl(moduleId, _portalSettings);
        }

        #endregion

        #region Properties

        public int TabId => ViewModule.TabId;
        public int ModuleId => ViewModule.ModuleId;
        public int UserId => _portalSettings.UserId;
        public IList<UserRoleInfo> UserRoles => _portalSettings.UserInfo.Social.Roles;
        public int PortalId => _portalSettings.PortalId;
        public string HomeDirectory => _portalSettings.HomeDirectory;
        public int ActiveTabId => _portalSettings.ActiveTab.TabID;
        public bool CanvasUnavailable => _portalSettings == null;
        public string HostName => _portalSettings.PortalAlias.HTTPAlias;
        public bool PreviewEnabled => (_portalSettings.UserMode == PortalSettings.Mode.View);

        #endregion
    }


}