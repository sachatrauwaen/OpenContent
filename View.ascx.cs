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

#endregion

namespace Satrabel.OpenContent
{
    public partial class View : RazorModuleBase, IActionable
    {
        private int _itemId = Null.NullInteger;

        protected override void OnPreRender(EventArgs e)
        {
            pHelp.Visible = false;
            //base.OnPreRender(e);
            string settingsJson;
            string outputString = "";
            FileUri template = OpenContentUtils.GetTemplate(ModuleContext.Settings);
            FileUri selectedTemplate = template;
            bool dataExist = false;
            //string templateFolder = "";
            TemplateManifest manifest = null;
            if (template != null)
            {
                // if there is a manifest and Main section exist , use it as template
                manifest = OpenContentUtils.GetTemplateManifest(template);
                if (manifest != null && manifest.Main != null)
                {
                    template = new FileUri(template.Directory, manifest.Main.Template);
                }
            }
            if (manifest != null && manifest.IsListTemplate)
            {
                // Multi items Template
                if (_itemId == Null.NullInteger)
                {
                    // List template
                    if (manifest.Main != null)
                    {
                        // for list templates a main template need to be defined
                        IEnumerable<OpenContentInfo> dataList;
                        dataExist = GetDataList(out dataList, out settingsJson);
                        if (dataExist)
                        {
                            outputString = GenerateListOutput(template.Directory, manifest.Main, dataList, settingsJson);
                        }
                    }
                }
                else
                {
                    // detail template
                    if (manifest.Detail != null)
                    {
                        string dataJson;
                        dataExist = GetData(_itemId, out dataJson, out settingsJson);
                        if (dataExist)
                        {
                            outputString = GenerateOutput(template.Directory, manifest.Detail, dataJson, settingsJson);
                        }
                    }
                }
            }
            else
            {
                TemplateFiles files = null;
                if (manifest != null)
                {
                    files = manifest.Main;
                }
                // single item template
                string dataJson;
                dataExist = GetData(out dataJson, out settingsJson);
                if (dataExist)
                {
                    outputString = GenerateOutput(template, dataJson, settingsJson, files);
                }
            }
            if (!dataExist)
            {
                // no date exist and ... -> show initialization
                string dataJson;
                if (ModuleContext.EditMode)
                {
                    // edit mode
                    if (template == null || ModuleContext.IsEditable)
                    {
                        pHelp.Visible = true;
                        if (!Page.IsPostBack)
                        {
                            ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, selectedTemplate, "OpenContent").ToArray());
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
                        string settingsData = ModuleContext.Settings["data"] as string;
                        bool templateDefined = template != null;
                        bool settingsDefined = !string.IsNullOrEmpty(settingsData);
                        bool settingsNeeded = false;

                        if (rblUseTemplate.SelectedIndex == 0)
                        {
                            string templateFilename = HostingEnvironment.MapPath("~/" + ddlTemplate.SelectedValue);
                            string prefix = Path.GetFileNameWithoutExtension(templateFilename) + "-";
                            string schemaFilename = Path.GetDirectoryName(templateFilename) + "\\" + prefix + "schema.json";
                            settingsNeeded = File.Exists(schemaFilename);
                            templateDefined = templateDefined &&
                                (!ddlTemplate.Visible || (template.FilePath == ddlTemplate.SelectedValue));
                            settingsDefined = settingsDefined || !settingsNeeded;
                        }
                        else
                        {
                            templateDefined = false;
                        }
                        bool TemplateDefined = template != null;
                        bool SettingsDefined = !string.IsNullOrEmpty(settingsData);
                        bool SettingsNeeded = false;

                        if (rblUseTemplate.SelectedIndex == 0)
                        {
                            string templateFilename = HostingEnvironment.MapPath("~/" + ddlTemplate.SelectedValue);
                            string prefix = Path.GetFileNameWithoutExtension(templateFilename) + "-";
                            string schemaFilename = Path.GetDirectoryName(templateFilename) + "\\" + prefix + "schema.json";
                            SettingsNeeded = File.Exists(schemaFilename);
                            TemplateDefined = TemplateDefined &&
                                (!ddlTemplate.Visible || (template.FilePath == ddlTemplate.SelectedValue));
                            SettingsDefined = SettingsDefined || !SettingsNeeded;
                        }
                        else
                        {
                            TemplateDefined = false;
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


                        if (rblUseTemplate.SelectedIndex == 0)
                        {
                            template = new FileUri(ddlTemplate.SelectedValue);
                            bool demoExist = GetDemoData(template, out dataJson, out settingsJson);
                            if (demoExist)
                            {
                                manifest = OpenContentUtils.GetTemplateManifest(template);
                                if (manifest != null && manifest.Main != null)
                                {
                                    //templateFolder = template.Directory;
                                    template = new FileUri(template.Directory + "/" + manifest.Main.Template);
                                }
                                outputString = GenerateOutput(template, dataJson, settingsJson, null);
                            }
                        }
                    }
                    else if (template != null)
                    {
                        bool demoExist = GetDemoData(template, out dataJson, out settingsJson);
                        if (demoExist)
                        {
                            outputString = GenerateOutput(template, dataJson, settingsJson, null);
                        }
                    }
                }
                else if (template != null)
                {
                    bool demoExist = GetDemoData(template, out dataJson, out settingsJson);
                    if (demoExist)
                    {
                        outputString = GenerateOutput(template, dataJson, settingsJson, null);
                    }
                }
            }
            if (!string.IsNullOrEmpty(outputString))
            {
                var lit = new LiteralControl(Server.HtmlDecode(outputString));
                Controls.Add(lit);
                //bool EditWitoutPostback = HostController.Instance.GetBoolean("EditWitoutPostback", false);
                var mst = OpenContentUtils.GetManifest(template.Directory);
                bool EditWitoutPostback = mst != null && mst.EditWitoutPostback;
                if (ModuleContext.PortalSettings.EnablePopUps && ModuleContext.IsEditable && EditWitoutPostback)
                {
                    AJAX.WrapUpdatePanelControl(lit, true);
                }
                IncludeResourses(template);
                //if (DemoData) pDemo.Visible = true;
            }
        }

        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        private string GenerateOutput(FileUri template, string dataJson, string settingsJson, TemplateFiles files)
        {
            try
            {
                if (template != null)
                {
                    if (!template.FileExists)
                        Exceptions.ProcessModuleLoadException(this, new Exception(template + " don't exist"));

                    string TemplateVirtualFolder = template.Directory;
                    string TemplateFolder = Server.MapPath(TemplateVirtualFolder);
                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        if (LocaleController.Instance.GetLocales(ModuleContext.PortalId).Count > 1)
                        {
                            dataJson = JsonUtils.SimplifyJson(dataJson, LocaleController.Instance.GetCurrentLocale(ModuleContext.PortalId).Code);
                        }
                        dynamic model = JsonUtils.JsonToDynamic(dataJson);

                        CompleteModel(settingsJson, TemplateFolder, model, files);
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

        private string ExecuteRazor(FileUri template, dynamic model)
        {
            string webConfig = template.PhysicalDirectoryName; // Path.GetDirectoryName(template.PhysicalFilePath);
            webConfig = webConfig.Remove(webConfig.LastIndexOf("\\")) + "\\web.config";
            if (!File.Exists(webConfig))
            {
                string filename = HostingEnvironment.MapPath("~/DesktopModules/OpenContent/Templates/web.config");
                File.Copy(filename, webConfig);
            }
            try
            {
                var razorEngine = new RazorEngine("~" + template.FilePath, ModuleContext, LocalResourceFile);
                var writer = new StringWriter();
                RazorRender(razorEngine.Webpage, writer, model);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(string.Format("Error while loading template {0}", template), this, ex);
                return "";
            }
        }
        private string GenerateListOutput(string TemplateVirtualFolder, TemplateFiles files, IEnumerable<OpenContentInfo> dataList, string settingsJson)
        {
            try
            {
                if (!(string.IsNullOrEmpty(files.Template)))
                {
                    string PhysicalTemplateFolder = Server.MapPath(TemplateVirtualFolder);
                    FileUri Template = CheckFiles(TemplateVirtualFolder, files, PhysicalTemplateFolder);
                    if (dataList != null && dataList.Any())
                    {
                        dynamic model = new ExpandoObject();
                        model.Items = new List<dynamic>();
                        foreach (var item in dataList)
                        {
                            string dataJson = item.Json;
                            if (LocaleController.Instance.GetLocales(ModuleContext.PortalId).Count > 1)
                            {
                                dataJson = JsonUtils.SimplifyJson(dataJson, LocaleController.Instance.GetCurrentLocale(ModuleContext.PortalId).Code);
                            }
                            dynamic dyn = JsonUtils.JsonToDynamic(dataJson);
                            dyn.Context = new ExpandoObject();

                            dyn.Context.Id = item.ContentId;
                            dyn.Context.EditUrl = ModuleContext.EditUrl("id", item.ContentId.ToString());
                            dyn.Context.DetailUrl = Globals.NavigateURL(ModuleContext.TabId, false, ModuleContext.PortalSettings, "", ModuleContext.PortalSettings.CultureCode, OpenContentUtils.CleanupUrl(item.Title), "id=" + item.ContentId.ToString());

                            model.Items.Add(dyn);
                        }
                        CompleteModel(settingsJson, PhysicalTemplateFolder, model, files);
                        return ExecuteTemplate(TemplateVirtualFolder, files, Template, model);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
            return "";
        }

        private void CompleteModel(string settingsJson, string PhysicalTemplateFolder, dynamic model, TemplateFiles manifest)
        {
            if (manifest != null && manifest.SchemaInTemplate)
            {
                // schema
                string schemaFilename = PhysicalTemplateFolder + "\\" + "schema.json";
                try
                {
                    dynamic schema = JsonUtils.JsonToDynamic(File.ReadAllText(schemaFilename));
                    model.Schema = schema;
                }
                catch (Exception ex)
                {
                    Exceptions.ProcessModuleLoadException(string.Format("Invalid json-schema. Please verify file {0}.", schemaFilename), this, ex, true);
                }
            }
            if (manifest != null && manifest.OptionsInTemplate)
            {
                // options
                JToken optionsJson = null;
                // default options
                string optionsFilename = PhysicalTemplateFolder + "\\" + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = PhysicalTemplateFolder + "\\" + "options." + ModuleContext.PortalSettings.CultureCode + ".json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        var extraJson = fileContent.ToJObject("Options cultureSpecific");
                        if (optionsJson == null)
                            optionsJson = extraJson;
                        else
                            optionsJson = optionsJson.JsonMerge(extraJson);
                    }
                }
                if (optionsJson != null)
                {
                    dynamic Options = JsonUtils.JsonToDynamic(optionsJson.ToString());
                    model.Options = Options;
                }
            }
            // settings
            if (settingsJson != null)
            {
                model.Settings = JsonUtils.JsonToDynamic(settingsJson);
            }
            // context
            model.Context = new ExpandoObject();
            model.Context.ModuleId = ModuleContext.ModuleId;
            model.Context.IsEditable = ModuleContext.IsEditable;
            model.Context.PortalId = ModuleContext.PortalId;
        }
        private string GenerateOutput(string TemplateVirtualFolder, TemplateFiles files, string dataJson, string settingsJson)
        {
            try
            {
                if (!(string.IsNullOrEmpty(files.Template)))
                {
                    string PhysicalTemplateFolder = Server.MapPath(TemplateVirtualFolder);
                    FileUri template = CheckFiles(TemplateVirtualFolder, files, PhysicalTemplateFolder);

                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        if (LocaleController.Instance.GetLocales(ModuleContext.PortalId).Count > 1)
                        {
                            dataJson = JsonUtils.SimplifyJson(dataJson, LocaleController.Instance.GetCurrentLocale(ModuleContext.PortalId).Code);
                        }
                        dynamic model = JsonUtils.JsonToDynamic(dataJson);
                        CompleteModel(settingsJson, PhysicalTemplateFolder, model, files);

                        return ExecuteTemplate(TemplateVirtualFolder, files, template, model);
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

        private string ExecuteTemplate(string TemplateVirtualFolder, TemplateFiles files, FileUri template, dynamic model)
        {
            if (template.Extension != ".hbs")
            {
                return ExecuteRazor(template, model);
            }
            else
            {
                HandlebarsEngine hbEngine = new HandlebarsEngine();
                return hbEngine.Execute(Page, this, files, TemplateVirtualFolder, model);
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

        private bool GetData(out string dataJson, out string settingsJson)
        {
            dataJson = "";
            settingsJson = "";
            OpenContentController ctrl = new OpenContentController();
            var struc = ctrl.GetFirstContent(ModuleContext.ModuleId);
            if (struc != null)
            {
                dataJson = struc.Json;
                settingsJson = ModuleContext.Settings["data"] as string;
                return true;
            }
            return false;
        }
        private bool GetData(int ContentId, out string dataJson, out string settingsJson)
        {
            dataJson = "";
            settingsJson = "";
            OpenContentController ctrl = new OpenContentController();
            var struc = ctrl.GetContent(ContentId, ModuleContext.ModuleId);
            if (struc != null)
            {
                dataJson = struc.Json;
                settingsJson = ModuleContext.Settings["data"] as string;
                Page.Title = struc.Title + " | " + ModuleContext.PortalSettings.PortalName;
                return true;
            }
            return false;
        }
        private bool GetDataList(out IEnumerable<OpenContentInfo> dataList, out string settingsJson)
        {
            settingsJson = "";
            OpenContentController ctrl = new OpenContentController();
            dataList = ctrl.GetContents(ModuleContext.ModuleId);
            if (dataList != null && dataList.Any())
            {
                settingsJson = ModuleContext.Settings["data"] as string;
                return true;
            }
            return false;
        }
        private bool GetDemoData(FileUri template, out string dataJson, out string settingsJson)
        {
            dataJson = "";
            settingsJson = "";
            OpenContentController ctrl = new OpenContentController();
            var dataFilename = template.PhysicalDirectoryName + "\\" + "data.json";
            if (File.Exists(dataFilename))
            {
                string fileContent = File.ReadAllText(dataFilename);
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    dataJson = fileContent;
                }
            }
            if (template == (OpenContentUtils.GetTemplate(ModuleContext.Settings)))
            {
                settingsJson = ModuleContext.Settings["data"] as string;
            }
            if (string.IsNullOrEmpty(settingsJson))
            {
                var settingsFilename = template.PhysicalDirectoryName + "\\" + template.FileNameWithoutExtension + "-data.json";
                if (File.Exists(settingsFilename))
                {
                    string fileContent = File.ReadAllText(settingsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        settingsJson = fileContent;
                    }
                }
            }
            return !string.IsNullOrWhiteSpace(dataJson);
        }
        #endregion
        public DotNetNuke.Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();

                FileUri template = OpenContentUtils.GetTemplate(ModuleContext.Settings);
                bool templateDefined = template != null;
                TemplateManifest manifest = null;
                if (templateDefined)
                {
                    manifest = OpenContentUtils.GetTemplateManifest(template);
                }

                bool listMode = manifest != null && manifest.IsListTemplate;
                if (Page.Request.QueryString["id"] != null)
                {
                    int.TryParse(Page.Request.QueryString["id"], out _itemId);
                }

                if (templateDefined)
                {
                    Actions.Add(ModuleContext.GetNextActionID(),
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
                Actions.Add(ModuleContext.GetNextActionID(),
                         Localization.GetString("EditSettings.Action", LocalResourceFile),
                         ModuleActionType.ContentOptions,
                         "",
                         "~/DesktopModules/OpenContent/images/settings.gif",
                         ModuleContext.EditUrl("EditSettings"),
                         false,
                         SecurityAccessLevel.Host,
                         true,
                         false);

                if (templateDefined)
                    Actions.Add(ModuleContext.GetNextActionID(),
                               Localization.GetString("EditTemplate.Action", LocalResourceFile),
                               ModuleActionType.ContentOptions,
                               "",
                               "~/DesktopModules/OpenContent/images/edittemplate.png",
                               ModuleContext.EditUrl("EditTemplate"),
                               false,
                               SecurityAccessLevel.Host,
                               true,
                               false);
                if (templateDefined && !listMode)
                    Actions.Add(ModuleContext.GetNextActionID(),
                               Localization.GetString("EditData.Action", LocalResourceFile),
                               ModuleActionType.EditContent,
                               "",
                               "~/DesktopModules/OpenContent/images/edit.png",
                               ModuleContext.EditUrl("EditData"),
                               false,
                               SecurityAccessLevel.Host,
                               true,
                               false);

                Actions.Add(ModuleContext.GetNextActionID(),
                           Localization.GetString("ShareTemplate.Action", LocalResourceFile),
                           ModuleActionType.ContentOptions,
                           "",
                           "~/DesktopModules/OpenContent/images/exchange.png",
                           ModuleContext.EditUrl("ShareTemplate"),
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
                Actions.Add(ModuleContext.GetNextActionID(),
                          Localization.GetString("Help.Action", LocalResourceFile),
                          ModuleActionType.ContentOptions,
                          "",
                          "~/DesktopModules/OpenContent/images/help.png",
                          "https://opencontent.codeplex.com/documentation",
                          false,
                          SecurityAccessLevel.Host,
                          true,
                          true);


                return Actions;
            }
        }

        public void RazorRender(WebPageBase Webpage, TextWriter writer, dynamic model)
        {
            var HttpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
            if ((Webpage) is DotNetNukeWebPage<dynamic>)
            {
                var mv = (DotNetNukeWebPage<dynamic>)Webpage;
                mv.Model = model;
            }
            if (Webpage != null)
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, null), writer, Webpage);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Page.Request.QueryString["id"] != null)
            {
                int.TryParse(Page.Request.QueryString["id"], out _itemId);
            }

            //string Template = OpenContentUtils.GetTemplateFolder(ModuleContext.Settings);
            //string settingsJson = ModuleContext.Settings["data"] as string;
            if (!Page.IsPostBack)
            {
            }

        }

        private void IncludeResourses(FileUri template)
        {
            if (template != null)
            {
                //JavaScript.RequestRegistration() 
                //string templateBase = template.FilePath.Replace("$.hbs", ".hbs");
                string cssfilename = Path.ChangeExtension(template.FilePath, "css");
                if (File.Exists(HostingEnvironment.MapPath(cssfilename)))
                {
                    ClientResourceManager.RegisterStyleSheet(Page, Page.ResolveUrl(cssfilename), FileOrder.Css.PortalCss);
                }
                string jsfilename = Path.ChangeExtension(template.FilePath, "js");
                if (File.Exists(HostingEnvironment.MapPath(jsfilename)))
                {
                    ClientResourceManager.RegisterScript(Page, Page.ResolveUrl(jsfilename), FileOrder.Js.DefaultPriority);
                }
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
            }
        }

        protected void rblUseTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            phFrom.Visible = rblUseTemplate.SelectedIndex == 1;
            phTemplateName.Visible = rblUseTemplate.SelectedIndex == 1;
            rblFrom.SelectedIndex = 0;
            var scriptFileSetting = OpenContentUtils.GetTemplate(ModuleContext.Settings);
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
                var scriptFileSetting = OpenContentUtils.GetTemplate(ModuleContext.Settings);
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());

                //ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());
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
                if (rblUseTemplate.SelectedIndex == 0) // existing
                {
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", ddlTemplate.SelectedValue);
                    //mc.UpdateModuleSetting(ModuleId, "data", HiddenField.Value);
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
                mc.DeleteModuleSetting(ModuleContext.ModuleId, "data");
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
    }
}