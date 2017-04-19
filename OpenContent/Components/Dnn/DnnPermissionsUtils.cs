using System;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;

namespace Satrabel.OpenContent.Components.Dnn
{
    public static class DnnPermissionsUtils
    {
        public static bool HasEditPermissions(PortalSettings portalSettings, ModuleInfo module, string editrole, int createdByUserId)
        {
            return module.HasEditRightsOnModule() || HasEditRole(portalSettings, editrole, createdByUserId);
        }

        public static bool HasEditRole(PortalSettings portalSettings, string editrole, int createdByUserId)
        {
            if (String.IsNullOrEmpty(editrole)) return false;
            if (editrole.ToLower() == "all") return true;
            if (portalSettings.UserInfo.IsInRole(editrole) && (createdByUserId == -1 || createdByUserId == portalSettings.UserId)) return true;
            return false;
        }

        public static bool CheckIfEditable(this ModuleInfo activeModule, PortalSettings portalSettings)
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
            bool blnPreview = (portalSettings.UserMode == PortalSettings.Mode.View);
            if (Globals.IsHostTab(portalSettings.ActiveTab.TabID))
            {
                blnPreview = false;
            }

            bool blnHasModuleEditPermissions = HasEditRightsOnModule(activeModule);

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

        public static bool HasEditRightsOnModule(this ModuleInfo activeModule)
        {
            bool blnHasModuleEditPermissions = false;
            if (activeModule != null)
            {
                //DNN already checks SuperUser and Administrator
                blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", activeModule);
            }
            return blnHasModuleEditPermissions;
        }
    }
}