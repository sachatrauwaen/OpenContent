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
            ModuleController mc = new ModuleController();
            var viewModule = mc.GetModule(moduleId, tabId, false);
            return Create(viewModule, ps);
        }

        public static OpenContentModuleConfig Create(ModuleInfo viewModule, PortalSettings portalSettings)
        {
            var retval = new OpenContentModuleConfig(viewModule, portalSettings)
            {
                TabId = viewModule.TabID,
                ModuleId = viewModule.ModuleID,
                UserId = portalSettings.UserId,
                UserRoles = portalSettings.UserInfo.Social.Roles,
                PortalId = portalSettings.PortalId,
                HomeDirectory = portalSettings.HomeDirectory,
                ActiveTabId = portalSettings.ActiveTab.TabID,
                CanvasUnavailable = portalSettings == null,
                HostName = portalSettings.PortalAlias.HTTPAlias,
                PreviewEnabled = (portalSettings.UserMode == PortalSettings.Mode.View),
            };
            return retval;
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

        #region Properties

        public int TabId { get; set; }
        public int ModuleId { get; set; }
        public int UserId { get; set; }
        public IList<UserRoleInfo> UserRoles { get; set; }
        public int PortalId { get; set; }
        public string HomeDirectory { get; set; }
        public int ActiveTabId { get; set; }
        public bool CanvasUnavailable { get; set; }
        public string HostName { get; set; }
        public bool PreviewEnabled { get; set; }

        #endregion
    }


}