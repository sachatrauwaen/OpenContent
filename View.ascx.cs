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
using System.Web.WebPages;
using System.Web;
using System.Web.Helpers;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Framework;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common;
using Satrabel.OpenContent.Components.Rss;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using Satrabel.OpenContent.Components.Dynamic;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Log;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Logging;
using Newtonsoft.Json;
using System.Text;
using Satrabel.OpenContent.Components.Lucene.Config;

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
        private readonly RenderInfo _renderinfo = new RenderInfo();
        private OpenContentSettings _settings;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // auto attach a ContentLocalized OpenContent module to the reference module of the default language
            string openContentAutoAttach = PortalController.GetPortalSetting("OpenContent_AutoAttach", ModuleContext.PortalId, "False");
            bool autoAttach = bool.Parse(openContentAutoAttach);
            if (autoAttach)
            {
                //var module = (new ModuleController()).GetModule(ModuleContext.moduleId, ModuleContext.tabId, false);
                ModuleInfo module = ModuleContext.Configuration;
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
                        _settings = module.OpenContentSettings();
                    }
                }
            }
            if (_settings == null)
                _settings = ModuleContext.OpenContentSettings();

            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.ModuleContext = ModuleContext;
            ti.Settings = _settings;
            ti.Renderinfo = _renderinfo;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page.Request.QueryString["id"] != null)
            {
                _itemId = Page.Request.QueryString["id"];
            }
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
            //initialize _info state
            _renderinfo.Template = _settings.Template;
            _renderinfo.DetailItemId = _itemId;
            if (_settings.TabId > 0 && _settings.ModuleId > 0) // other module
            {
                ModuleController mc = new ModuleController();
                _renderinfo.SetDataSourceModule(_settings.TabId, _settings.ModuleId, mc.GetModule(_renderinfo.ModuleId, _renderinfo.TabId, false), null, "");
            }
            else // this module
            {
                _renderinfo.SetDataSourceModule(_settings.TabId, ModuleContext.ModuleId, ModuleContext.Configuration, null, "");
            }
            //start rendering
            InitTemplateInfo();
            bool otherModuleWithFilterSettings = _settings.IsOtherModule && !string.IsNullOrEmpty(_settings.Query);
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
                            RenderDemoData();
                        }

                    }
                    else if (_renderinfo.Template != null)
                    {
                        RenderDemoData();
                    }
                }
                else if (_renderinfo.Template != null)
                {
                    RenderDemoData();
                }
            }
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
                IncludeResourses(_renderinfo.Template);
                //if (DemoData) pDemo.Visible = true;

                if (_renderinfo.Template != null && _renderinfo.Template.ClientSideData)
                {
                    DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxScriptSupport();
                    DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                }
                if (_renderinfo.Files != null && _renderinfo.Files.PartialTemplates != null)
                {
                    foreach (var item in _renderinfo.Files.PartialTemplates.Where(p => p.Value.ClientSide))
                    {
                        try
                        {
                            var f = new FileUri(_renderinfo.Template.ManifestFolderUri.FolderPath, item.Value.Template);
                            string s = File.ReadAllText(f.PhysicalFilePath);
                            var litPartial = new LiteralControl(s);
                            Controls.Add(litPartial);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, ex.Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                        }

                    }
                }
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
                if (templateDefined)
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
                        (listMode && !string.IsNullOrEmpty(_itemId) ? ModuleContext.EditUrl("id", _itemId.ToString()) : ModuleContext.EditUrl()),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
                }
                if (templateDefined && template.Manifest.AdditionalDataExists())
                {
                    foreach (var addData in template.Manifest.AdditionalData)
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
                if (templateDefined || settings.Manifest != null)
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
        private void InitTemplateInfo()
        {
            if (_settings.Template != null)
            {
                if (_renderinfo.Template.IsListTemplate)
                {
                    LogContext.Log(ModuleContext.ModuleId, "RequestContext", "QueryParam Id", _itemId);
                    // Multi items template
                    if (string.IsNullOrEmpty(_itemId))
                    {
                        // List template
                        if (_renderinfo.Template.Main != null)
                        {
                            // for list templates a main template need to be defined
                            _renderinfo.Files = _renderinfo.Template.Main;
                            string templateKey = GetDataList(_renderinfo, _settings, _renderinfo.Template.ClientSideData);
                            if (!string.IsNullOrEmpty(templateKey) && _renderinfo.Template.Views != null && _renderinfo.Template.Views.ContainsKey(templateKey))
                            {
                                _renderinfo.Files = _renderinfo.Template.Views[templateKey];
                            }
                            if (!_renderinfo.SettingsMissing)
                            {
                                _renderinfo.OutputString = GenerateListOutput(_settings.Template, _renderinfo.Files, _renderinfo.DataList, _renderinfo.SettingsJson);
                            }
                        }
                    }
                    else
                    {
                        // detail template
                        if (_renderinfo.Template.Detail != null)
                        {
                            GetDetailData(_renderinfo, _settings);
                        }
                        if (_renderinfo.Template.Detail != null && !_renderinfo.ShowInitControl)
                        {
                            _renderinfo.Files = _renderinfo.Template.Detail;
                            _renderinfo.OutputString = GenerateOutput(_settings.Template, _renderinfo.Template.Detail, _renderinfo.DataJson, _renderinfo.SettingsJson);
                        }
                        else // if itemid not corresponding to this module, show list template
                        {
                            // List template
                            if (_renderinfo.Template.Main != null)
                            {
                                // for list templates a main template need to be defined
                                _renderinfo.Files = _renderinfo.Template.Main;
                                string templateKey = GetDataList(_renderinfo, _settings, _renderinfo.Template.ClientSideData);
                                if (!string.IsNullOrEmpty(templateKey) && _renderinfo.Template.Views != null && _renderinfo.Template.Views.ContainsKey(templateKey))
                                {
                                    _renderinfo.Files = _renderinfo.Template.Views[templateKey];
                                }
                                if (!_renderinfo.ShowInitControl)
                                {
                                    _renderinfo.OutputString = GenerateListOutput(_settings.Template, _renderinfo.Files, _renderinfo.DataList, _renderinfo.SettingsJson);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // single item template
                    GetSingleData(_renderinfo, _settings);
                    bool settingsNeeded = _renderinfo.Template.SettingsNeeded();
                    if (!_renderinfo.ShowInitControl && (!settingsNeeded || !string.IsNullOrEmpty(_renderinfo.SettingsJson)))
                    {
                        _renderinfo.OutputString = GenerateOutput(_renderinfo.Template.MainTemplateUri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
                    }
                }
            }
        }

        private void IncludeResourses(TemplateManifest template)
        {
            if (template != null)
            {
                //JavaScript.RequestRegistration() 
                //string templateBase = template.FilePath.Replace("$.hbs", ".hbs");
                var cssfilename = new FileUri(Path.ChangeExtension(template.MainTemplateUri().FilePath, "css"));
                if (cssfilename.FileExists)
                {
                    ClientResourceManager.RegisterStyleSheet(Page, Page.ResolveUrl(cssfilename.UrlFilePath), FileOrder.Css.PortalCss);
                }
                var jsfilename = new FileUri(Path.ChangeExtension(template.MainTemplateUri().FilePath, "js"));
                if (jsfilename.FileExists)
                {
                    ClientResourceManager.RegisterScript(Page, Page.ResolveUrl(jsfilename.UrlFilePath), FileOrder.Js.DefaultPriority + 100);
                }
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
            }
        }

        private FileUri CheckFiles(TemplateManifest templateManifest, TemplateFiles files)
        {
            if (files == null)
            {
                LoggingUtils.ProcessModuleLoadException(this, new Exception("Manifest.json missing or incomplete"));
                return null;
            }

            var templateUri = new FileUri(templateManifest.ManifestFolderUri, files.Template);

            if (!templateUri.FileExists)
                LoggingUtils.ProcessModuleLoadException(this, new Exception("Template " + templateUri.UrlFilePath + " don't exist"));

            if (files.PartialTemplates != null)
            {
                foreach (var partial in files.PartialTemplates)
                {
                    var partialTemplateUri = new FileUri(templateManifest.ManifestFolderUri, partial.Value.Template);
                    if (!partialTemplateUri.FileExists)
                        LoggingUtils.ProcessModuleLoadException(this, new Exception("PartialTemplate " + partialTemplateUri.UrlFilePath + " don't exist"));
                }
            }
            return templateUri;
        }

        private bool Filter(string json, string key, string value)
        {
            bool accept = true;
            var obj = json.ToJObject("query string filter");
            JToken member = obj.SelectToken(key, false);
            if (member is JArray)
            {
                accept = member.Any(c => c.ToString() == value);
            }
            else if (member is JValue)
            {
                accept = member.ToString() == value;
            }
            return accept;
        }

        private bool Filter(dynamic obj, string key, string value)
        {
            bool accept = true;
            Object member = DynamicUtils.GetMemberValue(obj, key);
            if (member is IEnumerable<Object>)
            {
                accept = ((IEnumerable<Object>)member).Any(c => c.ToString() == value);
            }
            else if (member is string)
            {
                accept = (string)member == value;
            }
            return accept;
        }
        /*
         * Single Mode template
         * 
         */
        public void GetSingleData(RenderInfo info, OpenContentSettings settings)
        {
            info.ResetData();
            var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            var dsContext = new DataSourceContext()
            {
                ModuleId = info.ModuleId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Config = settings.Manifest.DataSourceConfig,
                Single = true
            };

            var dsItem = ds.Get(dsContext, null);

            if (dsItem != null)
            {
                info.SetData(dsItem, dsItem.Data, settings.Data);
            }
        }
        public string GetDataList(RenderInfo info, OpenContentSettings settings, bool clientSide)
        {
            string templateKey = "";
            info.ResetData();
            var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            var dsContext = new DataSourceContext()
            {
                ModuleId = info.ModuleId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Config = settings.Manifest.DataSourceConfig
            };
            IEnumerable<IDataItem> dataList = new List<IDataItem>();
            if (clientSide || !info.Files.DataInTemplate)
            {
                if (ds.Any(dsContext))
                {
                    info.SetData(dataList, settings.Data);
                    info.DataExist = true;
                }

                if (info.Template.Views != null)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(info.Template.Key.TemplateDir);
                    templateKey = GetTemplateKey(indexConfig);
                }
            }
            else
            {
                bool useLucene = info.Template.Manifest.Index;
                if (useLucene)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(info.Template.Key.TemplateDir);
                    if (info.Template.Views != null)
                    {
                        templateKey = GetTemplateKey(indexConfig);
                    }
                    bool addWorkFlow = ModuleContext.PortalSettings.UserMode != PortalSettings.Mode.Edit;
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    if (!string.IsNullOrEmpty(settings.Query))
                    {
                        var query = JObject.Parse(settings.Query);
                        queryBuilder.Build(query, addWorkFlow, ModuleContext.PortalSettings.UserId, Request.QueryString);
                    }
                    else
                    {
                        queryBuilder.BuildFilter(addWorkFlow, Request.QueryString);
                    }
                    dataList = ds.GetAll(dsContext, queryBuilder.Select).Items;
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Query";
                        LogContext.Log(ModuleContext.ModuleId, logKey, "select", queryBuilder.Select);
                        LogContext.Log(ModuleContext.ModuleId, logKey, "result", dataList);
                    }
                    //Log.Logger.DebugFormat("Query returned [{0}] results.", total);
                    if (!dataList.Any())
                    {
                        //Log.Logger.DebugFormat("Query did not return any results. API request: [{0}], Lucene Filter: [{1}], Lucene Query:[{2}]", settings.Query, queryDef.Filter == null ? "" : queryDef.Filter.ToString(), queryDef.Query == null ? "" : queryDef.Query.ToString());
                        if (ds.Any(dsContext))
                        {
                            info.SetData(dataList, settings.Data);
                            info.DataExist = true;
                        }
                    }
                }
                else
                {
                    //dataList = ctrl.GetContents(info.ModuleId).ToList();
                    dataList = ds.GetAll(dsContext, null).Items;
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Get all data of module";
                        LogContext.Log(ModuleContext.ModuleId, logKey, "result", dataList);
                    }
                }
                if (dataList.Any())
                {
                    info.SetData(dataList, settings.Data);
                }
            }
            return templateKey;
        }

        private string GetTemplateKey(FieldConfig IndexConfig)
        {
            string templateKey = "";
            var queryString = Request.QueryString;
            if (queryString != null)
            {
                foreach (string key in queryString)
                {
                    if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.Any(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var indexConfig = IndexConfig.Fields.Single(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        string val = queryString[key];
                        if (string.IsNullOrEmpty(templateKey))
                            templateKey = key;
                        else
                            templateKey += "-" + key;
                    }
                }
            }
            return templateKey;
        }

        public void GetDetailData(RenderInfo info, OpenContentSettings settings)
        {
            info.ResetData();
            var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            var dsContext = new DataSourceContext()
            {
                ModuleId = info.ModuleId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Config = settings.Manifest.DataSourceConfig
            };
            var dsItem = ds.Get(dsContext, info.DetailItemId);
            if (LogContext.IsLogActive)
            {
                var logKey = "Get detail data";
                LogContext.Log(ModuleContext.ModuleId, logKey, "result", dsItem);
            }

            if (dsItem != null)
            {
                info.SetData(dsItem, dsItem.Data, settings.Data);
            }
        }

        public bool GetDemoData(RenderInfo info, OpenContentSettings settings)
        {
            info.ResetData();
            //bool settingsNeeded = false;
            FileUri dataFilename = null;
            if (info.Template != null)
            {
                dataFilename = new FileUri(info.Template.ManifestFolderUri.UrlFolder, "data.json"); ;
            }
            if (dataFilename != null && dataFilename.FileExists)
            {
                string fileContent = File.ReadAllText(dataFilename.PhysicalFilePath);
                string settingContent = "";
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    if (settings.Template != null && info.Template.MainTemplateUri().FilePath == settings.Template.MainTemplateUri().FilePath)
                    {
                        settingContent = settings.Data;
                    }
                    if (string.IsNullOrEmpty(settingContent))
                    {
                        var settingsFilename = info.Template.MainTemplateUri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-data.json";
                        if (File.Exists(settingsFilename))
                        {
                            settingContent = File.ReadAllText(settingsFilename);
                        }
                        else
                        {
                            //string schemaFilename = info.Template.Uri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-schema.json";
                            //settingsNeeded = File.Exists(schemaFilename);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(fileContent))
                    info.SetData(null, fileContent, settingContent);
            }
            return !info.ShowInitControl; //!string.IsNullOrWhiteSpace(info.DataJson) && (!string.IsNullOrWhiteSpace(info.SettingsJson) || !settingsNeeded);
        }

        #region Render

        private string GenerateOutput(TemplateManifest templateManifest, TemplateFiles files, JToken dataJson, string settingsJson)
        {
            // detail template
            try
            {
                var templateVirtualFolder = templateManifest.ManifestFolderUri.UrlFolder;
                if (!string.IsNullOrEmpty(files.Template))
                {
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    FileUri templateUri = CheckFiles(templateManifest, files);

                    if (dataJson != null)
                    {
                        int mainTabId = _settings.DetailTabId > 0 ? _settings.DetailTabId : _settings.TabId;
                        ModelFactory mf = new ModelFactory(_renderinfo.Data, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, mainTabId, _settings.ModuleId);
                        dynamic model = mf.GetModelAsDynamic();


                        if (!string.IsNullOrEmpty(_renderinfo.Template.Manifest.DetailMetaTitle))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            Page.Title = hbEngine.Execute(_renderinfo.Template.Manifest.DetailMetaTitle, model);
                        }
                        if (!string.IsNullOrEmpty(_renderinfo.Template.Manifest.DetailMetaDescription))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            PageUtils.SetPageDescription(Page, hbEngine.Execute(_renderinfo.Template.Manifest.DetailMetaDescription, model));
                        }
                        if (!string.IsNullOrEmpty(_renderinfo.Template.Manifest.DetailMeta))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            PageUtils.SetPageMeta(Page, hbEngine.Execute(_renderinfo.Template.Manifest.DetailMeta, model));
                        }
                        return ExecuteTemplate(templateManifest, files, templateUri, model);
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
            }
            return "";
        }

        private string GenerateOutput(FileUri template, JToken dataJson, string settingsJson, TemplateFiles files)
        {
            try
            {
                if (template != null)
                {
                    string templateVirtualFolder = template.UrlFolder;
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    if (dataJson != null)
                    {
                        ModelFactory mf;
                        int mainTabId = _settings.DetailTabId > 0 ? _settings.DetailTabId : _settings.TabId;
                        if (_renderinfo.Data == null)
                        {
                            // demo data
                            mf = new ModelFactory(_renderinfo.DataJson, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, mainTabId, _settings.ModuleId);
                        }
                        else
                        {
                            mf = new ModelFactory(_renderinfo.Data, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, mainTabId, _settings.ModuleId);
                        }
                        dynamic model = mf.GetModelAsDynamic();
                        if (LogContext.IsLogActive)
                        {
                            var logKey = "Render single item template";
                            LogContext.Log(ModuleContext.ModuleId, logKey, "template", template.FilePath);
                            LogContext.Log(ModuleContext.ModuleId, logKey, "model", model);
                        }

                        if (template.Extension != ".hbs")
                        {
                            return ExecuteRazor(template, model);
                        }
                        else
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            return hbEngine.Execute(Page, template, model);
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex);
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex);
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
            }
            return "";
        }

        private string GenerateListOutput(TemplateManifest templateManifest, TemplateFiles files, IEnumerable<IDataItem> dataList, string settingsJson)
        {
            try
            {
                var templateVirtualFolder = templateManifest.ManifestFolderUri.UrlFolder;
                if (!string.IsNullOrEmpty(files.Template))
                {
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    FileUri templateUri = CheckFiles(templateManifest, files);
                    if (dataList != null)
                    {
                        int mainTabId = _settings.DetailTabId > 0 ? _settings.DetailTabId : _settings.TabId;
                        ModelFactory mf = new ModelFactory(dataList, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, mainTabId, _settings.ModuleId);
                        dynamic model = mf.GetModelAsDynamic();
                        return ExecuteTemplate(templateManifest, files, templateUri, model);
                    }
                }
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex);
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex);
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
            }
            return "";
        }

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
        private string ExecuteRazor(FileUri template, dynamic model)
        {

            string webConfig = template.PhysicalFullDirectory; // Path.GetDirectoryName(template.PhysicalFilePath);
            webConfig = webConfig.Remove(webConfig.LastIndexOf("\\")) + "\\web.config";
            if (!File.Exists(webConfig))
            {
                string filename = HostingEnvironment.MapPath("~/DesktopModules/OpenContent/Templates/web.config");
                File.Copy(filename, webConfig);
            }
            try
            {
                var writer = new StringWriter();
                try
                {
                    var razorEngine = new RazorEngine("~/" + template.FilePath, ModuleContext, LocalResourceFile);
                    razorEngine.Render(writer, model);
                }
                catch (Exception ex)
                {
                    string stack = string.Join("\n", ex.StackTrace.Split('\n').Where(s => s.Contains("\\Portals\\") && s.Contains("in")).Select(s => s.Substring(s.IndexOf("in"))).ToArray());
                    throw new TemplateException("Failed to render Razor template " + template.FilePath + "\n" + stack, ex, model, template.FilePath);
                }
                return writer.ToString();
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex);
                return "";
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex);
                return "";
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
                return "";
            }
        }
        private string ExecuteTemplate(TemplateManifest templateManifest, TemplateFiles files, FileUri templateUri, dynamic model)
        {
            var templateVirtualFolder = templateManifest.ManifestFolderUri.UrlFolder;
            if (LogContext.IsLogActive)
            {
                var logKey = "Render template";
                LogContext.Log(ModuleContext.ModuleId, logKey, "template", templateUri.FilePath);
                LogContext.Log(ModuleContext.ModuleId, logKey, "model", model);
            }
            if (templateUri.Extension != ".hbs")
            {
                return ExecuteRazor(templateUri, model);
            }
            else
            {
                HandlebarsEngine hbEngine = new HandlebarsEngine();
                return hbEngine.Execute(Page, this, files, templateVirtualFolder, model);
            }
        }
        private void RenderDemoData()
        {

            TemplateManifest template = _renderinfo.Template;
            if (template != null && template.IsListTemplate)
            {
                // Multi items template
                if (string.IsNullOrEmpty(_renderinfo.DetailItemId))
                {
                    // List template
                    if (template.Main != null)
                    {
                        // for list templates a main template need to be defined
                        _renderinfo.Files = _renderinfo.Template.Main;
                        /*
                        GetDataList(_renderinfo, _settings, template.ClientSideData);
                        if (!_renderinfo.SettingsMissing)
                        {
                            _renderinfo.OutputString = GenerateListOutput(_renderinfo.Template.Uri().UrlFolder, template.Main, _renderinfo.DataList, _renderinfo.SettingsJson);
                        }
                         */
                    }
                }
            }
            else
            {

                bool demoExist = GetDemoData(_renderinfo, _settings);
                bool settingsNeeded = _renderinfo.Template.SettingsNeeded();

                if (demoExist && (!settingsNeeded || !string.IsNullOrEmpty(_renderinfo.SettingsJson)))
                {
                    _renderinfo.OutputString = GenerateOutput(_renderinfo.Template.MainTemplateUri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
                }
                //too many rendering issues 
                //bool dsDataExist = _datasource.GetOtherModuleDemoData(_info, _info, _settings);
                //if (dsDataExist)
                //    _info.OutputString = GenerateOutput(_info.Template.Uri(), _info.DataJson, _info.SettingsJson, null);

            }
        }

        #endregion
    }
}