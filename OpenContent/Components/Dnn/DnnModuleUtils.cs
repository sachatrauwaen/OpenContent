using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components.Datasource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Satrabel.OpenContent.Components.Dnn
{
    public class DnnModuleUtils
    {

        public static void AutoAttachLocalizedModule(ref ModuleInfo module)
        {
            var defaultModule = module.DefaultLanguageModule;
            if (defaultModule == null) return;  //if module is not localized, return
            if (module.ModuleID == defaultModule.ModuleID) return;  //if module is already attached, return

            if (ModuleHasValidConfig(module) && ModuleHasAlreadyData(module))
            {
                // this module is in another language but has already data.
                // Therefor we will not AutoAttach it, because otherwise all data will be deleted.

                //App.Services.Logger.Info($"Module {module.ModuleID} on Tab {module.TabID} has not been AutoAttached because it already contains data.");
                //return;
            }

            try
            {
                var mc = (new ModuleController());
                mc.DeLocalizeModule(module);

                mc.ClearCache(defaultModule.TabID);
                mc.ClearCache(module.TabID);
                const string MODULE_SETTINGS_CACHE_KEY = "ModuleSettings{0}"; // to be compatible with dnn 7.2
                DataCache.RemoveCache(string.Format(MODULE_SETTINGS_CACHE_KEY, defaultModule.TabID));
                DataCache.RemoveCache(string.Format(MODULE_SETTINGS_CACHE_KEY, module.TabID));

                module = mc.GetModule(defaultModule.ModuleID, module.TabID, true);
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Module {module.ModuleID} on Tab {module.TabID} has not been AutoAttached because an error occured", ex);
            }
        }

        public static bool ModuleHasValidConfig(ModuleInfo moduleInfo)
        {
            OpenContentModuleConfig ocModuleConfig = OpenContentModuleConfig.Create(moduleInfo, PortalSettings.Current);
            return ocModuleConfig.Settings.Manifest != null;
        }

        public static bool ModuleHasAlreadyData(ModuleInfo moduleInfo)
        {
            OpenContentModuleConfig ocModuleConfig = OpenContentModuleConfig.Create(moduleInfo, PortalSettings.Current);
            Datasource.IDataSource ds = DataSourceManager.GetDataSource(ocModuleConfig.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(ocModuleConfig);

            return ds.Any(dsContext);
        }

        public static void AutoEditMode(PortalSettings PortalSettings)
        {
            
                if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
                {
                    var defaultMode = PortalSettings.DefaultControlPanelMode;
                    if (defaultMode == PortalSettings.Mode.Edit)
                    {
                        string setting = Convert.ToString(Personalization.GetProfile("Usability", "UserMode" + PortalSettings.Current.PortalId));
                        //if (!IsPageAdmin() & IsModuleAdmin())
                        {
                            if (setting != "EDIT")
                            {
                                Personalization.SetProfile("Usability", "UserMode" + PortalSettings.Current.PortalId, "EDIT");
                                //Page.Response.AppendHeader("X-UserMode", setting + "/" + IsPageAdmin() + "/" + IsModuleAdmin());
                            }
                            JavaScript.RequestRegistration(CommonJs.DnnPlugins); // avoid js error 
                        }
                    }
                }
                //string  usermode = "" + DotNetNuke.Services.Personalization.Personalization.GetProfile("Usability", "UserMode" + PortalSettings.Current.PortalId);
            
        }

        public static void AddEditorRole(ModuleInstanceContext ModuleContext)
        {
            if (ModuleContext.PortalSettings.UserId > 0)
            {
                var roleIdStr = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetEditorRoleId();
                if (!string.IsNullOrEmpty(roleIdStr))
                {
                    int roleId = int.Parse(roleIdStr);
                    var objModule = ModuleContext.Configuration;
                    //todo: probable DNN bug.  objModule.ModulePermissions doesn't return correct permissions for attached multi-lingual modules
                    //don't alter permissions of modules that are non-default language and that are attached
                    var permExist = objModule.ModulePermissions.Where(tp => tp.RoleID == roleId).Any();
                    if (!permExist)
                    {
                        AutoSetPermission(objModule, roleId);
                    }
                }
            }
        }

        public static void AutoSetPermission(ModuleInfo objModule, int roleId)
        {
            //todo sacha: add two permissions, read and write; Or better still add all permissions that are available. eg if you installed extra permissions

            var permissionController = new PermissionController();
            // view permission
            var arrSystemModuleViewPermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
            var permission = (PermissionInfo)arrSystemModuleViewPermissions[0];
            var objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                //ModuleDefID = permission.ModuleDefID,
                //PermissionCode = permission.PermissionCode,
                PermissionID = permission.PermissionID,
                PermissionKey = permission.PermissionKey,
                RoleID = roleId,
                //UserID = userId,
                AllowAccess = true
            };
            objModule.ModulePermissions.Add(objModulePermission);

            // edit permission
            arrSystemModuleViewPermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "EDIT");
            permission = (PermissionInfo)arrSystemModuleViewPermissions[0];
            objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                //ModuleDefID = permission.ModuleDefID,
                //PermissionCode = permission.PermissionCode,
                PermissionID = permission.PermissionID,
                PermissionKey = permission.PermissionKey,
                RoleID = roleId,
                //UserID = userId,
                AllowAccess = true
            };
            objModule.ModulePermissions.Add(objModulePermission);
            try
            {
                ModulePermissionController.SaveModulePermissions(objModule);
            }
            catch (Exception)
            {
                //App.Services.Logger.Error($"Failed to automaticly set the permission. It already exists? tab={0}, moduletitle={1} ", objModule.TabID ,objModule.ModuleTitle);
            }
        }
    }
}