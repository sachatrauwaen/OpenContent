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
using DotNetNuke.Entities.Portals;
using System.Web.UI.WebControls;
using DotNetNuke.Security.Roles;
using Satrabel.OpenContent.Components.Alpaca;

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
            //cmdCancel.Click += cmdCancel_Click;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                var globalSettingsController = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController(ModuleContext.PortalId);
                ddlRoles.Items.Add(new ListItem("None", "-1"));
                var rc = new RoleController();
                foreach (var role in rc.GetRoles(PortalId))
                {
                    ddlRoles.Items.Add(new ListItem(role.RoleName, role.RoleID.ToString()));
                }

                string OpenContent_EditorsRoleId = PortalController.GetPortalSetting("OpenContent_EditorsRoleId", ModuleContext.PortalId, "");
                if (!string.IsNullOrEmpty(OpenContent_EditorsRoleId))
                {
                    var li = ddlRoles.Items.FindByValue(OpenContent_EditorsRoleId);
                    if (li != null)
                    {
                        li.Selected = true;
                    }
                }
                string OpenContent_AutoAttach = PortalController.GetPortalSetting("OpenContent_AutoAttach", ModuleContext.PortalId, "False");
                cbMLContent.Checked = bool.Parse(OpenContent_AutoAttach);

                foreach (var item in new[] { 5, 10, 25, 50, 100 })
                {
                    ddlMaxVersions.Items.Add(new ListItem(item.ToString(), item.ToString()));
                }
                var maxVersionItem = ddlMaxVersions.Items.FindByValue(globalSettingsController.GetMaxVersions().ToString());
                if (maxVersionItem != null) maxVersionItem.Selected = true;

                string OpenContent_Logging = PortalController.GetPortalSetting("OpenContent_Logging", ModuleContext.PortalId, "none");
                ddlLogging.SelectedValue = OpenContent_Logging;

                var editLayoutItem = ddlEditLayout.Items.FindByValue(((int)globalSettingsController.GetEditLayout()).ToString());
                if (editLayoutItem != null) editLayoutItem.Selected = true;

                cbLoadBootstrap.Checked = globalSettingsController.GetLoadBootstrap();
                cbLoadBootstrap.Visible = lLoadBootstrap.Visible = globalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
                tbGoogleApiKey.Text = globalSettingsController.GetGoogleApiKey();
                cbFastHandlebars.Checked = globalSettingsController.GetFastHandlebars();
                cbSaveXml.Checked = globalSettingsController.IsSaveXml();
                cmdUpgradeXml.Visible = cbSaveXml.Checked;
            }
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            var globalSettingsController = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController(ModuleContext.PortalId);
            if (ddlRoles.SelectedIndex > 0)
                PortalController.UpdatePortalSetting(ModuleContext.PortalId, "OpenContent_EditorsRoleId", ddlRoles.SelectedValue, true);
            else
                PortalController.DeletePortalSetting(ModuleContext.PortalId, "OpenContent_EditorsRoleId");

            PortalController.UpdatePortalSetting(ModuleContext.PortalId, "OpenContent_AutoAttach", cbMLContent.Checked.ToString(), true);
            int maxVersions;
            if (int.TryParse(ddlMaxVersions.SelectedValue, out maxVersions))
            {
                globalSettingsController.SetMaxVersions(maxVersions);
            }
            PortalController.UpdatePortalSetting(ModuleContext.PortalId, "OpenContent_Logging", ddlLogging.SelectedValue, true);
            int editLayout;
            if (int.TryParse(ddlEditLayout.SelectedValue, out editLayout))
            {
                globalSettingsController.SetEditLayout((AlpacaLayoutEnum)editLayout);
            }
            globalSettingsController.SetLoadBootstrap(cbLoadBootstrap.Checked);
            globalSettingsController.SetGoogleApiKey(tbGoogleApiKey.Text);
            globalSettingsController.SetFastHandlebars(cbFastHandlebars.Checked);
            globalSettingsController.SetSaveXml(cbSaveXml.Checked);

            Response.Redirect(Globals.NavigateURL(), true);
        }

        protected void cmdUpgradeXml_Click(object sender, EventArgs e)
        {
            var globalSettingsController = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController(ModuleContext.PortalId);
            if (globalSettingsController.IsSaveXml())
            {
                Log.Logger.Info("Updating all OpenContent Xml data for portal " + ModuleContext.PortalId);
                try
                {
                    var ctrl = new OpenContentController(ModuleContext.PortalId);
                    var modules = DnnUtils.GetDnnOpenContentModules(ModuleContext.PortalId);
                    foreach (var module in modules)
                    {
                        var contents = ctrl.GetContents(module.ModuleId);
                        foreach (var item in contents)
                        {
                            ctrl.UpdateXmlContent(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Error while Updating all OpenContent Xml data for portal " + ModuleContext.PortalId, ex);
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
        }


    }
}