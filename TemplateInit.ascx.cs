using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Rss;

namespace Satrabel.OpenContent
{
    public partial class TemplateInit : System.Web.UI.UserControl
    {
        public ModuleInstanceContext ModuleContext { get; set; }
        public OpenContentSettings Settings { get; set; }
        public RenderInfo Renderinfo { get; set; }
        public bool RenderOnlySaveButton { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            pHelp.Visible = false;
        }
        protected void rblFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTemplate.Items.Clear();
            if (rblFrom.SelectedIndex == 0) // site
            {
                var scriptFileSetting = ModuleContext.OpenContentSettings().Template;
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
            ActivateDetailPage();
        }

        protected void rblDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblDataSource.SelectedIndex == 1) // other module
            {
                BindOtherModules(-1, -1);
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                var dsSettings = dsModule.OpenContentSettings();
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
            var scriptFileSetting = ModuleContext.OpenContentSettings().Template;
            ddlTemplate.Items.Clear();
            if (rblUseTemplate.SelectedIndex == 0) // existing
            {
                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplatesFiles(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());
            }
            else if (rblUseTemplate.SelectedIndex == 1) // new
            {

                ddlTemplate.Items.AddRange(OpenContentUtils.GetTemplates(ModuleContext.PortalSettings, ModuleContext.ModuleId, scriptFileSetting, "OpenContent").ToArray());
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
                mc.UpdateModuleSetting(ModuleContext.ModuleId, "detailtabid", ddlDetailPage.SelectedValue);
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
            ActivateDetailPage();
        }

        protected void ddlDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
            var dsSettings = dsModule.OpenContentSettings();
            BindTemplates(dsSettings.Template, dsSettings.Template.Uri());
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
                rblDataSource.SelectedIndex = (Settings.TabId > 0 && Settings.ModuleId > 0 ? 1 : 0);
                BindOtherModules(Settings.TabId, Settings.ModuleId);
                BindTemplates(Settings.Template, (Renderinfo.IsOtherModule ? Renderinfo.Template.Uri() : null));
                BindDetailPage(Settings.DetailTabId);
            }
            if (rblDataSource.SelectedIndex == 1) // other module
            {
                var dsModule = (new ModuleController()).GetTabModule(int.Parse(ddlDataSource.SelectedValue));
                var dsSettings = dsModule.OpenContentSettings();
                Renderinfo.SetDataSourceModule(dsModule.TabID, dsModule.ModuleID, dsModule, dsSettings.Template, dsSettings.Data);
            }
            BindButtons(Settings, Renderinfo);
            if (rblUseTemplate.SelectedIndex == 0 ) // existing template
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

            //todo: next line should exclude the modules of other languages
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == "OpenContent" && m.IsDeleted == false && !m.OpenContentSettings().IsOtherModule);

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
                    var tabpath = tab.TabPath.Replace("//", "/").TrimEnd(tab.TabName).Trim('/');
                    var li = new ListItem(string.Format("[{2}]{0} - {1}", tab.TabName, item.ModuleTitle, tabpath), item.TabModuleID.ToString());
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
        private void BindDetailPage(int tabId)
        {

            ActivateDetailPage();

            ddlDetailPage.Items.Clear();
            ddlDetailPage.Items.Add(new ListItem("Main Module Page", "-1"));
            var listItems = new List<ListItem>();
            var tc = new TabController();
            var tabs = TabController.GetTabPathDictionary(ModuleContext.PortalId, DnnUtils.GetCurrentCultureCode());

            //var tabs = tc.GetTabsByPortal(ModuleContext.PortalId);
            foreach (var tab in tabs)
            {

                //var tabpath = tab.TabPath.Replace("//", "/").TrimEnd(tab.TabName).Trim('/');
                var li = new ListItem(tab.Key.Replace("//", " / ").TrimStart(" / "), tab.Value.ToString());
                if (!tab.Key.StartsWith("//Admin//"))
                {
                    listItems.Add(li);
                    if (tab.Value == tabId)
                    {
                        li.Selected = true;
                    }
                }
            }
            foreach (ListItem li in listItems.OrderBy(x => x.Text))
            {
                ddlDetailPage.Items.Add(li);
            }
        }

        private void ActivateDetailPage()
        {
            phDetailPage.Visible = false;
            if (ddlTemplate.SelectedIndex >= 0 && rblUseTemplate.SelectedIndex == 0 ) // existing template
            {
                var template = new FileUri(ddlTemplate.SelectedValue);
                var manifest = template.ToTemplateManifest();
                if (manifest.IsListTemplate)
                {
                    phDetailPage.Visible = true;
                }
            }
        }

    }
}