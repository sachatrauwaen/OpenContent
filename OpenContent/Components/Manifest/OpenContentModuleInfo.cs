using DotNetNuke.Entities.Modules;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Dnn;
using System;

namespace Satrabel.OpenContent.Components
{
    public interface IOpenContentModuleInfo
    {
    }

    public class OpenContentModuleInfo : IOpenContentModuleInfo
    {
        private ModuleInfo _dataModule;
        private OpenContentSettings _settings;
        private readonly IDictionary _moduleSettings;
        private readonly PortalSettings _portalSettings;

        public OpenContentModuleInfo(ModuleInfo viewModule, PortalSettings ps)
        {
            ViewModule = viewModule;
            _moduleSettings = viewModule.ModuleSettings;
            _portalSettings = ps;
        }

        public OpenContentModuleInfo(int moduleId, int tabId, PortalSettings ps)
        {
            ModuleController mc = new ModuleController();
            ViewModule = mc.GetModule(moduleId, tabId, false);
            _moduleSettings = ViewModule.ModuleSettings;
            _portalSettings = ps;
        }

        public ModuleInfo ViewModule { get; }
        public ModuleInfo DataModule
        {
            get
            {
                if (Settings.ModuleId > 0 && _dataModule == null)
                {
                    ModuleController mc = new ModuleController();
                    _dataModule = mc.GetModule(Settings.ModuleId, Settings.TabId, false);
                }
                else if (_dataModule == null)
                {
                    _dataModule = ViewModule;
                }
                return _dataModule;
            }
        }

        public int TabId => ViewModule.TabID;
        public int ModuleId => ViewModule.ModuleID;

        public string PageUrl => DnnUrlUtils.NavigateUrl(ViewModule.TabID);
        public int UserId => _portalSettings.UserId;
        public IList<UserRoleInfo> UserRoles => _portalSettings.UserInfo.Social.Roles;
        public int PortalId => _portalSettings.PortalId;
        public string HomeDirectory => _portalSettings.HomeDirectory;

        public string GetUrl(int detailTabId, string getCurrentCultureCode)
        {
            return DnnUrlUtils.NavigateUrl(detailTabId, _portalSettings, getCurrentCultureCode);
        }

        public bool IsInRole(string editrole)
        {
            return _portalSettings.UserInfo.IsInRole(editrole);
        }

        public OpenContentSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new OpenContentSettings(_moduleSettings);
                return _settings;
            }
        }

        public int ActiveTabId => _portalSettings.ActiveTab.TabID;
        public bool CanvasUnavailable => _portalSettings == null;
        public string HostName => _portalSettings.PortalAlias.HTTPAlias;
        public bool PreviewEnabled => (_portalSettings.UserMode == PortalSettings.Mode.View);

        public int GetDetailTabId()
        {
            return Settings.DetailTabId > 0 ? Settings.DetailTabId : (Settings.TabId > 0 ? Settings.TabId : ViewModule.TabID);
        }

        public bool IsListMode()
        {
            return Settings.Template != null && Settings.Template.IsListTemplate;
        }

        public string NavigateUrl(int detailTabId, string getCurrentCultureCode)
        {
            return DnnUrlUtils.NavigateUrl(detailTabId, _portalSettings, getCurrentCultureCode);
        }

        public string EditUrl(string id, string itemId, int viewModuleModuleId)
        {
            return DnnUrlUtils.EditUrl(id, itemId, viewModuleModuleId, _portalSettings);
        }

        internal string NavigateUrl(int detailTabId, string v1, string v2)
        {
            return DnnUrlUtils.NavigateUrl(detailTabId, _portalSettings, v1, v2);
        }

        internal string EditUrl(int moduleId)
        {
            return DnnUrlUtils.EditUrl(moduleId, _portalSettings);
        }
    }


}