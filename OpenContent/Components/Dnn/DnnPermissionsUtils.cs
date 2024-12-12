using System;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;

namespace Satrabel.OpenContent.Components.Dnn
{
    public static class DnnPermissionsUtils
    {
        public static bool HasAllUsersViewPermissions(this OpenContentModuleConfig module)
        {
            bool blnHasModuleViewPermissions = false;
            if (module.ViewModule != null)
            {
                //DNN already checks SuperUser and Administrator
                blnHasModuleViewPermissions = ModulePermissionController.HasModuleAccess(AppDefinitions.SecurityAccessLevel.View.ToDnnSecurityAccessLevel(), "CONTENT", DnnUtils.GetDnnModule(module.ViewModule));
            }
            return blnHasModuleViewPermissions;
        }

        public static bool HasEditPermissions(OpenContentModuleConfig ocModuleConfig, string editrole, bool editroleAllItems, int createdByUserId)
        {
            return ocModuleConfig.ViewModule.HasEditRightsOnModule() || HasEditRole(ocModuleConfig, editrole, editroleAllItems, createdByUserId);
        }

        /// <summary>
        /// Determines whether current logged in user has edit rights on module.
        /// </summary>
        /// <remarks></remarks>
        public static bool HasEditRightsOnModule(this OpenContentModuleInfo activeModule)
        {
            bool blnHasModuleEditPermissions = false;
            if (activeModule != null)
            {
                //DNN already checks SuperUser and Administrator
                var moduleInfo = DnnUtils.GetDnnModule(activeModule);
                if (moduleInfo == null)
                {
                    App.Services.Logger.Error($"Module {activeModule.ModuleId} of tab {activeModule.TabId} was not found while in HasEditRightsOnModule(). StackTrace: {Environment.StackTrace}");
                    throw new Exception($"(Other)Module was not found. Check your Switch Template config. See log for more info.");
                }
                blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", moduleInfo);
            }
            return blnHasModuleEditPermissions;
        }

        public static bool HasEditRole(OpenContentModuleConfig ocModuleConfig, string editrole, bool editroleAllItems, int createdByUserId)
        {
            if (String.IsNullOrEmpty(editrole)) return false;
            if (editrole.ToLower() == "all") return true;
            if (ocModuleConfig.IsInRole(editrole) && (editroleAllItems || createdByUserId == -1 || createdByUserId == ocModuleConfig.UserId)) return true;
            return false;
        }

        public static bool CheckIfEditable(this OpenContentModuleInfo dataModule, OpenContentModuleConfig ocModuleConfig)
        {
            bool isEditable;
            //first check some weird Dnn issue
            if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
            {
                var personalization = (PersonalizationInfo)HttpContext.Current.Items["Personalization"];
                if (personalization != null && personalization.UserId == -1)
                {
                    //this should never happen. 
                    //Let us make sure that the wrong value is no longer cached 
                    HttpContext.Current.Items.Remove("Personalization");
                }
            }
            bool blnPreview = ocModuleConfig.PreviewEnabled;
            if (Globals.IsHostTab(ocModuleConfig.TabId))
            {
                blnPreview = false;
            }

            bool blnHasModuleEditPermissions = dataModule.HasEditRightsOnModule();

            if (blnPreview == false && blnHasModuleEditPermissions)
            {
                isEditable = true;
            }
            else
            {
                isEditable = false;
            }
            return isEditable;
        }

        public static bool IsModuleAdmin()
        {
            bool isModuleAdmin = Null.NullBoolean;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions && objModule.ModuleDefinition.DefaultCacheTime != -1)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }
            return PortalSettings.Current.ControlPanelSecurity == PortalSettings.ControlPanelPermission.TabEditor && isModuleAdmin;
        }

        public static bool IsPageAdmin()
        {
            bool isPageAdmin = Null.NullBoolean;
            if (TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage())
            {
                isPageAdmin = true;
            }
            return isPageAdmin;
        }
    }
}