#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Render;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using IDataSource = Satrabel.OpenContent.Components.Datasource.IDataSource;
using SecurityAccessLevel = DotNetNuke.Security.SecurityAccessLevel;

#endregion

namespace Satrabel.OpenContent
{
    /// <summary>
    /// This view will look in the settings if a template and all the necessary extra has already been defined.
    /// If so, it will render the template
    /// If not, it will display a 
    /// </summary>
    public partial class View : DotNetNuke.Web.Razor.RazorModuleBase, IActionable
    {
        private RenderInfo _renderinfo;
        private OpenContentSettings _settings;
        private RenderEngine _engine;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ModuleInfo module = ModuleContext.Configuration;

            if (App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetAutoAttach())
            {
                // auto attach a ContentLocalized OpenContent module to the reference module of the default language
                AutoAttachLocalizedModule(ref module);
            }
            _engine = new RenderEngine(OpenContentModuleConfig.Create(module, PortalSettings.Current), new DnnRenderContext(ModuleContext), LocalResourceFile);
            _renderinfo = _engine.Info;
            _settings = _engine.Settings;
            _engine.QueryString = Page.Request.QueryString;
            if (Page.Request.QueryString["id"] != null)
            {
                _engine.ItemId = Page.Request.QueryString["id"];
            }

