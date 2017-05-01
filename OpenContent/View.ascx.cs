#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Diagnostics;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using System.Web.UI;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Framework;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Logging;
using Newtonsoft.Json;
using System.Text;
using Satrabel.OpenContent.Components.Render;
using System.Web;
using Satrabel.OpenContent.Components.Dnn;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Framework.JavaScriptLibraries;
using Localization = DotNetNuke.Services.Localization.Localization;

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
        private string _itemId = null;
        private RenderInfo _renderinfo;
        private OpenContentSettings _settings;
        private RenderEngine _engine;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ModuleInfo module = ModuleContext.Configuration;

            if (App.Services.GlobalSettings.GetAutoAttach())
            {
                // auto attach a ContentLocalized OpenContent module to the reference module of the default language
                AutoAttachLocalizedModule(ref module);
            }
            _engine = new RenderEngine(module);
            _renderinfo = _engine.Info;
            _settings = _engine.Settings;

            //initialize TemplateInitControl
            TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.ModuleContext = ModuleContext;
            ti.Settings = _settings;
            ti.Renderinfo = _renderinfo;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _engine.QueryString = Page.Request.QueryString;
            if (Page.Request.QueryString["id"] != null)
            {
                _engine.ItemId = Page.Request.QueryString["id"];
            }
            _engine.LocalResourceFile = LocalResourceFile;
            _engine.ModuleContext = ModuleContext;
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
                if (ModuleContext.Configuration.HasEditRightsOnModule())
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
                //if (DemoData) pDemo.Visible = true;
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
            var defaultModule = module.DefaultLanguageModule;
            if (defaultModule != null)
            {
                if (ModuleContext.ModuleId != defaultModule.ModuleID)
                {
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
                if (!string.IsNullOrEmpty(App.Services.GlobalSettings.GetEditorRoleId()))
                {
                    int roleId = int.Parse(App.Services.GlobalSettings.GetEditorRoleId());
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

                TemplateManifest template = _settings.Template;
                bool templateDefined = template != null;
                bool listMode = template != null && template.IsListTemplate;

                if (Page.Request.QueryString["id"] != null)
                {
                    _itemId = Page.Request.QueryString["id"];
                }

                //Add item / Edit Item
                if (templateDefined && template.DataNeeded() && !_settings.Manifest.DisableEdit)
                {
                    string title = Localization.GetString((listMode && string.IsNullOrEmpty(_itemId) ? ModuleActionType.AddContent : ModuleActionType.EditContent), LocalResourceFile);
                    if (!string.IsNullOrEmpty(_settings.Manifest.Title))
                    {
                        title = Localization.GetString((listMode && string.IsNullOrEmpty(_itemId) ? "Add.Action" : "Edit.Action"), LocalResourceFile) + " " + _settings.Manifest.Title;
                    }
                    actions.Add(ModuleContext.GetNextActionID(),
                        title,
                        ModuleActionType.AddContent,
                        "",
                        (listMode && string.IsNullOrEmpty(_itemId) ? "~/DesktopModules/OpenContent/images/addcontent2.png" : "~/DesktopModules/OpenContent/images/editcontent2.png"),
                        (listMode && !string.IsNullOrEmpty(_itemId) ? ModuleContext.EditUrl("id", _itemId) : ModuleContext.EditUrl()),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
                }

                //Add AdditionalData manage actions
                if (templateDefined && template.Manifest.AdditionalDataDefined() && !_settings.Manifest.DisableEdit)
                {
                    foreach (var addData in template.Manifest.AdditionalDataDefinition)
                    {
                        if (addData.Value.SourceRelatedDataSource == RelatedDataSourceType.AdditionalData)
                        {
                            actions.Add(ModuleContext.GetNextActionID(),
                                addData.Value.Title,
                                ModuleActionType.EditContent,
                                "",
                                "~/DesktopModules/OpenContent/images/editcontent2.png",
                                ModuleContext.EditUrl("key", addData.Key, "EditAddData"),
                                false,
                                SecurityAccessLevel.Edit,
                                true,
                                false);
                        }
                        else
                        {
                            actions.Add(ModuleContext.GetNextActionID(),
                                addData.Value.Title,
                                ModuleActionType.EditContent,
                                "",
                                "~/DesktopModules/OpenContent/images/editcontent2.png",
                                DnnUrlUtils.NavigateUrl(addData.Value.DataTabId),
                                false,
                                SecurityAccessLevel.Edit,
                                true,
                                false);
                        }
                    }
                }

                //Manage Form Submissions
                if (templateDefined && OpenContentUtils.FormExist(_settings.Template.ManifestFolderUri))
                {

                    actions.Add(ModuleContext.GetNextActionID(),
                        "Submissions",
                        ModuleActionType.EditContent,
                        "",
                        "~/DesktopModules/OpenContent/images/editcontent2.png",
                        ModuleContext.EditUrl("Submissions"),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
                }

                //Edit Template Settings
                if (templateDefined && _settings.Template.SettingsNeeded())
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("EditSettings.Action", LocalResourceFile),
                        ModuleActionType.ContentOptions,
                        "",
                        "~/DesktopModules/OpenContent/images/editsettings2.png",
                        ModuleContext.EditUrl("EditSettings"),
                        false,
                        SecurityAccessLevel.Admin,
                        true,
                        false);
                }

                //Edit Form Settings
                if (templateDefined && OpenContentUtils.FormExist(_settings.Template.ManifestFolderUri))
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("FormSettings.Action", LocalResourceFile),
                        ModuleActionType.ContentOptions,
                        "",
                        "~/DesktopModules/OpenContent/images/editsettings2.png",
                        ModuleContext.EditUrl("formsettings"),
                        false,
                        SecurityAccessLevel.Admin,
                        true,
                        false);
                }

                //Switch Template
                actions.Add(ModuleContext.GetNextActionID(),
                    Localization.GetString("EditInit.Action", LocalResourceFile),
                    ModuleActionType.ContentOptions,
                    "",
                    "~/DesktopModules/OpenContent/images/editinit.png",
                    ModuleContext.EditUrl("EditInit"),
                    false,
                    SecurityAccessLevel.Admin,
                    true,
                    false);

                //Edit Filter Settings
                if (templateDefined && listMode)
                {
                    if (_settings.Manifest.Index)
                    {
                        actions.Add(ModuleContext.GetNextActionID(),
                            Localization.GetString("EditQuery.Action", LocalResourceFile),
                            ModuleActionType.ContentOptions,
                            "",
                            "~/DesktopModules/OpenContent/images/editfilter.png",
                            ModuleContext.EditUrl("EditQuery"),
                            false,
                            SecurityAccessLevel.Admin,
                            true,
                            false);
                    }
                }

                //Form Builder
                if (templateDefined && OpenContentUtils.BuildersExist(_settings.Template.ManifestFolderUri))
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("Builder.Action", LocalResourceFile),
                        ModuleActionType.ContentOptions,
                        "",
                        "~/DesktopModules/OpenContent/images/formbuilder.png",
                        ModuleContext.EditUrl("FormBuilder"),
                        false,
                        SecurityAccessLevel.Admin,
                        true,
                        false);

                //Edit Template Files
                if (templateDefined)
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("EditTemplate.Action", LocalResourceFile),
                        ModuleActionType.ContentOptions,
                        "",
                        "~/DesktopModules/OpenContent/images/edittemplate.png",
                        ModuleContext.EditUrl("EditTemplate"),
                        false,
                        SecurityAccessLevel.Host,
                        true,
                        false);
                
                //Edit Raw Data
                if (templateDefined && _settings.Manifest != null &&
                    (template.DataNeeded() || template.SettingsNeeded() || template.Manifest.AdditionalDataDefined()) && !_settings.Manifest.DisableEdit)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("EditData.Action", LocalResourceFile),
                        ModuleActionType.EditContent,
                        "",
                        "~/DesktopModules/OpenContent/images/edit.png",
                        (listMode && !string.IsNullOrEmpty(_itemId) ? ModuleContext.EditUrl("id", _itemId.ToString(), "EditData") : ModuleContext.EditUrl("EditData")),
                        false,
                        SecurityAccessLevel.Host,
                        true,
                        false);
                }

                //Template Exchange
                actions.Add(ModuleContext.GetNextActionID(),
                    Localization.GetString("ShareTemplate.Action", LocalResourceFile),
                    ModuleActionType.ContentOptions,
                    "",
                    "~/DesktopModules/OpenContent/images/exchange.png",
                    ModuleContext.EditUrl("ShareTemplate"),
                    false,
                    SecurityAccessLevel.Host,
                    true,
                    false);

                //Edit Global Settings
                actions.Add(ModuleContext.GetNextActionID(),
                    Localization.GetString("EditGlobalSettings.Action", LocalResourceFile),
                    ModuleActionType.ContentOptions,
                    "",
                    "~/DesktopModules/OpenContent/images/settings.png",
                    ModuleContext.EditUrl("EditGlobalSettings"),
                    false,
                    SecurityAccessLevel.Host,
                    true,
                    false);

                //Help
                actions.Add(ModuleContext.GetNextActionID(),
                    Localization.GetString("Help.Action", LocalResourceFile),
                    ModuleActionType.ContentOptions,
                    "",
                    "~/DesktopModules/OpenContent/images/help.png",
                    "https://opencontent.readme.io",
                    false,
                    SecurityAccessLevel.Host,
                    true,
                    true);


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