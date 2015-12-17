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
using DotNetNuke.Web.Razor;
using System.IO;
using DotNetNuke.Services.Exceptions;
using System.Web.UI;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using Newtonsoft.Json;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Json;
using System.Web.WebPages;
using System.Web;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Framework;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common;
using DotNetNuke.UI;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Controllers;
using Satrabel.OpenContent.Components.Rss;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using Satrabel.OpenContent.Components.Dynamic;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Log;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{
    /// <summary>
    /// This view will look in the settings if a template and all the necessary extra has already been defined.
    /// If so, it will render the template
    /// If not, it will display a 
    /// </summary>
    public partial class View : RazorModuleBase, IActionable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(View));

        private int _itemId = Null.NullInteger;
        private readonly TemplateInfo _info = new TemplateInfo();
        private OpenContentSettings _settings;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var modSettings = ModuleContext.Settings;

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
                        modSettings = module.ModuleSettings;
                    }
                }
            }

            _settings = new OpenContentSettings(modSettings);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.Request.QueryString["id"] != null)
            {
                int.TryParse(Page.Request.QueryString["id"], out _itemId);
            }

            if (!Page.IsPostBack)
            {
                //if (ModuleContext.EditMode && !ModuleContext.IsEditable)
                if (ModuleContext.PortalSettings.UserId > 0)
                {
                    string OpenContent_EditorsRoleId = PortalController.GetPortalSetting("OpenContent_EditorsRoleId", ModuleContext.PortalId, "");
                    if (!string.IsNullOrEmpty(OpenContent_EditorsRoleId))
                    {
                        int roleId = int.Parse(OpenContent_EditorsRoleId);
                        var objModule = ModuleContext.Configuration;
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
                            ModulePermissionController.SaveModulePermissions(objModule);
                        }
                    }


                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            //base.OnPreRender(e);
            pHelp.Visible = false;

            //initialize _info state
            _info.SetSelectedTemplate(_settings.Template);
            _info.DetailItemId = _itemId;
            if (_settings.TabId > 0 && _settings.ModuleId > 0) // other module
            {
                ModuleController mc = new ModuleController();
                _info.SetDataSourceModule(_settings.TabId, _settings.ModuleId, mc.GetModule(_info.ModuleId, _info.TabId, false), null, "");
            }
            else // this module
            {
                _info.SetDataSourceModule(_settings.TabId, ModuleContext.ModuleId, ModuleContext.Configuration, null, "");
            }

            //start rendering
            InitTemplateInfo();
            if (_info.ShowInitControl)
            {
                // no data exist and ... -> show initialization
                if (ModuleContext.EditMode)
                {
                    // edit mode
                    if (_info.Template == null || ModuleContext.IsEditable)
                    {
                        RenderInitForm();
                    }
                    else if (_info.Template != null)
                    {
                        RenderDemoData();
                    }
                }
                else if (_info.Template != null)
                {
                    RenderDemoData();
                }
            }
            if (_info.Template != null && !string.IsNullOrEmpty(_info.OutputString))
            {
                //Rendering was succesful.

                var lit = new LiteralControl(Server.HtmlDecode(_info.OutputString));
                Controls.Add(lit);
                var mst = _info.Template.Manifest;
                bool editWitoutPostback = mst != null && mst.EditWitoutPostback;
                if (ModuleContext.PortalSettings.EnablePopUps && ModuleContext.IsEditable && editWitoutPostback)
                {
                    AJAX.WrapUpdatePanelControl(lit, true);
                }
                IncludeResourses(_info.Template);
                //if (DemoData) pDemo.Visible = true;
                if (_info.Template.IsListTemplate)
                {
                    DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxScriptSupport();
                    DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                }

            }
        }

        public DotNetNuke.Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();

                var settings = new OpenContentSettings(ModuleContext.Settings);
                TemplateManifest template = settings.Template;
                bool templateDefined = template != null;
                bool listMode = template != null && template.IsListTemplate;

                if (Page.Request.QueryString["id"] != null)
                {
                    int.TryParse(Page.Request.QueryString["id"], out _itemId);
                }

                if (templateDefined)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString((listMode && _itemId == Null.NullInteger ? ModuleActionType.AddContent : ModuleActionType.EditContent), LocalResourceFile),
                        ModuleActionType.AddContent,
                        "",
                        "",
                        (listMode && _itemId != Null.NullInteger ? ModuleContext.EditUrl("id", _itemId.ToString()) : ModuleContext.EditUrl()),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
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
                    Localization.GetString("EditSettings.Action", LocalResourceFile),
                    ModuleActionType.ContentOptions,
                    "",
                    "~/DesktopModules/OpenContent/images/settings.gif",
                    ModuleContext.EditUrl("EditSettings"),
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
                if (templateDefined || settings.Manifest != null)
                    actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString("EditData.Action", LocalResourceFile),
                        ModuleActionType.EditContent,
                        "",
                        "~/DesktopModules/OpenContent/images/edit.png",
                        ModuleContext.EditUrl("EditData"),
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
                    "~/DesktopModules/OpenContent/images/exchange.png",
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

        protected void rblDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblDataSource.SelectedIndex == 1) // other module
            {
                //BindOtherModules(dsModule.TabID, dsModule.ModuleID);
                BindOtherModules(-1, -1);
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                var dsSettings = new OpenContentSettings(dsModule.ModuleSettings);
                BindTemplates(dsSettings.Template, dsSettings.Template.Uri());
            }
            else // this module
            {
                BindOtherModules(-1, -1);
                BindTemplates(null, null);
            }
        }

        protected void rblUseTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            phFrom.Visible = rblUseTemplate.SelectedIndex == 1;
            phTemplateName.Visible = rblUseTemplate.SelectedIndex == 1;
            rblFrom.SelectedIndex = 0;
            var scriptFileSetting = new OpenContentSettings(ModuleContext.Settings).Template;
            ddlTemplate.Items.Clear();
            if (rblUseTemplate.SelectedIndex == 0) // existing
            {
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());
            }
            else if (rblUseTemplate.SelectedIndex == 1) // new
            {
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());
            }
        }

        protected void rblFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTemplate.Items.Clear();
            if (rblFrom.SelectedIndex == 0) // site
            {
                var scriptFileSetting = new OpenContentSettings(ModuleContext.Settings).Template;
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());

                //ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.moduleId, scriptFileSetting, "OpenContent").ToArray());
            }
            else if (rblFrom.SelectedIndex == 1) // web
            {
                FeedParser parser = new FeedParser();
                var items = parser.Parse("http://www.openextensions.net/templates?agentType=rss&PropertyTypeID=9", FeedType.RSS);
                foreach (var item in items.OrderBy(t => t.Title))
                {
                    ddlTemplate.Items.Add(new ListItem(item.Title, item.ZipEnclosure));
                }
                if (ddlTemplate.Items.Count > 0)
                {
                    tbTemplateName.Text = Path.GetFileNameWithoutExtension(ddlTemplate.Items[0].Value);
                }
            }
        }
        protected void bSave_Click(object sender, EventArgs e)
        {
            try
            {
                ModuleController mc = new ModuleController();

                if (rblDataSource.SelectedIndex == 0) // this module
                {
                    mc.DeleteModuleSetting(ModuleContext.ModuleId, "tabid");
                    mc.DeleteModuleSetting(ModuleContext.ModuleId, "moduleid");
                }
                else // other module
                {
                    var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "tabid", dsModule.TabID.ToString());
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "moduleid", dsModule.ModuleID.ToString());
                }

                if (rblUseTemplate.SelectedIndex == 0) // existing
                {
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", ddlTemplate.SelectedValue);
                }
                else if (rblUseTemplate.SelectedIndex == 1) // new
                {
                    if (rblFrom.SelectedIndex == 0) // site
                    {
                        string oldFolder = Server.MapPath(ddlTemplate.SelectedValue);
                        string template = OpenContentUtils.CopyTemplate(ModuleContext.PortalId, oldFolder, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                    }
                    else if (rblFrom.SelectedIndex == 1) // web
                    {
                        string fileName = ddlTemplate.SelectedValue;
                        string template = OpenContentUtils.ImportFromWeb(ModuleContext.PortalId, fileName, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                    }
                }
                //don't reset settings. Sure they might be invalid, but maybe not. And you can't ever revert.
                //mc.DeleteModuleSetting(ModuleContext.ModuleId, "data");
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                //Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void ddlTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblUseTemplate.SelectedIndex == 0) // existing
            {

            }
            else if (rblUseTemplate.SelectedIndex == 1) // new template
            {
                if (rblFrom.SelectedIndex == 1) // web
                {
                    tbTemplateName.Text = Path.GetFileNameWithoutExtension(ddlTemplate.SelectedValue);
                }
            }
        }

        protected void ddlDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
            var dsSettings = new OpenContentSettings(dsModule.ModuleSettings);
            BindTemplates(dsSettings.Template, dsSettings.Template.Uri());
        }

        #endregion

        #region GetData

        private void GetData()
        {
            _info.ResetData();

            OpenContentController ctrl = new OpenContentController();
            var struc = ctrl.GetFirstContent(_info.ModuleId);
            if (struc != null)
            {
                _info.SetData(struc.Json, _settings.Data);
            }
        }

        private void GetDataList(TemplateInfo info, OpenContentSettings settings, bool clientSide)
        {
            _info.ResetData();
            OpenContentController ctrl = new OpenContentController();
            List<OpenContentInfo> dataList;
            if (clientSide)
            {
                var data = ctrl.GetFirstContent(info.ModuleId);
                if (data != null)
                {
                    dataList = new List<OpenContentInfo>();
                    _info.SetData(dataList, settings.Data);
                }
            }
            else
            {
                bool useLucene = info.Template.Manifest.Index;
                if (useLucene)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    string luceneFilter = settings.LuceneFilter;
                    string luceneSort = settings.LuceneSort;
                    int? luceneMaxResults = settings.LuceneMaxResults;
                    SearchResults docs = LuceneController.Instance.Search(_info.ModuleId.ToString(), "Title", "",
                        luceneFilter, luceneSort, (luceneMaxResults.HasValue ? luceneMaxResults.Value : 100), 0,
                        indexConfig);
                    int total = docs.ToalResults;
                    dataList = new List<OpenContentInfo>();
                    foreach (var item in docs.ids)
                    {
                        var content = ctrl.GetContent(int.Parse(item));
                        if (content != null)
                        {
                            dataList.Add(content);
                        }
                    }
                }
                else
                {
                    dataList = ctrl.GetContents(info.ModuleId).ToList();
                }
                if (dataList.Any())
                {
                    _info.SetData(dataList, settings.Data);
                }
            }
        }

        private void GetDetailData(TemplateInfo info, OpenContentSettings settings)
        {
            _info.ResetData();
            OpenContentController ctrl = new OpenContentController();
            var struc = ctrl.GetContent(info.DetailItemId);
            if (struc != null)
            {
                _info.SetData(struc.Json, settings.Data);
            }

        }

        private bool GetDemoData(TemplateInfo info, OpenContentSettings settings)
        {
            _info.ResetData();
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
                    _info.SetData(fileContent, settingContent);
            }
            return !info.ShowInitControl; //!string.IsNullOrWhiteSpace(info.DataJson) && (!string.IsNullOrWhiteSpace(info.SettingsJson) || !settingsNeeded);
        }

        private bool GetOtherModuleDemoData(TemplateInfo info, OpenContentSettings settings)
        {
            _info.ResetData();
            var ctrl = new OpenContentController();
            var struc = ctrl.GetFirstContent(info.ModuleId);
            if (struc != null)
            {
                if (settings.Template != null && info.Template.Uri().FilePath == settings.Template.Uri().FilePath)
                {
                    _info.SetData(struc.Json, settings.Data);
                }
                if (string.IsNullOrEmpty(info.SettingsJson))
                {
                    var settingsFilename = info.Template.Uri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-data.json";
                    if (File.Exists(settingsFilename))
                    {
                        string settingsContent = File.ReadAllText(settingsFilename);
                        if (!string.IsNullOrWhiteSpace(settingsContent))
                        {
                            _info.SetData(struc.Json, settingsContent);
                        }
                    }
                }
                //Als er OtherModuleSettingsJson bestaan en 
                if (_info.OtherModuleTemplate.Uri().FilePath == _info.Template.Uri().FilePath && !string.IsNullOrEmpty(_info.OtherModuleSettingsJson))
                {
                    _info.SetData(struc.Json, _info.OtherModuleSettingsJson);
                }
                _info.OutputString = GenerateOutput(_info.Template.Uri(), _info.DataJson, _info.SettingsJson, null);

                return true;
            }
            return false;
        }

        #endregion

        private void InitTemplateInfo()
        {
            if (_settings.Template != null)
            {
                if (_info.Template.IsListTemplate)
                {
                    // Multi items template
                    if (_itemId == Null.NullInteger)
                    {
                        // List template
                        if (_info.Template.Main != null)
                        {
                            // for list templates a main template need to be defined
                            GetDataList(_info, _settings, _info.Template.ClientSideData);
                            if (!_info.ShowInitControl)
                            {
                                _info.OutputString = GenerateListOutput(_settings.Template.Uri().UrlFolder, _info.Template.Main, _info.DataList, _info.SettingsJson);
                            }
                        }
                    }
                    else
                    {
                        // detail template
                        if (_info.Template.Detail != null)
                        {
                            GetDetailData(_info, _settings);
                            if (!_info.ShowInitControl)
                            {
                                _info.OutputString = GenerateOutput(_settings.Template.Uri().UrlFolder, _info.Template.Detail, _info.DataJson, _info.SettingsJson);
                            }
                        }
                    }
                }
                else
                {
                    // single item template
                    GetData();
                    if (!_info.ShowInitControl)
                    {
                        _info.OutputString = GenerateOutput(_info.Template.Uri(), _info.DataJson, _info.SettingsJson, _info.Template.Main);
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
                    ClientResourceManager.RegisterScript(Page, Page.ResolveUrl(jsfilename.UrlFilePath), FileOrder.Js.DefaultPriority+100);
                }
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
            }
        }

        private FileUri CheckFiles(string templateVirtualFolder, TemplateFiles files, string templateFolder)
        {
            if (files == null)
            {
                Exceptions.ProcessModuleLoadException(this, new Exception("Manifest.json missing or incomplete"));
            }
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
            JObject obj = json.ToJObject("query string filter");
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

        private void BindOtherModules(int tabId, int moduleId)
        {
            IEnumerable<ModuleInfo> modules = (new ModuleController()).GetModules(ModuleContext.PortalId).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == "OpenContent" && m.IsDeleted == false);
            rblDataSource.Items[1].Enabled = modules.Any();
            phDataSource.Visible = rblDataSource.SelectedIndex == 1; // other module
            if (rblDataSource.SelectedIndex == 1) // other module
            {
                rblUseTemplate.SelectedIndex = 0; // existing template
                phFrom.Visible = false;
                phTemplateName.Visible = false;
            }
            rblUseTemplate.Items[1].Enabled = rblDataSource.SelectedIndex == 0; // this module
            ddlDataSource.Items.Clear();
            var listItems = new List<ListItem>();
            foreach (var item in modules)
            {
                if (item.TabModuleID != ModuleContext.TabModuleId)
                {
                    var tc = new TabController();
                    var tab = tc.GetTab(item.TabID, ModuleContext.PortalId, false);
                    var li = new ListItem(tab.TabName + " - " + item.ModuleTitle, item.TabModuleID.ToString());
                    listItems.Add(li);
                    if (item.TabID == tabId && item.ModuleID == moduleId)
                    {
                        li.Selected = true;
                    }
                }
            }
            foreach (ListItem li in listItems.OrderBy(x => x.Text))
            {
                ddlDataSource.Items.Add(li);
            }
        }

        private void BindTemplates(TemplateManifest template, FileUri otherModuleTemplate)
        {
            ddlTemplate.Items.Clear();

            //var templateUri = template == null ? null : template.Uri;
            //var otherModuleTemplateUri = otherModuleTemplate == null ? null : otherModuleTemplate.Uri;

            ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, template, "OpenContent", otherModuleTemplate).ToArray());
            if (ddlTemplate.Items.Count == 0)
            {
                rblUseTemplate.Items[0].Enabled = false;
                rblUseTemplate.SelectedIndex = 1;
                rblUseTemplate_SelectedIndexChanged(null, null);
                rblFrom.Items[0].Enabled = false;
                rblFrom.SelectedIndex = 1;
                rblFrom_SelectedIndexChanged(null, null);

            }
        }

        private void BindButtons(OpenContentSettings settings, TemplateInfo info)
        {
            bool templateDefined = info.Template != null;
            bool settingsDefined = !string.IsNullOrEmpty(settings.Data);
            bool settingsNeeded = false;

            if (rblUseTemplate.SelectedIndex == 0) // existing template
            {
                //create tmp TemplateManifest
                var templateManifest = new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest();
                settingsNeeded = templateManifest.SettingsNeeded();

                templateDefined = templateDefined && (!ddlTemplate.Visible || (settings.Template.Key.FullKeyString() == ddlTemplate.SelectedValue));
                settingsDefined = settingsDefined || !settingsNeeded;
            }
            else // new template
            {
                templateDefined = false;
            }

            bSave.CssClass = "dnnPrimaryAction";
            bSave.Enabled = true;
            hlEditSettings.CssClass = "dnnSecondaryAction";
            hlEditContent.CssClass = "dnnSecondaryAction";
            //if (ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            hlEditSettings.Enabled = false;
            hlEditSettings.Visible = settingsNeeded;

            if (templateDefined && ModuleContext.EditMode && settingsNeeded)
            {
                //hlTempleteExchange.NavigateUrl = ModuleContext.EditUrl("ShareTemplate");
                hlEditSettings.NavigateUrl = ModuleContext.EditUrl("EditSettings");
                //hlTempleteExchange.Visible = true;
                hlEditSettings.Enabled = true;

                bSave.CssClass = "dnnSecondaryAction";
                bSave.Enabled = false;
                hlEditSettings.CssClass = "dnnPrimaryAction";
                hlEditContent.CssClass = "dnnSecondaryAction";

            }
            hlEditContent.Enabled = false;
            hlEditContent2.Enabled = false;
            if (templateDefined && settingsDefined && ModuleContext.EditMode)
            {
                hlEditContent.NavigateUrl = ModuleContext.EditUrl("Edit");
                hlEditContent.Enabled = true;
                hlEditContent2.NavigateUrl = ModuleContext.EditUrl("Edit");
                hlEditContent2.Enabled = true;
                bSave.CssClass = "dnnSecondaryAction";
                bSave.Enabled = false;
                hlEditSettings.CssClass = "dnnSecondaryAction";
                hlEditContent.CssClass = "dnnPrimaryAction";
            }
        }

        #region Render

        private string GenerateOutput(string templateVirtualFolder, TemplateFiles files, string dataJson, string settingsJson)
        {
            try
            {
                if (!(string.IsNullOrEmpty(files.Template)))
                {
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    FileUri template = CheckFiles(templateVirtualFolder, files, physicalTemplateFolder);

                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        ModelFactory mf = new ModelFactory(dataJson, settingsJson, physicalTemplateFolder, _info.Template.Manifest, files, ModuleContext.Configuration, ModuleContext.PortalSettings);
                        dynamic model = mf.GetModelAsDynamic();

                        Page.Title = model.Title + " | " + ModuleContext.PortalSettings.PortalName;
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

        private string GenerateOutput(FileUri template, string dataJson, string settingsJson, TemplateFiles files)
        {
            try
            {
                if (template != null)
                {
                    if (!template.FileExists)
                        Exceptions.ProcessModuleLoadException(this, new Exception(template.FilePath + " don't exist"));

                    string templateVirtualFolder = template.UrlFolder;
                    string physicalTemplateFolder = Server.MapPath(templateVirtualFolder);
                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        ModelFactory mf = new ModelFactory(dataJson, settingsJson, physicalTemplateFolder, _info.Template.Manifest, files, ModuleContext.Configuration, ModuleContext.PortalSettings);
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

        private string GenerateListOutput(string TemplateVirtualFolder, TemplateFiles files, IEnumerable<OpenContentInfo> dataList, string settingsJson)
        {
            try
            {
                if (!(string.IsNullOrEmpty(files.Template)))
                {
                    string physicalTemplateFolder = Server.MapPath(TemplateVirtualFolder);
                    FileUri templateUri = CheckFiles(TemplateVirtualFolder, files, physicalTemplateFolder);
                    if (dataList != null)
                    {
                        ModelFactory mf = new ModelFactory(dataList, settingsJson, physicalTemplateFolder, _info.Template.Manifest, files, ModuleContext.Configuration, ModuleContext.PortalSettings);
                        dynamic model = mf.GetModelAsDynamic();
                        return ExecuteTemplate(TemplateVirtualFolder, files, templateUri, model);
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

        private void RenderInitForm()
        {
            pHelp.Visible = true;
            if (!Page.IsPostBack)
            {
                rblDataSource.SelectedIndex = (_settings.TabId > 0 && _settings.ModuleId > 0 ? 1 : 0);
                BindOtherModules(_settings.TabId, _settings.ModuleId);
                BindTemplates(_settings.Template, (_info.IsOtherModule ? _info.Template.Uri() : null));
            }
            if (rblDataSource.SelectedIndex == 1) // other module
            {
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                var dsSettings = new OpenContentSettings(dsModule.ModuleSettings);
                _info.SetDataSourceModule(dsModule.TabID, dsModule.ModuleID, dsModule, dsSettings.Template, dsSettings.Data);
            }
            BindButtons(_settings, _info);
            if (rblUseTemplate.SelectedIndex == 0) // existing template
            {
                _info.SetSelectedTemplate(new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest());
                if (rblDataSource.SelectedIndex == 0) // this module
                {
                    RenderDemoData();
                }
                else // other module
                {
                    RenderOtherModuleDemoData();
                }
            }
            else // new template
            {
                _info.SetSelectedTemplate(new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest());
                if (rblFrom.SelectedIndex == 0) // site
                {
                    RenderDemoData();
                }
            }
        }

        private void RenderDemoData()
        {
            bool demoExist = GetDemoData(_info, _settings);
            if (demoExist)
            {
                _info.OutputString = GenerateOutput(_info.Template.Uri(), _info.DataJson, _info.SettingsJson, _info.Template.Main);
            }
        }

        private void RenderOtherModuleDemoData()
        {
            TemplateManifest template = _info.Template;
            if (template != null && template.IsListTemplate)
            {
                // Multi items template
                if (_info.DetailItemId == Null.NullInteger)
                {
                    // List template
                    if (template.Main != null)
                    {
                        // for list templates a main template need to be defined
                        GetDataList(_info, _settings, template.ClientSideData);
                        if (!_info.ShowInitControl)
                        {
                            _info.OutputString = GenerateListOutput(_info.Template.Uri().UrlFolder, template.Main, _info.DataList, _info.SettingsJson);
                        }
                    }
                }
            }
            else
            {
                //too many rendering issues 
             //   bool dsDataExist = GetOtherModuleDemoData(_info, _settings);

            }
        }

        private void RazorRender(WebPageBase webpage, TextWriter writer, dynamic model)
        {
            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            if ((webpage) is DotNetNukeWebPage<dynamic>)
            {
                var mv = (DotNetNukeWebPage<dynamic>)webpage;
                mv.Model = model;
            }
            if (webpage != null)
                webpage.ExecutePageHierarchy(new WebPageContext(httpContext, webpage, null), writer, webpage);
        }

        #endregion
    }
}