            //initialize TemplateInitControl
            TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.ModuleContext = ModuleContext;
            ti.Settings = _settings;
            ti.Renderinfo = _renderinfo;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            if (!Page.IsPostBack)
            {
                AddEditorRole();
                AutoEditMode();
            }
            try
            {
                _engine.Render(Page);
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex);
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex);
            }
            catch (NotAuthorizedException ex)
            {
                if (_engine.ModuleConfig.ViewModule.HasEditRightsOnModule())
                    RenderHttpException(ex);
                else
                    throw;
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
            }
            if (_renderinfo.Template != null && !string.IsNullOrEmpty(_renderinfo.OutputString))
            {
                try
                {
                    _engine.IncludeResourses(Page, this);
                }
                catch (Exception ex)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, ex.Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            //base.OnPreRender(e);
            //pHelp.Visible = false;
            GenerateAndRenderDemoData();
            if (_renderinfo.Template != null && !string.IsNullOrEmpty(_renderinfo.OutputString))
            {
                //Rendering was succesful.
                var lit = new LiteralControl(Server.HtmlDecode(_renderinfo.OutputString));
                Controls.Add(lit);
                var mst = _renderinfo.Template.Manifest;
                bool editWitoutPostback = mst != null && mst.EditWitoutPostback;
                if (ModuleContext.PortalSettings.EnablePopUps && ModuleContext.IsEditable && editWitoutPostback)
                {
                    AJAX.WrapUpdatePanelControl(lit, true);
                }
                _engine.IncludeMeta(Page);
            }
            if (LogContext.IsLogActive && !Debugger.IsAttached)
            {
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
                StringBuilder logScript = new StringBuilder();
                //logScript.AppendLine("<script type=\"text/javascript\"> ");
                logScript.AppendLine("$(document).ready(function () { ");
                logScript.AppendLine("var logs = " + JsonConvert.SerializeObject(LogContext.Current.ModuleLogs(ModuleContext.ModuleId)) + "; ");
                logScript.AppendLine("$.fn.openContent.printLogs(\"Module " + ModuleContext.ModuleId + " - " + ModuleContext.Configuration.ModuleTitle + "\", logs);");
                logScript.AppendLine("});");
                //logScript.AppendLine("</script>");
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "logScript" + ModuleContext.ModuleId, /*DotNetNuke.UI.Utilities.ClientAPI.EscapeForJavascript*/(logScript.ToString()), true);
            }
        }

        #endregion

        #region Private Methods

        protected bool IsModuleAdmin()
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

        protected bool IsPageAdmin()
        {
            bool isPageAdmin = Null.NullBoolean;
            if (TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage())
            {
                isPageAdmin = true;
            }
            return isPageAdmin;
        }

        private void AutoAttachLocalizedModule(ref ModuleInfo module)
        {
            if (module.DefaultLanguageModule != null) //if module is not on a default language page
            {
                var defaultModule = module.DefaultLanguageModule;
                if (ModuleContext.ModuleId != defaultModule.ModuleID) //not yet attached
                {
                    if (ModuleHasAlreadyData(module))
                    {
                        // this module is in another language but has already data.
                        // Therefor we will not AutoAttach it, because otherwise all data will be deleted.
                        App.Services.Logger.Info($"Module {module.ModuleID} on Tab {module.TabID} has not been AutoAttached because it already contains data.");
                        return;
                    }

                    var mc = (new ModuleController());
                    mc.DeLocalizeModule(module);

                    mc.ClearCache(defaultModule.TabID);
                    mc.ClearCache(module.TabID);
                    const string MODULE_SETTINGS_CACHE_KEY = "ModuleSettings{0}"; // to be compatible with dnn 7.2
                    DataCache.RemoveCache(string.Format(MODULE_SETTINGS_CACHE_KEY, defaultModule.TabID));
                    DataCache.RemoveCache(string.Format(MODULE_SETTINGS_CACHE_KEY, module.TabID));

                    //DataCache.ClearCache();
                    module = mc.GetModule(defaultModule.ModuleID, ModuleContext.TabId, true);
                    //_settings = module.OpenContentSettings();
                }
            }
        }

        private static bool ModuleHasAlreadyData(ModuleInfo moduleInfo)
        {
            OpenContentModuleConfig ocModuleConfig = OpenContentModuleConfig.Create(moduleInfo, PortalSettings.Current);
            IDataSource ds = DataSourceManager.GetDataSource(ocModuleConfig.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(ocModuleConfig);

            return ds.Any(dsContext);
        }

        private void AutoEditMode()
        {
            if (!Page.IsPostBack)
            {
                if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
                {
                    var defaultMode = ModuleContext.PortalSettings.DefaultControlPanelMode;
                    if (defaultMode == PortalSettings.Mode.Edit)
                    {
                        string setting = Convert.ToString(Personalization.GetProfile("Usability", "UserMode" + PortalSettings.Current.PortalId));
                        if (!IsPageAdmin() & IsModuleAdmin())
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
        }

        private void AddEditorRole()
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

        private void AutoSetPermission(ModuleInfo objModule, int roleId)
        {
            //todo sacha: add two permissions, read and write; Or better still add all permissions that are available. eg if you installed extra permissions

            var permissionController = new PermissionController();
            // view permission
            var arrSystemModuleViewPermissions = permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
            var permission = (PermissionInfo)arrSystemModuleViewPermissions[0];
            var objModulePermission = new ModulePermissionInfo
            {
                ModuleID = ModuleContext.Configuration.ModuleID,
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
                ModuleID = ModuleContext.Configuration.ModuleID,
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
            catch (Exception ex)
            {
                //App.Services.Logger.Error($"Failed to automaticly set the permission. It already exists? tab={0}, moduletitle={1} ", objModule.TabID ,objModule.ModuleTitle);
            }
        }

        private void GenerateAndRenderDemoData()
        {
            bool otherModuleWithFilterSettings = _settings.IsOtherModule && _settings.Query.Exists();
            if (_renderinfo.ShowInitControl && !otherModuleWithFilterSettings)
            {
                /* thows error because _renderinfo.Module is null
                var templatemissing = OpenContentUtils.CheckOpenContentSettings(_renderinfo.Module, _settings);
                if (templatemissing)
                {
                    //todo: show message on screen
                }
                */
                // no data exist and ... -> show initialization
                if (ModuleContext.EditMode)
                {
                    // edit mode
                    if (_renderinfo.Template == null || ModuleContext.IsEditable)
                    {
                        RenderInitForm();
                        if (_renderinfo.ShowDemoData)
                        {
                            _engine.RenderDemoData(Page);
                        }
                    }
                    else if (_renderinfo.Template != null)
                    {
                        _engine.RenderDemoData(Page);
                    }
                }
                else if (_renderinfo.Template != null)
                {
                    _engine.RenderDemoData(Page);
                }
            }
        }

        private void RenderInitForm()
        {
            TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.RenderInitForm();
        }

        #endregion

        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                var actionDefinitions = _engine.GetMenuActions();

                foreach (var item in actionDefinitions)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        item.Title,
                        item.ActionType.ToDnnActionType(),
                        "",
                        item.Image,
                        item.Url,
                        false,
                        item.AccessLevel.ToDnnSecurityAccessLevel(),
                        true,
                        item.NewWindow);
                }

                return actions;
            }
        }

        #endregion

        #region Exceptions
        private void RenderTemplateException(TemplateException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template error</b></p>" + ex.MessageAsHtml(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template source</b></p>" + Server.HtmlEncode(ex.TemplateSource).Replace("\n", "<br/>"), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template model</b></p> <pre>" + JsonConvert.SerializeObject(ex.TemplateModel, Formatting.Indented)/*.Replace("\n", "<br/>")*/+"</pre>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            //lErrorMessage.Text = ex.HtmlMessage;
            //lErrorModel.Text = "<pre>" + JsonConvert.SerializeObject(ex.TemplateModel, Formatting.Indented)/*.Replace("\n", "<br/>")*/+"</pre>";
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in tempate";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList());
                LogContext.Log(ModuleContext.ModuleId, logKey, "Model", ex.TemplateModel);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Source", ex.TemplateSource);
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }
        private void RenderJsonException(InvalidJsonFileException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Json error</b></p>" + ex.MessageAsHtml(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in json";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList());
                LogContext.Log(ModuleContext.ModuleId, logKey, "Filename", ex.Filename);
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }

        private void RenderHttpException(NotAuthorizedException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Permission error</b></p>" + ex.Message.Replace("\n", "<br />"), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            if (LogContext.IsLogActive)
            {
                var logKey = "Error accessing data";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList());
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }
        #endregion
    }

}