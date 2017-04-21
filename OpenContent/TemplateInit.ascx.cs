using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Rss;
using Satrabel.OpenContent.Components.Logging;

namespace Satrabel.OpenContent
{
    public partial class TemplateInit : System.Web.UI.UserControl
    {
        public bool PageRefresh { get; set; }
        public ModuleInstanceContext ModuleContext { get; set; }
        public OpenContentSettings Settings { get; set; }
        public RenderInfo Renderinfo { get; set; }
        public bool RenderOnlySaveButton { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            pHelp.Visible = false;
            phCurrentTemplate.Visible = false;
        }
        protected void rblFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTemplate.Items.Clear();
            if (rblFrom.SelectedIndex == 0) // site
            {
                var scriptFileSetting = ModuleContext.OpenContentSettings().Template;
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, AppConfig.OPENCONTENT).ToArray());
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
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, AppConfig.OPENCONTENT).ToArray());
            }
            else if (rblUseTemplate.SelectedIndex == 1) // new
            {

                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, AppConfig.OPENCONTENT).ToArray());
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
                        string fileName = ddlTemplate.SelectedValue;
                        string template = OpenContentUtils.ImportFromWeb(ModuleContext.PortalId, fileName, tbTemplateName.Text);
                        mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                        ModuleContext.Settings["template"] = template;
                    }
                }
                mc.UpdateModuleSetting(ModuleContext.ModuleId, "detailtabid", ddlDetailPage.SelectedValue);

                
                //don't reset settings. Sure they might be invalid, but maybe not. And you can't ever revert.
                //mc.DeleteModuleSetting(ModuleContext.ModuleId, "data");
                if (PageRefresh)
                {
                    Response.Redirect(Globals.NavigateURL(), true);
                }
                else
                {
                    Settings = ModuleContext.OpenContentSettings();
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
            ddlTemplate.Items.Clear();

            //var templateUri = template == null ? null : template.Uri;
            //var otherModuleTemplateUri = otherModuleTemplate == null ? null : otherModuleTemplate.Uri;

            ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, template, AppConfig.OPENCONTENT, otherModuleTemplate).ToArray());
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
            }
        }
        public void RenderInitForm()
        {
            Renderinfo.ShowDemoData = false;
            pHelp.Visible = true;
            if (!Page.IsPostBack || ddlTemplate.Items.Count == 0)
            {
                rblDataSource.SelectedIndex = (Settings.IsOtherModule ? 1 : 0);
                BindOtherModules(Settings.TabId, Settings.ModuleId);
                BindTemplates(Settings.Template, (Renderinfo.IsOtherModule ? Renderinfo.Template.MainTemplateUri() : null));
                BindDetailPage(Settings.DetailTabId, Settings.TabId, Settings.GetModuleId(ModuleContext.ModuleId));
            }
            if (rblDataSource.SelectedIndex == 1) // other module
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
                    Renderinfo.Template = new FileUri(ddlTemplate.SelectedValue).ToTemplateManifest();
                    if (rblFrom.SelectedIndex == 0) // site
                    {
                        //todo RenderDemoData();
                        //Renderinfo.ShowDemoData = true;
                    }
                }
            }
        }

        private void BindOtherModules(int tabId, int moduleId)
        {
            IEnumerable<ModuleInfo> modules = (new ModuleController()).GetModules(ModuleContext.PortalId).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == AppConfig.OPENCONTENT && m.IsDeleted == false && !m.OpenContentSettings().IsOtherModule);
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
        }
        private void BindDetailPage(int currentDetailTabId, int othermoduleTabId, int dataModuleId)
        {
            string format;
            ListItem li;

            ActivateDetailPage();
            ddlDetailPage.Items.Clear();

            int othermoduleDetailTabId = GetOtherModuleDetailTabId(othermoduleTabId, dataModuleId);
            if (othermoduleDetailTabId > 0)
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

            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(dataModuleId, othermoduleTabId, false);
            if (moduleInfo == null)
            {
                //This should never happen
                Log.Logger.Error($"Module {dataModuleId} not found while in GetOtherModuleDetailTabId()");
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
                if (moduleInfo.ModuleDefinition.FriendlyName == AppConfig.OPENCONTENT)
                {
                    if (moduleInfo.OpenContentSettings().GetModuleId(moduleInfo.ModuleID) == datamoduleId)
                    {
                        return true;
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

    }
}