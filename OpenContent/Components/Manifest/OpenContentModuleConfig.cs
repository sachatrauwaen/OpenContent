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

    public class OpenContentModuleConfig : IOpenContentModuleInfo
    {
        private OpenContentModuleInfo _dataModule;
        private OpenContentSettings _settings;
        private readonly IDictionary _moduleSettings;
        private readonly PortalSettings _portalSettings;

        private OpenContentModuleConfig(ModuleInfo viewModule, PortalSettings portalSettings)
        {
            ViewModule = new OpenContentModuleInfo(viewModule);
            PortalId = viewModule.PortalID;
            _moduleSettings = viewModule.ModuleSettings;
            _portalSettings = portalSettings;
        }

        public static OpenContentModuleConfig Create(int moduleId, int tabId, PortalSettings portalSettings)
        {
            var viewModule = DnnUtils.GetDnnModule(tabId, moduleId);
            return Create(viewModule, portalSettings);
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
                    _dataModule = new OpenContentModuleInfo(PortalId, Settings.TabId, Settings.ModuleId);
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

        internal string GetUrl(int detailTabId, string pagename, string idParam)
        {
            return DnnUrlUtils.NavigateUrl(detailTabId, _portalSettings, pagename, idParam);
        }

        public string EditUrl(string id, string itemId, int viewModuleModuleId)
        {
            return DnnUrlUtils.EditUrl(id, itemId, viewModuleModuleId, _portalSettings);
        }

        public string EditAddDataUrl(string id, string itemId, int viewModuleModuleId)
        {
            return DnnUrlUtils.EditAddDataUrl(id, itemId, viewModuleModuleId, _portalSettings);
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
        public int PortalId { get; }
        public string HomeDirectory => _portalSettings.HomeDirectory;
        public int ActiveTabId => _portalSettings.ActiveTab.TabID;
        public bool CanvasUnavailable => _portalSettings == null;
        public string HostName => _portalSettings.PortalAlias.HTTPAlias;
        public bool PreviewEnabled => _portalSettings != null && (_portalSettings.UserMode == PortalSettings.Mode.View);

        #endregion
    }


}