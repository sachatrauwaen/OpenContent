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


    public class OpenContentModuleInfo : IOpenContentModuleInfo
    {
        private ModuleInfo _dataModule;
        private OpenContentSettings _settings;
        private readonly IDictionary _moduleSettings;
        private readonly PortalSettings _portalSettings;

        private OpenContentModuleInfo(ModuleInfo viewModule, PortalSettings ps)
        {
            ViewModule = viewModule;
            _moduleSettings = viewModule.ModuleSettings;
            _portalSettings = ps;
        }

        public static OpenContentModuleInfo Create(int moduleId, int tabId, PortalSettings ps)
        {
            ModuleController mc = new ModuleController();
            var viewModule = mc.GetModule(moduleId, tabId, false);
            return Create(viewModule, ps);
        }

        public static OpenContentModuleInfo Create(ModuleInfo viewModule, PortalSettings portalSettings)
        {
            var retval = new OpenContentModuleInfo(viewModule, portalSettings)
            {
                TabId = viewModule.TabID,
                ModuleId = viewModule.ModuleID,
               // PageUrl = DnnUrlUtils.NavigateUrl(viewModule.TabID),
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
            return Settings.DetailTabId > 0 ? Settings.DetailTabId : (Settings.TabId > 0 ? Settings.TabId : ViewModule.TabID);
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