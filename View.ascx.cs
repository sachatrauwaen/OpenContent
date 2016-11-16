#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Razor;
using System.IO;
using DotNetNuke.Services.Exceptions;
using System.Web.UI;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Framework;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Dynamic;
using DotNetNuke.Security.Permissions;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Logging;
using Newtonsoft.Json;
using System.Text;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Render;
using System.Web;

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
        RenderEngine engine;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ModuleInfo module = ModuleContext.Configuration;
            // auto attach a ContentLocalized OpenContent module to the reference module of the default language
            string openContentAutoAttach = PortalController.GetPortalSetting("OpenContent_AutoAttach", ModuleContext.PortalId, "False");
            bool autoAttach = bool.Parse(openContentAutoAttach);
            if (autoAttach)
            {
                //var module = (new ModuleController()).GetModule(ModuleContext.moduleId, ModuleContext.tabId, false);
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
            engine = new RenderEngine(module);
            _renderinfo = engine.Info;
            _settings = engine.Settings;

            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.ModuleContext = ModuleContext;
            ti.Settings = _settings;
            ti.Renderinfo = _renderinfo;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            engine.QueryString = Page.Request.QueryString;
            if (Page.Request.QueryString["id"] != null)
            {
                engine.ItemId = Page.Request.QueryString["id"];
            }
            engine.LocalResourceFile = LocalResourceFile;
            engine.ModuleContext = ModuleContext;
            if (!Page.IsPostBack)
            {
                if (ModuleContext.PortalSettings.UserId > 0)
                {
                    string OpenContent_EditorsRoleId = PortalController.GetPortalSetting("OpenContent_EditorsRoleId", ModuleContext.PortalId, "");
                    if (!string.IsNullOrEmpty(OpenContent_EditorsRoleId))
                    {
                        int roleId = int.Parse(OpenContent_EditorsRoleId);
                        var objModule = ModuleContext.Configuration;
                        //todo: probable DNN bug.  objModule.ModulePermissions doesn't return correct permissions for attached multi-lingual modules
                        //don't alter permissions of modules that are non-default language and that are attached
                        var permExist = objModule.ModulePermissions.Where(tp => tp.RoleID == roleId).Any();
                        if (!permExist)
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
                                //Log.Logger.ErrorFormat("Failed to automaticly set the permission. It already exists? tab={0}, moduletitle={1} ", objModule.TabID ,objModule.ModuleTitle);
                            }
                        }
                    }
                }
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            //base.OnPreRender(e);
            //pHelp.Visible = false;
            try
            {
                engine.Render(Page);
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex);
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex);
            }
            /*
        catch (HttpException ex)
        {
            throw ex;
        }
             */
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
            }
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
                try
                {
                    engine.IncludeResourses(Page, this);
                }
                catch (Exception ex)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, ex.Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                }
                //if (DemoData) pDemo.Visible = true;
            }
            if (LogContext.IsLogActive && !Debugger.IsAttached)
            {
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
                StringBuilder logScript = new StringBuilder();
                logScript.AppendLine("<script type=\"text/javascript\"> ");
                logScript.AppendLine("$(document).ready(function () { ");
                logScript.AppendLine("var logs = " + JsonConvert.SerializeObject(LogContext.Current.ModuleLogs(ModuleContext.ModuleId)) + "; ");
                logScript.AppendLine("$.fn.openContent.printLogs('Module " + ModuleContext.ModuleId + " - " + ModuleContext.Configuration.ModuleTitle + "', logs);");
                logScript.AppendLine("});");
                logScript.AppendLine("</script>");
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "logScript" + ModuleContext.ModuleId, logScript.ToString());
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
                            engine.RenderDemoData(Page);
                        }
                    }
                    else if (_renderinfo.Template != null)
                    {
                        engine.RenderDemoData(Page);
                    }
                }
                else if (_renderinfo.Template != null)
                {
                    engine.RenderDemoData(Page);
                }
            }
        }
        private void RenderInitForm()
        {
            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.RenderInitForm();
        }
        public DotNetNuke.Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();

                var settings = ModuleContext.OpenContentSettings();
                TemplateManifest template = settings.Template;
                bool templateDefined = template != null;
                bool listMode = template != null && template.IsListTemplate;

                if (Page.Request.QueryString["id"] != null)
                {
                    _itemId = Page.Request.QueryString["id"];
                }
                if (templateDefined && template.DataNeeded() && !settings.Manifest.DisableEdit)
                {
                    string title = Localization.GetString((listMode && string.IsNullOrEmpty(_itemId) ? ModuleActionType.AddContent : ModuleActionType.EditContent), LocalResourceFile);
                    if (!string.IsNullOrEmpty(settings.Manifest.Title))
                    {
                        title = Localization.GetString((listMode && string.IsNullOrEmpty(_itemId) ? "Add.Action" : "Edit.Action"), LocalResourceFile) + " " + settings.Manifest.Title;
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
                if (templateDefined && template.Manifest.AdditionalDataExists() && !settings.Manifest.DisableEdit)
                {
                    foreach (var addData in template.Manifest.AdditionalData)
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
                            
                        }
                    }
                }
                /*
                string AddEditControl = PortalController.GetPortalSetting("OpenContent_AddEditControl", ModuleContext.PortalId, "");
                if (TemplateDefined && !string.IsNullOrEmpty(AddEditControl))
                {
                    Actions.Add(ModuleContext.GetNextActionID(),
                                Localization.GetString("AddEntity.Action", LocalResourceFile),
                                ModuleActionType.EditContent,
                                "",
                                "",
                                ModuleContext.EditUrl("AddEdit"),
                                false,
                                SecurityAccessLevel.Edit,
                                true,
                                false);
                }
                */


                if (templateDefined && settings.Template.SettingsNeeded())
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

                if (templateDefined && OpenContentUtils.FormExist(settings.Template.ManifestFolderUri))
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("FormSettings.Action", LocalResourceFile),
                        ModuleActionType.ContentOptions,
                        "",
                        "~/DesktopModules/OpenContent/images/editsettings2.png",
                        ModuleContext.EditUrl("formsettings"),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
                }

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
                if (templateDefined && listMode)
                {
                    //bool queryAvailable = settings.Template.QueryAvailable();
                    if (settings.Manifest.Index)
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


                if (templateDefined && OpenContentUtils.BuildersExist(settings.Template.ManifestFolderUri))
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
                if (templateDefined && settings.Manifest != null &&
                     (template.DataNeeded() || template.SettingsNeeded() || template.Manifest.AdditionalDataExists()) && !settings.Manifest.DisableEdit)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("EditData.Action", LocalResourceFile),
                        ModuleActionType.EditContent,
                        "",
                        "~/DesktopModules/OpenContent/images/edit.png",
                        //ModuleContext.EditUrl("EditData"),
                        (listMode && !string.IsNullOrEmpty(_itemId) ? ModuleContext.EditUrl("id", _itemId.ToString(), "EditData") : ModuleContext.EditUrl("EditData")),
                        false,
                        SecurityAccessLevel.Host,
                        true,
                        false);
                }
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
                /*
                Actions.Add(ModuleContext.GetNextActionID(),
                           Localization.GetString("EditGlobalSettings.Action", LocalResourceFile),
                           ModuleActionType.ContentOptions,
                           "",
                           "~/DesktopModules/OpenContent/images/settings.png",
                           ModuleContext.EditUrl("EditGlobalSettings"),
                           false,
                           SecurityAccessLevel.Host,
                           true,
                           false);
                */
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
        private string RemoveHost(string editUrl)
        {
            //Dnn sometimes adds an incorrect alias.
            //To fix this just remove the host. Give the browser a relative url

            if (string.IsNullOrEmpty(editUrl)) return editUrl;
            editUrl = editUrl.Replace("//", "");
            var pos = editUrl.IndexOf("/");
            if (pos == -1) return editUrl;
            return editUrl.Remove(0, pos);
        }
        #endregion

        #region Exceptions
        private void RenderTemplateException(TemplateException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template error</b></p>" + ex.MessageAsHtml, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template source</b></p>" + Server.HtmlEncode(ex.TemplateSource).Replace("\n", "<br/>"), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template model</b></p> <pre>" + JsonConvert.SerializeObject(ex.TemplateModel, Formatting.Indented)/*.Replace("\n", "<br/>")*/+"</pre>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            //lErrorMessage.Text = ex.HtmlMessage;
            //lErrorModel.Text = "<pre>" + JsonConvert.SerializeObject(ex.TemplateModel, Formatting.Indented)/*.Replace("\n", "<br/>")*/+"</pre>";
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in tempate";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Model", ex.TemplateModel);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Source", ex.TemplateSource);
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }
        private void RenderJsonException(InvalidJsonFileException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Json error</b></p>" + ex.MessageAsHtml, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in json";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Filename", ex.Filename);
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }
        #endregion
    }
}