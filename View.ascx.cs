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
        private int ItemId = Null.NullInteger;
        protected override string RazorScriptFile
        {
            get
            {
                // string m_RazorScriptFile = base.RazorScriptFile;
                var m_RazorScriptFile = "";
                string Template = ModuleContext.Settings["template"] as string;

                if (pHelp.Visible && rblUseTemplate.SelectedIndex == 0)
                {
                    Template = ddlTemplate.SelectedValue;
                }
                if (!(string.IsNullOrEmpty(Template)))
                {
                    m_RazorScriptFile = "~/" + Template;
                }
                return m_RazorScriptFile;
            }
        }
        /*
        public bool ListMode
        {
            get
            {
                bool ListMode = false;
                var ListSetting = ModuleContext.Settings["list"] as string;
                return ListSetting != null && bool.TryParse(ListSetting, out ListMode) && ListMode;
            }
        }
        */
        protected override void OnPreRender(EventArgs e)
        {
            pHelp.Visible = false;
            //base.OnPreRender(e);

            string settingsJson;
            string OutputString = "";
            var Template = ModuleContext.Settings["template"] as string;
            bool dataExist = false;
            if (OpenContentUtils.IsListTemplate(Template))
            {
                if (ItemId == Null.NullInteger)
                {
                    IEnumerable<OpenContentInfo> dataList;
                    dataExist = GetDataList(out dataList, out settingsJson);
                    if (dataExist)
                    {
                        OutputString = GenerateListOutput(Template, dataList, settingsJson);
                    }
                }
                else
                {
                    Template = Template.Replace(".hbs", "detail.hbs");
                    string dataJson;
                    dataExist = GetData(ItemId, out dataJson, out settingsJson);
                    if (dataExist)
                    {
                        OutputString = GenerateOutput(Template, dataJson, settingsJson);
                    }
                }
                
            }
            else
            {
                string dataJson;
                dataExist = GetData(out dataJson, out settingsJson);
                if (dataExist)
                {
                    OutputString = GenerateOutput(Template, dataJson, settingsJson);
                }
            }
            if (!dataExist)
            {
                string dataJson;
                if (ModuleContext.EditMode)
                {
                    if (string.IsNullOrEmpty(Template) || ModuleContext.IsEditable)
                    {
                        pHelp.Visible = true;
                        if (!Page.IsPostBack)
                        {
                            ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, Template, "OpenContent").ToArray());
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
                        if (rblUseTemplate.SelectedIndex == 0)
                        {
                            Template = ddlTemplate.SelectedValue;
                            bool demoExist = GetDemoData(Template, out dataJson, out settingsJson);
                            if (demoExist)
                            {
                                OutputString = GenerateOutput(Template, dataJson, settingsJson);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(Template))
                    {
                        bool demoExist = GetDemoData(Template, out dataJson, out settingsJson);
                        if (demoExist)
                        {
                            OutputString = GenerateOutput(Template, dataJson, settingsJson);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(Template))
                {
                    bool demoExist = GetDemoData(Template, out dataJson, out settingsJson);
                    if (demoExist)
                    {
                        OutputString = GenerateOutput(Template, dataJson, settingsJson);
                    }
                }
            }
            if (!string.IsNullOrEmpty(OutputString))
            {
                var lit = new LiteralControl(Server.HtmlDecode(OutputString));
                Controls.Add(lit);
                if (ModuleContext.PortalSettings.EnablePopUps && ModuleContext.IsEditable && HostController.Instance.GetBoolean("EditWitoutPostback", false))
                {
                    AJAX.WrapUpdatePanelControl(lit, true);
                }
                IncludeResourses(Template);
                //if (DemoData) pDemo.Visible = true;
            }
        }

        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        private string GenerateOutput(string Template, string dataJson, string settingsJson)
        {

            try
            {
                if (!(string.IsNullOrEmpty(Template)))
                {
                    if (!File.Exists(Server.MapPath(Template)))
                        Exceptions.ProcessModuleLoadException(this, new Exception(Template + " don't exist"));

                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        dynamic model = JsonUtils.JsonToDynamic(dataJson);

                        string TemplateFilename = HostingEnvironment.MapPath("~/" + Template);
                        // schema
                        string schemaFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "schema.json";
                        model.Schema = JsonUtils.JsonToDynamic(File.ReadAllText(schemaFilename));



                        if (settingsJson != null)
                            model.Settings = JsonUtils.JsonToDynamic(settingsJson);
                        model.Context = new
                        {
                            ModuleId = ModuleContext.ModuleId,
                            IsEditable = ModuleContext.IsEditable,
                            PortalId = ModuleContext.PortalId
                        };

                        if (Path.GetExtension(Template) != ".hbs")
                        {
                            string webConfig = Path.GetDirectoryName(Server.MapPath(Template));
                            webConfig = webConfig.Remove(webConfig.LastIndexOf("\\")) + "\\web.config";
                            if (!File.Exists(webConfig))
                            {
                                string filename = HostingEnvironment.MapPath("~/DesktopModules/OpenContent/Templates/web.config");
                                File.Copy(filename, webConfig);
                            }
                            try
                            {
                                var razorEngine = new RazorEngine(Template, ModuleContext, LocalResourceFile);
                                var writer = new StringWriter();
                                RazorRender(razorEngine.Webpage, writer, model);
                                return writer.ToString();
                            }
                            catch (Exception ex)
                            {
                                Exceptions.ProcessModuleLoadException(string.Format("Error while loading template {0}", Template), this, ex);
                            }
                        }
                        else
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            //return hbEngine.Execute(Page, Template, model);
                            return hbEngine.Execute(Page, this, Template, Template, model);
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
        private string GenerateListOutput(string Template, IEnumerable<OpenContentInfo> dataList, string settingsJson)
        {
            try
            {
                if (!(string.IsNullOrEmpty(Template)))
                {
                    if (!File.Exists(Server.MapPath(Template)))
                        Exceptions.ProcessModuleLoadException(this, new Exception(Template + " don't exist"));

                    if (dataList != null && dataList.Any())
                    {
                        //dynamic model = JsonUtils.JsonToDynamic(dataJson);
                        dynamic model = new ExpandoObject();
                        model.Items = new List<dynamic>();
                        foreach (var item in dataList)
                        {
                            dynamic dyn = JsonUtils.JsonToDynamic(item.Json);
                            dyn.Context = new
                            {
                                Id = item.ContentId,
                                EditUrl = ModuleContext.EditUrl("id", item.ContentId.ToString()),
                                DetailUrl = Globals.NavigateURL(ModuleContext.TabId, false, ModuleContext.PortalSettings, "", ModuleContext.PortalSettings.CultureCode, OpenContentUtils.CleanupUrl(item.Title), "id=" + item.ContentId.ToString())
                            };

                            model.Items.Add(dyn);
                        }

                        string TemplateFilename = HostingEnvironment.MapPath("~/" + Template);
                        // schema
                        string schemaFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "schema.json";

                        dynamic schema = JsonUtils.JsonToDynamic(File.ReadAllText(schemaFilename));
                        model.Schema = schema;

                        JToken optionsJson = null;
                        // default options
                        string optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "options.json";
                        if (File.Exists(optionsFilename))
                        {
                            string fileContent = File.ReadAllText(optionsFilename);
                            if (!string.IsNullOrWhiteSpace(fileContent))
                            {
                                optionsJson = JObject.Parse(fileContent);
                            }
                        }
                        // language options
                        optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "options." + ModuleContext.PortalSettings.CultureCode + ".json";
                        if (File.Exists(optionsFilename))
                        {
                            string fileContent = File.ReadAllText(optionsFilename);
                            if (!string.IsNullOrWhiteSpace(fileContent))
                            {
                                if (optionsJson == null)
                                    optionsJson = JObject.Parse(fileContent);
                                else 
                                    optionsJson = optionsJson.JsonMerge(JObject.Parse(fileContent));
                            }
                        }
                        if (optionsJson != null)
                        {
                            dynamic Options = JsonUtils.JsonToDynamic(optionsJson.ToString());
                            model.Options = Options;
                        }

                        if (settingsJson != null)
                            model.Settings = JsonUtils.JsonToDynamic(settingsJson);
                        model.Context = new
                        {
                            ModuleId = ModuleContext.ModuleId,
                            IsEditable = ModuleContext.IsEditable,
                            PortalId = ModuleContext.PortalId
                        };

                        if (Path.GetExtension(Template) != ".hbs")
                        {
                            string webConfig = Path.GetDirectoryName(Server.MapPath(Template));
                            webConfig = webConfig.Remove(webConfig.LastIndexOf("\\")) + "\\web.config";
                            if (!File.Exists(webConfig))
                            {
                                string filename = HostingEnvironment.MapPath("~/DesktopModules/OpenContent/Templates/web.config");
                                File.Copy(filename, webConfig);
                            }
                            try
                            {
                                var razorEngine = new RazorEngine(Template, ModuleContext, LocalResourceFile);
                                var writer = new StringWriter();
                                RazorRender(razorEngine.Webpage, writer, model);
                                return writer.ToString();
                            }
                            catch (Exception ex)
                            {
                                Exceptions.ProcessModuleLoadException(string.Format("Error while loading template {0}", Template), this, ex);
                            }
                        }
                        else
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            //return hbEngine.Execute(Page, Template, model);
                            return hbEngine.Execute(Page, this, Template, Template.Replace(".hbs","item.hbs"), model);
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
                Page.Title = struc.Title + " | "+ModuleContext.PortalSettings.PortalName;
                return true;
            }
            return false;
        }

        private bool GetDataList(out IEnumerable<OpenContentInfo> dataList, out string settingsJson)
        {
            settingsJson = "";
            OpenContentController ctrl = new OpenContentController();
            dataList = ctrl.GetContents(ModuleContext.ModuleId);
            if (dataList != null)
            {
                settingsJson = ModuleContext.Settings["data"] as string;
                return true;
            }
            return false;
        }


        private bool GetDemoData(string Template, out string dataJson, out string settingsJson)
        {
            dataJson = "";
            settingsJson = "";
            OpenContentController ctrl = new OpenContentController();
            var dataFilename = Path.GetDirectoryName(Server.MapPath(Template)) + "\\" + "data.json";
            if (File.Exists(dataFilename))
            {
                string fileContent = File.ReadAllText(dataFilename);
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    dataJson = fileContent;
                }
            }
            if (Template == (ModuleContext.Settings["template"] as string))
            {
                settingsJson = ModuleContext.Settings["data"] as string;
            }
            if (string.IsNullOrEmpty(settingsJson))
            {
                var settingsFilename = Path.GetDirectoryName(Server.MapPath(Template)) + "\\" + Path.GetFileNameWithoutExtension(Template) + "-data.json";
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

                string Template = ModuleContext.Settings["template"] as string;
                bool TemplateDefined = !string.IsNullOrEmpty(Template);
                bool ListMode = OpenContentUtils.IsListTemplate(Template);
                if (Page.Request.QueryString["id"] != null)
                {
                    int.TryParse(Page.Request.QueryString["id"], out ItemId);
                }

                if (TemplateDefined)
                {
                    Actions.Add(ModuleContext.GetNextActionID(),
                        Localization.GetString((ListMode && ItemId == Null.NullInteger ? ModuleActionType.AddContent : ModuleActionType.EditContent), LocalResourceFile),
                                ModuleActionType.AddContent,
                                "",
                                "",
                                (ListMode && ItemId != Null.NullInteger ? ModuleContext.EditUrl("id",ItemId.ToString()) : ModuleContext.EditUrl()) ,
                                false,
                                SecurityAccessLevel.Edit,
                                true,
                                false);
                }
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

                if (TemplateDefined)
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
                if (TemplateDefined && !ListMode)
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
                int.TryParse(Page.Request.QueryString["id"], out ItemId);
            }

            string Template = ModuleContext.Settings["template"] as string;
            if (!Page.IsPostBack)
            {
            }
            bool TemplateDefined = !string.IsNullOrEmpty(Template);
            if (ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                //hlTempleteExchange.NavigateUrl = ModuleContext.EditUrl("ShareTemplate");
                hlEditSettings.NavigateUrl = ModuleContext.EditUrl("EditSettings");
                //hlTempleteExchange.Visible = true;
                hlEditSettings.Visible = true;
            }
            if (TemplateDefined && ModuleContext.EditMode)
            {
                hlEditContent.NavigateUrl = ModuleContext.EditUrl("Edit");
                hlEditContent.Visible = true;
                hlEditContent2.NavigateUrl = ModuleContext.EditUrl("Edit");
                hlEditContent2.Visible = true;
            }
        }

        private void IncludeResourses(string Template)
        {
            if (!(string.IsNullOrEmpty(Template)))
            {
                //JavaScript.RequestRegistration() 
                string TemplateBase = Template.Replace("$.hbs", ".hbs");
                string cssfilename = Path.ChangeExtension(TemplateBase, "css");
                if (File.Exists(HostingEnvironment.MapPath(cssfilename)))
                {
                    ClientResourceManager.RegisterStyleSheet(Page, Page.ResolveUrl(cssfilename), FileOrder.Css.PortalCss);
                }
                string jsfilename = Path.ChangeExtension(TemplateBase, "js");
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
            var scriptFileSetting = ModuleContext.Settings["template"] as string;
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
                var scriptFileSetting = ModuleContext.Settings["template"] as string;
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
                        string Template = OpenContentUtils.CopyTemplate(ModuleContext.PortalId, oldFolder, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", Template);
                    }
                    else if (rblFrom.SelectedIndex == 1) // web
                    {
                        string FileName = ddlTemplate.SelectedValue;
                        string Template = OpenContentUtils.ImportFromWeb(ModuleContext.PortalId, FileName, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", Template);
                    }
                }
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