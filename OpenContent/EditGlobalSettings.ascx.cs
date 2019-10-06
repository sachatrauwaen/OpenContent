#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using Satrabel.OpenContent.Components;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.UrlRewriter;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditGlobalSettings : PortalModuleBase
    {
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.Click += cmdSave_Click;
            cmdUpgradeXml.Click += cmdUpgradeXml_Click;
            cmdPurgeUrlCache.Click += cmdPurgeUrlCache_Click;
            //cmdCancel.Click += cmdCancel_Click;
        }

        private void cmdPurgeUrlCache_Click(object sender, EventArgs e)
        {
            UrlRulesCaching.PurgeCache(PortalId);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                var globalSettingsRepository= App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId);
                ddlRoles.Items.Add(new ListItem("None", "-1"));
                var rc = new RoleController();
                foreach (var role in rc.GetRoles(PortalId))
                {
                    ddlRoles.Items.Add(new ListItem(role.RoleName, role.RoleID.ToString()));
                }

                if (!string.IsNullOrEmpty(globalSettingsRepository.GetEditorRoleId()))
                {
                    var li = ddlRoles.Items.FindByValue(globalSettingsRepository.GetEditorRoleId());
                    if (li != null)
                    {
                        li.Selected = true;
                    }
                }
                cbMLContent.Checked = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetAutoAttach();

                foreach (var item in new[] { 5, 10, 25, 50, 100 })
                {
                    ddlMaxVersions.Items.Add(new ListItem(item.ToString(), item.ToString()));
                }
                var maxVersionItem = ddlMaxVersions.Items.FindByValue(globalSettingsRepository.GetMaxVersions().ToString());
                if (maxVersionItem != null) maxVersionItem.Selected = true;

                ddlLogging.SelectedValue = globalSettingsRepository.GetLoggingScope();

                var editLayoutItem = ddlEditLayout.Items.FindByValue(((int)globalSettingsRepository.GetEditLayout()).ToString());
                if (editLayoutItem != null) editLayoutItem.Selected = true;

                cbLoadBootstrap.Checked = globalSettingsRepository.GetLoadBootstrap();
                cbLoadBootstrap.Visible = lLoadBootstrap.Visible = globalSettingsRepository.GetEditLayout() != AlpacaLayoutEnum.DNN;
                cbLoadGlyphicons.Checked = globalSettingsRepository.GetLoadGlyphicons();
                cbLoadGlyphicons.Visible = cbLoadBootstrap.Visible;
                tbGoogleApiKey.Text = globalSettingsRepository.GetGoogleApiKey();
                cbLegacyHandlebars.Checked = globalSettingsRepository.GetLegacyHandlebars();
                //cbCompositeCss.Checked = globalSettingsRepository.GetCompositeCss();
                cbSaveXml.Checked = globalSettingsRepository.IsSaveXml();
                tbGithubRepository.Text = globalSettingsRepository.GetGithubRepository();
                cmdUpgradeXml.Visible = cbSaveXml.Checked;
            }
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            var globalSettingsRepository = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId);
            globalSettingsRepository.SetEditorRoleId(ddlRoles.SelectedIndex > 0 ? ddlRoles.SelectedValue : "");
            globalSettingsRepository.SetAutoAttach(cbMLContent.Checked.ToString());
            int maxVersions;
            if (int.TryParse(ddlMaxVersions.SelectedValue, out maxVersions))
            {
                globalSettingsRepository.SetMaxVersions(maxVersions);

            }
            globalSettingsRepository.SetLoggingScope(ddlLogging.SelectedValue);
            int editLayout;
            if (int.TryParse(ddlEditLayout.SelectedValue, out editLayout))
            {
                globalSettingsRepository.SetEditLayout((AlpacaLayoutEnum)editLayout);
            }
            globalSettingsRepository.SetLoadBootstrap(cbLoadBootstrap.Checked);
            globalSettingsRepository.SetLoadGlyphicons(cbLoadGlyphicons.Checked);
            globalSettingsRepository.SetGoogleApiKey(tbGoogleApiKey.Text);
            globalSettingsRepository.SetLegacyHandlebars(cbLegacyHandlebars.Checked);
            //globalSettingsRepository.SetCompositeCss(cbCompositeCss.Checked);
            globalSettingsRepository.SetSaveXml(cbSaveXml.Checked);
            globalSettingsRepository.SetGithubRepository(tbGithubRepository.Text);

            Response.Redirect(Globals.NavigateURL(), true);
        }

        protected void cmdUpgradeXml_Click(object sender, EventArgs e)
        {
            var globalSettingsController = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId);
            if (globalSettingsController.IsSaveXml())
            {
                App.Services.Logger.Info("Updating all OpenContent Xml data for portal " + ModuleContext.PortalId);
                try
                {
                    var ctrl = new OpenContentController(ModuleContext.PortalId);
                    var modules = DnnUtils.GetDnnOpenContentModules(ModuleContext.PortalId);
                    foreach (var module in modules)
                    {
                        Log.Logger.Info("Updating all OpenContent Xml data for module " + module.ModuleId);
                        var contents = ctrl.GetContents(module.ModuleId);
                        foreach (var item in contents)
                        {
                            ctrl.UpdateXmlContent(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.Services.Logger.Error("Error while Updating all OpenContent Xml data for portal " + ModuleContext.PortalId, ex);
                }
                finally
                {
                }
                Log.Logger.Info("Finished Updating all OpenContent Xml data for portal " + ModuleContext.PortalId);
            }
            //Response.Redirect(Globals.NavigateURL(), true);
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
        }
        #endregion

        protected void ddlEditLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbLoadBootstrap.Visible = lLoadBootstrap.Visible = ddlEditLayout.SelectedValue != "1"; // DNN
            cbLoadGlyphicons.Visible = cbLoadBootstrap.Visible;
        }


    }
}