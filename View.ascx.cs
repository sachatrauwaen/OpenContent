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
                            var f = new FileUri(_renderinfo.Template.ManifestDir.FolderPath, item.Value.Template);
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
                if (templateDefined && template.Manifest.AdditionalData != null)
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
        #endregion
        private void InitTemplateInfo()
        {
            if (_settings.Template != null)
            {
                if (_renderinfo.Template.IsListTemplate)
                {
                    // Multi items template
                    if (string.IsNullOrEmpty(_itemId))
                    {
                        // List template
                        if (_renderinfo.Template.Main != null)
                        {
                            // for list templates a main template need to be defined
                            _renderinfo.Files = _renderinfo.Template.Main;
                            GetDataList(_renderinfo, _settings, _renderinfo.Template.ClientSideData);
                            if (!_renderinfo.SettingsMissing)
                            {
                                _renderinfo.OutputString = GenerateListOutput(_settings.Template.Uri().UrlFolder, _renderinfo.Template.Main, _renderinfo.DataList, _renderinfo.SettingsJson);
                            }
                        }
                    }
                    else
                    {
                        // detail template
                        GetDetailData(_renderinfo, _settings);
                        if (_renderinfo.Template.Detail != null && !_renderinfo.ShowInitControl)
                        {
                            _renderinfo.Files = _renderinfo.Template.Detail;
                            _renderinfo.OutputString = GenerateOutput(_settings.Template.Uri().UrlFolder, _renderinfo.Template.Detail, _renderinfo.DataJson, _renderinfo.SettingsJson);
                        }
                        else // if itemid not corresponding to this module, show list template
                        {
                            // List template
                            if (_renderinfo.Template.Main != null)
                            {
                                // for list templates a main template need to be defined
                                _renderinfo.Files = _renderinfo.Template.Main;
                                GetDataList(_renderinfo, _settings, _renderinfo.Template.ClientSideData);
                                if (!_renderinfo.ShowInitControl)
                                {
                                    _renderinfo.OutputString = GenerateListOutput(_settings.Template.Uri().UrlFolder, _renderinfo.Template.Main, _renderinfo.DataList, _renderinfo.SettingsJson);
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
                        _renderinfo.OutputString = GenerateOutput(_renderinfo.Template.Uri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
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
                var cssfilename = new FileUri(Path.ChangeExtension(template.Uri().FilePath, "css"));
                if (cssfilename.FileExists)
                {
                    ClientResourceManager.RegisterStyleSheet(Page, Page.ResolveUrl(cssfilename.UrlFilePath), FileOrder.Css.PortalCss);
                }
                var jsfilename = new FileUri(Path.ChangeExtension(template.Uri().FilePath, "js"));
                if (jsfilename.FileExists)
                {
                    ClientResourceManager.RegisterScript(Page, Page.ResolveUrl(jsfilename.UrlFilePath), FileOrder.Js.DefaultPriority + 100);
                }
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
            }
        }

        private FileUri CheckFiles(string templateVirtualFolder, TemplateFiles files, string templateFolder)
        {
            if (files == null)
                Exceptions.ProcessModuleLoadException(this, new Exception("Manifest.json missing or incomplete"));
            string templateFile = templateFolder + "\\" + files.Template;
            string template = templateVirtualFolder + "/" + files.Template;
            if (!File.Exists(templateFile))
                Exceptions.ProcessModuleLoadException(this, new Exception(template + " don't exist"));
            if (files.PartialTemplates != null)
            {
                foreach (var partial in files.PartialTemplates)
                {
                    templateFile = templateFolder + "\\" + partial.Value.Template;
                    string partialTemplate = templateVirtualFolder + "/" + partial.Value.Template;
                    if (!File.Exists(templateFile))
                        Exceptions.ProcessModuleLoadException(this, new Exception(partialTemplate + " don't exist"));
                }
            }
            return new FileUri(template);
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
        public void GetDataList(RenderInfo info, OpenContentSettings settings, bool clientSide)
        {
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
            }
            else
            {
                bool useLucene = info.Template.Manifest.Index;
                if (useLucene)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(info.Template.Key.TemplateDir);
                    bool addWorkFlow = ModuleContext.PortalSettings.UserMode != PortalSettings.Mode.Edit;
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    if (!string.IsNullOrEmpty(settings.Query))
                    {
                        var query = JObject.Parse(settings.Query);
                        queryBuilder.Build(query, addWorkFlow);
                    }
                    else
                    {
                        queryBuilder.BuildFilter(addWorkFlow);
                    }
                    dataList = ds.GetAll(dsContext, queryBuilder.Select).Items;
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
                }
                if (dataList.Any())
                {
                    info.SetData(dataList, settings.Data);
                }
            }
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
                dataFilename = new FileUri(info.Template.Uri().UrlFolder, "data.json"); ;
            }
            if (dataFilename != null && dataFilename.FileExists)
            {
                string fileContent = File.ReadAllText(dataFilename.PhysicalFilePath);
                string settingContent = "";
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    if (settings.Template != null && info.Template.Uri().FilePath == settings.Template.Uri().FilePath)
                    {
                        settingContent = settings.Data;
                    }
                    if (string.IsNullOrEmpty(settingContent))
                    {
                        var settingsFilename = info.Template.Uri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-data.json";
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
        /*
        internal bool GetOtherModuleDemoData(RenderInfo info, OpenContentSettings settings)
        {
            info.ResetData();
            var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            var dsContext = new DataSourceContext()
            {
                ModuleId = info.ModuleId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Config = settings.Manifest.DataSourceConfig
            };
            var dsItem = ds.GetFirst(dsContext);
            if (dsItem != null)
            {
                if (settings.Template != null && info.Template.Uri().FilePath == settings.Template.Uri().FilePath)
                {
                    info.SetData(dsItem, dsItem.Data, settings.Data);
                }
                if (string.IsNullOrEmpty(info.SettingsJson))
                {
                    var settingsFilename = info.Template.Uri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-data.json";
                    if (File.Exists(settingsFilename))
                    {
                        string settingsContent = File.ReadAllText(settingsFilename);
                        if (!string.IsNullOrWhiteSpace(settingsContent))
                        {
                            info.SetData(dsItem, dsItem.Data, settingsContent);
                        }
                    }
                }
                //Als er OtherModuleSettingsJson bestaan en 
                if (info.OtherModuleTemplate.Uri().FilePath == info.Template.Uri().FilePath && !string.IsNullOrEmpty(info.OtherModuleSettingsJson))
                {
                    info.SetData(dsItem, dsItem.Data, info.OtherModuleSettingsJson);
                }
                return true;
            }
            return false;
        }
        */
        #region Render

        private string GenerateOutput(string templateVirtualFolder, TemplateFiles files, JToken dataJson, string settingsJson)
        {
            // detail template
            try
            {
                if (!(string.IsNullOrEmpty(files.Template)))
                {
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    FileUri template = CheckFiles(templateVirtualFolder, files, physicalTemplateFolder);

                    if (dataJson != null)
                    {
                        int MainTabId = _settings.DetailTabId > 0 ? _settings.DetailTabId : _settings.TabId;
                        ModelFactory mf = new ModelFactory(_renderinfo.Data, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, MainTabId, _settings.ModuleId);
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
                        //Page.Title = model.Title + " | " + ModuleContext.PortalSettings.PortalName;
                        return ExecuteTemplate(templateVirtualFolder, files, template, model);
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
                Exceptions.ProcessModuleLoadException(this, ex);
            }
            return "";
        }

        private string GenerateOutput(FileUri template, JToken dataJson, string settingsJson, TemplateFiles files)
        {
            try
            {
                if (template != null)
                {
                    if (!template.FileExists)
                        Exceptions.ProcessModuleLoadException(this, new Exception(template.FilePath + " don't exist"));

                    string templateVirtualFolder = template.UrlFolder;
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    if (dataJson != null)
                    {
                        ModelFactory mf;
                        int MainTabId = _settings.DetailTabId > 0 ? _settings.DetailTabId : _settings.TabId;
                        if (_renderinfo.Data == null)
                        {
                            // demo data
                            mf = new ModelFactory(_renderinfo.DataJson, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, MainTabId, _settings.ModuleId);
                        }
                        else
                        {
                            mf = new ModelFactory(_renderinfo.Data, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, MainTabId, _settings.ModuleId);
                        }
                        dynamic model = mf.GetModelAsDynamic();
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
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);

            }
            return "";
        }

        private string GenerateListOutput(string templateVirtualFolder, TemplateFiles files, IEnumerable<IDataItem> dataList, string settingsJson)
        {
            try
            {
                if (!(string.IsNullOrEmpty(files.Template)))
                {
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    FileUri templateUri = CheckFiles(templateVirtualFolder, files, physicalTemplateFolder);
                    if (dataList != null)
                    {
                        int MainTabId = _settings.DetailTabId > 0 ? _settings.DetailTabId : _settings.TabId;
                        ModelFactory mf = new ModelFactory(dataList, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, ModuleContext.Configuration, ModuleContext.PortalSettings, MainTabId, _settings.ModuleId);
                        dynamic model = mf.GetModelAsDynamic();
                        return ExecuteTemplate(templateVirtualFolder, files, templateUri, model);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
            return "";
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
                var razorEngine = new RazorEngine("~/" + template.FilePath, ModuleContext, LocalResourceFile);
                var writer = new StringWriter();
                RazorRender(razorEngine.Webpage, writer, model);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(string.Format("Error while loading template {0} on page {1}", template.FilePath, this.Request.RawUrl), this, ex);
                return "";
            }
        }
        private string ExecuteTemplate(string templateVirtualFolder, TemplateFiles files, FileUri template, dynamic model)
        {
            if (template.Extension != ".hbs")
            {
                return ExecuteRazor(template, model);
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
                    _renderinfo.OutputString = GenerateOutput(_renderinfo.Template.Uri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
                }
                //too many rendering issues 
                //bool dsDataExist = _datasource.GetOtherModuleDemoData(_info, _info, _settings);
                //if (dsDataExist)
                //    _info.OutputString = GenerateOutput(_info.Template.Uri(), _info.DataJson, _info.SettingsJson, null);

            }
        }

        private void RazorRender(WebPageBase webpage, TextWriter writer, dynamic model)
        {
            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            if ((webpage) is OpenContentWebPage)
            {
                var mv = (OpenContentWebPage)webpage;
                mv.Model = model;
            }
            if (webpage != null)
                webpage.ExecutePageHierarchy(new WebPageContext(httpContext, webpage, null), writer, webpage);
        }

        #endregion
    }
}