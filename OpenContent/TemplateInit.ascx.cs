using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Rss;
using Satrabel.OpenContent.Components.Logging;
using DotNetNuke.Services.Localization;
using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent
{
    public partial class TemplateInit : System.Web.UI.UserControl
    {
        public bool PageRefresh { get; set; }
        public ModuleInstanceContext ModuleContext { get; set; }
        public OpenContentSettings Settings { get; set; }
        public RenderInfo Renderinfo { get; set; }
        public bool RenderOnlySaveButton { get; set; }
        public string ResourceFile { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            pHelp.Visible = false;
            phCurrentTemplate.Visible = false;

            foreach (ListItem item in rblDataSource.Items)
            {
                string key = item.Attributes["ResourceKey"];
                if (key != null)
                {
                    item.Text = Localization.GetString(key + ".Text", ResourceFile);
                }
            }
            foreach (ListItem item in rblUseTemplate.Items)
            {
                string key = item.Attributes["ResourceKey"];
                if (key != null)
                {
                    item.Text = Localization.GetString(key + ".Text", ResourceFile);
                }
            }
            foreach (ListItem item in rblFrom.Items)
            {
                string key = item.Attributes["ResourceKey"];
                if (key != null)
                {
                    item.Text = Localization.GetString(key + ".Text", ResourceFile);
                }
            }
        }

        public string Resource(string key)
        {
            return Localization.GetString(key + ".Text", ResourceFile);
        }
        protected void rblFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTemplate.Items.Clear();
            if (rblFrom.SelectedIndex == 0) // site
            {
                var scriptFileSetting = ModuleContext.OpenContentSettings().Template;
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, App.Config.Opencontent).ToArray());
            }
            else if (rblFrom.SelectedIndex == 1) // web
            {
                FeedParser parser = new FeedParser();
                //var items = parser.Parse("http://www.openextensions.net/templates?agentType=rss&PropertyTypeID=9", FeedType.RSS);
                //foreach (var item in items.OrderBy(t => t.Title))
                //{
                //    ddlTemplate.Items.Add(new ListItem(item.Title, item.ZipEnclosure));
                //}
                //if (ddlTemplate.Items.Count > 0)
                //{
                //    tbTemplateName.Text = Path.GetFileNameWithoutExtension(ddlTemplate.Items[0].Value);
                //}


                foreach (var item in GithubTemplateUtils.GetTemplateList(ModuleContext.PortalId).Where(t => t.Type == Components.Github.TypeEnum.Dir).OrderBy(t => t.Name))
                {
                    ddlTemplate.Items.Add(new ListItem(item.Name, item.Path));
                }
                if (ddlTemplate.Items.Count > 0)
                {
                    tbTemplateName.Text = Path.GetFileNameWithoutExtension(ddlTemplate.Items[0].Value);
                }
            }
            ActivateDetailPage();
        }

        protected void rblDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblDataSource.SelectedIndex == 1) // other module
            {
                BindOtherModules(-1, -1);
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                var dsSettings = dsModule.OpenContentSettings();
                BindTemplates(dsSettings.Template, dsSettings.Template.MainTemplateUri());
            }
            if (rblDataSource.SelectedIndex == 2) // other portal
            {
                BindOtherPortals(-1);
                BindOtherModules(-1, -1);
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                var dsSettings = dsModule.OpenContentSettings();
                BindTemplates(dsSettings.Template, dsSettings.Template.MainTemplateUri());
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
            var scriptFileSetting = ModuleContext.OpenContentSettings().Template;
            ddlTemplate.Items.Clear();
            if (rblUseTemplate.SelectedIndex == 0) // existing
            {
                ddlTemplate.Items.AddRange(OpenContentUtils.ListOfTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, App.Config.Opencontent).ToArray());
            }
            else if (rblUseTemplate.SelectedIndex == 1) // new
            {

                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, App.Config.Opencontent).ToArray());
            }
            ActivateDetailPage();
        }

        protected void bSave_Click(object sender, EventArgs e)
        {
            try
            {
                ModuleController mc = new ModuleController();
                if (rblDataSource.SelectedIndex == 0) // this module
                {
                    mc.DeleteModuleSetting(ModuleContext.ModuleId, "portalid");
                    mc.DeleteModuleSetting(ModuleContext.ModuleId, "tabid");
                    mc.DeleteModuleSetting(ModuleContext.ModuleId, "moduleid");
                }
                if (rblDataSource.SelectedIndex == 0) // other module
                {
                    var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                    mc.DeleteModuleSetting(ModuleContext.ModuleId, "portalid");
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "tabid", dsModule.TabID.ToString());
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "moduleid", dsModule.ModuleID.ToString());
                }
                else // other portal
                {
                    var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                    var dsPortal = int.Parse(ddlPortals.SelectedValue);
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "portalid", dsModule.TabID.ToString());
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "tabid", dsModule.TabID.ToString());
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "moduleid", dsModule.ModuleID.ToString());
                }
                if (rblUseTemplate.SelectedIndex == 0) // existing
                {
                    mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", ddlTemplate.SelectedValue);
                    ModuleContext.Settings["template"] = ddlTemplate.SelectedValue;
                }
                else if (rblUseTemplate.SelectedIndex == 1) // new
                {

                    if (rblFrom.SelectedIndex == 0) // site
                    {
                        string oldFolder = Server.MapPath(ddlTemplate.SelectedValue);
                        string template = OpenContentUtils.CopyTemplate(ModuleContext.PortalId, oldFolder, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                        ModuleContext.Settings["template"] = template;
                    }
                    else if (rblFrom.SelectedIndex == 1) // web
                    {
                        //string fileName = ddlTemplate.SelectedValue;
                        //string template = OpenContentUtils.ImportFromWeb(ModuleContext.PortalId, fileName, tbTemplateName.Text);
                        //mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                        //ModuleContext.Settings["template"] = template;
                        //string fileName = ddlTemplate.SelectedValue;
                        string template = GithubTemplateUtils.ImportFromGithub(ModuleContext.PortalId, ddlTemplate.SelectedItem.Text, ddlTemplate.SelectedValue, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                        ModuleContext.Settings["template"] = template;
                    }
                }
                mc.UpdateModuleSetting(ModuleContext.ModuleId, "detailtabid", ddlDetailPage.SelectedValue);


                //don't reset settings. Sure they might be invalid, but maybe not. And you can't ever revert.
                //mc.DeleteModuleSetting(ModuleContext.ModuleId, "data");

                Settings = ModuleContext.OpenContentSettings();
                if (PageRefresh || !Settings.Template.DataNeeded())
                {
                    Response.Redirect(Globals.NavigateURL(), true);
                }
                else
                {
                    rblUseTemplate.SelectedIndex = 0;
                    phTemplateName.Visible = rblUseTemplate.SelectedIndex == 1;
                    phFrom.Visible = rblUseTemplate.SelectedIndex == 1;
                    rblFrom.SelectedIndex = 0;
                    BindTemplates(Settings.Template, null);
                    Renderinfo.Template = Settings.Template;
                    BindButtons(Settings, Renderinfo);
                    ActivateDetailPage();
                }
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
            ActivateDetailPage();
        }

        protected void ddlDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
            var dsSettings = dsModule.OpenContentSettings();
            BindTemplates(dsSettings.Template, dsSettings.Template.MainTemplateUri());
        }

        private void BindTemplates(TemplateManifest template, FileUri otherModuleTemplate)
        {
            var SelectedPortalSettings = ModuleContext.PortalSettings;
            if (rblDataSource.SelectedIndex == 2)// other portal
            {
                SelectedPortalSettings = new PortalSettings(int.Parse(ddlPortals.SelectedValue));
            }
            ddlTemplate.Items.Clear();
            ddlTemplate.Items.AddRange(OpenContentUtils.ListOfTemplatesFiles(SelectedPortalSettings, ModuleContext.ModuleId, template, App.Config.Opencontent, otherModuleTemplate).ToArray());
            if (ddlTemplate.Items.Count == 0)
            {
                rblUseTemplate.Items[0].Enabled = false;
                rblUseTemplate.SelectedIndex = 1;
                rblUseTemplate_SelectedIndexChanged(null, null);
                rblFrom.Items[0].Enabled = false;
                rblFrom.SelectedIndex = 1;
                rblFrom_SelectedIndexChanged(null, null);

            }
            ActivateDetailPage();
        }

        private void BindButtons(OpenContentSettings settings, RenderInfo info)
        {
            bool templateDefined = info.Template != null;
            bool settingsDefined = !string.IsNullOrEmpty(settings.Data);
            bool settingsNeeded = false;

            if (rblUseTemplate.SelectedIndex == 0 && ddlTemplate.SelectedIndex >= 0) // existing template
            {
                //create tmp TemplateManifest
                var templateManifest = new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest();
                settingsNeeded = templateManifest.SettingsNeeded();

                templateDefined = templateDefined && (!ddlTemplate.Visible || (settings.Template.Key.ToString() == ddlTemplate.SelectedValue));
                settingsDefined = settingsDefined || !settingsNeeded;
            }
            else // new template
            {
                templateDefined = false;
            }

            if (!templateDefined && !settings.FirstTimeInitialisation && ddlTemplate.Items.FindByValue(settings.TemplateKey.ToString()) == null)
            {
                lCurrentTemplate.Text = settings.TemplateKey.ToString();
                phCurrentTemplate.Visible = true;
            }

            bSave.CssClass = "dnnPrimaryAction";
            bSave.Enabled = true;
            hlEditSettings.CssClass = "dnnSecondaryAction";
            hlEditContent.CssClass = "dnnSecondaryAction";
            //if (ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            hlEditSettings.Enabled = false;
            hlEditSettings.Visible = settingsNeeded && !RenderOnlySaveButton;

            if (templateDefined && ModuleContext.EditMode && settingsNeeded)
            {
                //hlTempleteExchange.NavigateUrl = ModuleContext.EditUrl("ShareTemplate");
                hlEditSettings.NavigateUrl = ModuleContext.EditUrl("EditSettings");
                //hlTempleteExchange.Visible = true;
                hlEditSettings.Enabled = true;

                //bSave.CssClass = "dnnSecondaryAction";
                //bSave.Enabled = false;
                //hlEditSettings.CssClass = "dnnPrimaryAction";
                //hlEditContent.CssClass = "dnnSecondaryAction";

            }
            hlEditContent.Visible = !RenderOnlySaveButton;
            hlEditContent2.Visible = !RenderOnlySaveButton;
            hlEditContent.Enabled = false;
            hlEditContent2.Enabled = false;
            if (templateDefined && settingsDefined && ModuleContext.EditMode)
            {
                hlEditContent.NavigateUrl = ModuleContext.EditUrl("Edit");
                hlEditContent.Enabled = true;
                hlEditContent2.NavigateUrl = ModuleContext.EditUrl("Edit");
                hlEditContent2.Enabled = true;
                //bSave.CssClass = "dnnSecondaryAction";
                //bSave.Enabled = false;
                //hlEditSettings.CssClass = "dnnSecondaryAction";
                //hlEditContent.CssClass = "dnnPrimaryAction";

                var template = new FileUri(ddlTemplate.SelectedValue);
                var manifest = template.ToTemplateManifest();
                hlEditContent.Text = App.Services.Localizer.GetString(manifest.IsListTemplate ? "Add.Action" : "Edit.Action", ResourceFile);
                if (!string.IsNullOrEmpty(manifest.Title))
                {
                    hlEditContent.Text = hlEditContent.Text + " " + manifest.Title;
                }
            }
        }
        public void RenderInitForm()
        {
            Renderinfo.ShowDemoData = false;
            pHelp.Visible = true;
            if (!Page.IsPostBack || ddlTemplate.Items.Count == 0)
            {
                rblDataSource.SelectedIndex = (Settings.IsOtherPortal ? 2 : (Settings.IsOtherModule ? 1 : 0) );
                BindOtherPortals(Settings.PortalId);
                BindOtherModules(Settings.TabId, Settings.ModuleId);

                BindTemplates(Settings.Template, (Renderinfo.IsOtherModule ? Renderinfo.Template.MainTemplateUri() : null));
                BindDetailPage(Settings.DetailTabId, Settings.TabId, Settings.GetModuleId(ModuleContext.ModuleId));
            }
            if (rblDataSource.SelectedIndex == 1 || rblDataSource.SelectedIndex == 2) // other module or other portal
            {
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                Renderinfo.IsOtherModule = (dsModule.TabID > 0 && dsModule.ModuleID > 0);
            }
            BindButtons(Settings, Renderinfo);
            if (rblUseTemplate.SelectedIndex == 0) // existing template
            {
                Renderinfo.Template = new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest();
                if (rblDataSource.SelectedIndex == 0) // this module
                {
                    //todo RenderDemoData();
                    Renderinfo.ShowDemoData = true;
                }
                else // other module
                {
                    //todo RenderOtherModuleDemoData();
                    Renderinfo.ShowDemoData = true;
                }
            }
            else // new template
            {
                if (!string.IsNullOrEmpty(ddlTemplate.SelectedValue))
                {
                    //Renderinfo.Template = new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest();
                    if (rblFrom.SelectedIndex == 0) // site
                    {
                        //todo RenderDemoData();
                        //Renderinfo.ShowDemoData = true;
                    }
                }
            }

        }
        private void BindOtherPortals(int portalId)
        {
            phPortals.Visible = rblDataSource.SelectedIndex == 2; // other portal
            IEnumerable<PortalInfo> portals = (new PortalController()).GetPortals().Cast<PortalInfo>();

            ddlPortals.Items.Clear();

            var listItems = new List<ListItem>();
            foreach (var item in portals)
            {

                var li = new ListItem(item.PortalName, item.PortalID.ToString());

                listItems.Add(li);
                if (item.PortalID == portalId)
                {
                    li.Selected = true;
                }

            }
            foreach (ListItem li in listItems.OrderBy(x => x.Text))
            {
                ddlPortals.Items.Add(li);
            }
            if (portalId < 0)
            {
                ddlPortals.SelectedValue = ModuleContext.PortalId.ToString();
            }
            //ddlPortals.Items[1].Enabled = ddlDataSource.Items.Count > 0;
        }
        private void BindOtherModules(int tabId, int moduleId)
        {
            int SelectedPortalId = ModuleContext.PortalId;
            if (rblDataSource.SelectedIndex == 2) // other portal
            {
                SelectedPortalId = int.Parse(ddlPortals.SelectedValue);
            }
            IEnumerable<ModuleInfo> modules = (new ModuleController()).GetModules(SelectedPortalId).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == App.Config.Opencontent && m.IsDeleted == false && !m.OpenContentSettings().IsOtherModule);
            rblDataSource.Items[1].Enabled = modules.Any();
            phPortals.Visible = rblDataSource.SelectedIndex == 2; // other portal
            phDataSource.Visible = rblDataSource.SelectedIndex == 1 || rblDataSource.SelectedIndex == 2; // other module
            if (rblDataSource.SelectedIndex == 1 || rblDataSource.SelectedIndex == 2) // other module
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
                    var tab = tc.GetTab(item.TabID, SelectedPortalId, false);
                    if (!tab.IsNeutralCulture && tab.CultureCode != DnnLanguageUtils.GetCurrentCultureCode())
                    {
                        // skip other cultures
                        continue;
                    }

                    var tabpath = tab.TabPath.Replace("//", "/").Trim('/');
                    var li = new ListItem(string.Format("{1} - {0}", item.ModuleTitle, tabpath), item.TabModuleID.ToString());

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
            rblDataSource.Items[1].Enabled = ddlDataSource.Items.Count > 0;
        }
        private void BindDetailPage(int currentDetailTabId, int othermoduleTabId, int dataModuleId)
        {
            string format;
            ListItem li;

            ActivateDetailPage();
            ddlDetailPage.Items.Clear();

            int othermoduleDetailTabId = GetOtherModuleDetailTabId(othermoduleTabId, dataModuleId);
            if (othermoduleDetailTabId > 0 && rblDataSource.SelectedIndex != 2)
            {
                //add extra li with "Default Detail Page" directly to dropdown
                format = LogContext.IsLogActive ? "Main Module Detail Page - [{0}]" : "Main Module Detail Page";
                li = new ListItem(string.Format(format, othermoduleDetailTabId), othermoduleDetailTabId.ToString());
                ddlDetailPage.Items.Add(li);
            }

            var listItems = new List<ListItem>();
            Dictionary<string, int> tabs = TabController.GetTabPathDictionary(ModuleContext.PortalId, DnnLanguageUtils.GetCurrentCultureCode());

            foreach (var tabId in tabs.Where(i => IsTabWithModuleWithSameMainModule(i.Value, dataModuleId) && IsAccessibleTab(i.Value)))
            {
                string tabname = tabId.Key.Replace("//", " / ").TrimStart(" / ");

                if ((othermoduleTabId > 0 && tabId.Value == othermoduleTabId) || (othermoduleTabId == -1 && tabId.Value == ModuleContext.TabId))
                {
                    //add extra li with "Main Module Page" directly to dropdown
                    format = LogContext.IsLogActive ? "Main Module Page - {0} [{1}]" : "Main Module Page";
                    li = new ListItem(string.Format(format, tabname, tabId.Value), "-1");
                    ddlDetailPage.Items.Add(li);
                }
                if (othermoduleTabId > 0 && tabId.Value == ModuleContext.TabId)
                {
                    //add extra li with "CurrentPage" directly to dropdown
                    format = LogContext.IsLogActive ? "Current Page - {0} [{1}]" : "Current Page";
                    li = new ListItem(string.Format(format, tabname, tabId.Value), tabId.Value.ToString());
                    ddlDetailPage.Items.Add(li);
                }

                format = LogContext.IsLogActive ? "{0} [{1}]" : "{0}";
                li = new ListItem(string.Format(format, tabname, tabId.Value), tabId.Value.ToString());

                listItems.Add(li);
                if (tabId.Value == currentDetailTabId)
                {
                    li.Selected = true;
                }

            }
            foreach (ListItem listItem in listItems.OrderBy(x => x.Text))
            {
                ddlDetailPage.Items.Add(listItem);
            }
        }

        private int GetOtherModuleDetailTabId(int othermoduleTabId, int dataModuleId)
        {
            //If tab<0 then the data does not come from an other module
            if (othermoduleTabId < 0) return 0;

            var moduleInfo = DnnUtils.GetDnnModule(othermoduleTabId, dataModuleId);
            if (moduleInfo == null)
            {
                //This should never happen
                App.Services.Logger.Error($"Module {dataModuleId} not found while in GetOtherModuleDetailTabId()");
                return 0;
            }

            var mainModuleSettings = moduleInfo.OpenContentSettings();

            if (mainModuleSettings == null) return 0;
            if (mainModuleSettings.TabId > -1) return 0; //the other module gets his data also from another module?! Let's not support that.

            return mainModuleSettings.DetailTabId == -1 ? moduleInfo.TabID : mainModuleSettings.DetailTabId;
        }

        private bool IsAccessibleTab(int tabId)
        {
            //ignore redirected tabs
            var tabinfo = TabController.Instance.GetTab(tabId, ModuleContext.PortalId);
            return tabinfo.IsPublishedTab();
        }

        private bool IsTabWithModuleWithSameMainModule(int tabId, int datamoduleId)
        {
            //only tabs with oc-module with main-moduleId= CurrentMainModuleId
            var tabinfo = TabController.Instance.GetTab(tabId, ModuleContext.PortalId);
            foreach (var item in tabinfo.ChildModules)
            {
                ModuleInfo moduleInfo = item.Value;
                if (moduleInfo.ModuleDefinition.FriendlyName == App.Config.Opencontent)
                {
                    try
                    {
                        if (moduleInfo.OpenContentSettings().GetModuleId(moduleInfo.ModuleID) == datamoduleId)
                        {
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return false;
        }

        private void ActivateDetailPage()
        {
            phDetailPage.Visible = false;
            if (ddlTemplate.SelectedIndex >= 0 && rblUseTemplate.SelectedIndex == 0) // existing template
            {
                var template = new FileUri(ddlTemplate.SelectedValue);
                var manifest = template.ToTemplateManifest();
                if (manifest.IsListTemplate && manifest.Manifest.Templates.Any(t => t.Value.Detail != null))
                {
                    phDetailPage.Visible = true;
                }
            }
        }

        protected void ddlPotals_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindOtherModules(-1, -1);
            var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
            var dsSettings = dsModule.OpenContentSettings();
            BindTemplates(dsSettings.Template, dsSettings.Template.MainTemplateUri());
        }
    }
}