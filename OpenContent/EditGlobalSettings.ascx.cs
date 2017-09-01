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
            //cmdCancel.Click += cmdCancel_Click;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                ddlRoles.Items.Add(new ListItem("None", "-1"));
                var rc = new RoleController();
                foreach (var role in rc.GetRoles(PortalId))
                {
                    ddlRoles.Items.Add(new ListItem(role.RoleName, role.RoleID.ToString()));
                }

                if (!string.IsNullOrEmpty(App.Services.CreateGlobalSettingsRepository().GetEditorRoleId()))
                {
                    var li = ddlRoles.Items.FindByValue(App.Services.CreateGlobalSettingsRepository().GetEditorRoleId());
                    if (li != null)
                    {
                        li.Selected = true;
                    }
                }
                cbMLContent.Checked = App.Services.CreateGlobalSettingsRepository().GetAutoAttach();

                foreach (var item in new[] { 5, 10, 25, 50, 100 })
                {
                    ddlMaxVersions.Items.Add(new ListItem(item.ToString(), item.ToString()));
                }
                var maxVersionItem = ddlMaxVersions.Items.FindByValue(App.Services.CreateGlobalSettingsRepository().GetMaxVersions().ToString());
                if (maxVersionItem != null) maxVersionItem.Selected = true;

                ddlLogging.SelectedValue = App.Services.CreateGlobalSettingsRepository().GetLoggingScope();

                var editLayoutItem = ddlEditLayout.Items.FindByValue(((int)App.Services.CreateGlobalSettingsRepository().GetEditLayout()).ToString());
                if (editLayoutItem != null) editLayoutItem.Selected = true;

                cbLoadBootstrap.Checked = App.Services.CreateGlobalSettingsRepository().GetLoadBootstrap();
                cbLoadBootstrap.Visible = lLoadBootstrap.Visible = App.Services.CreateGlobalSettingsRepository().GetEditLayout() != AlpacaLayoutEnum.DNN;
                tbGoogleApiKey.Text = App.Services.CreateGlobalSettingsRepository().GetGoogleApiKey();
                cbFastHandlebars.Checked = App.Services.CreateGlobalSettingsRepository().GetFastHandlebars();
            }
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            App.Services.CreateGlobalSettingsRepository().SetEditorRoleId(ddlRoles.SelectedIndex > 0 ? ddlRoles.SelectedValue : "");
            App.Services.CreateGlobalSettingsRepository().SetAutoAttach(cbMLContent.Checked.ToString());
            int maxVersions;
            if (int.TryParse(ddlMaxVersions.SelectedValue, out maxVersions))
            {
                App.Services.CreateGlobalSettingsRepository().SetMaxVersions(maxVersions);

            }
            App.Services.CreateGlobalSettingsRepository().SetLoggingScope(ddlLogging.SelectedValue);
            int editLayout;
            if (int.TryParse(ddlEditLayout.SelectedValue, out editLayout))
            {
                App.Services.CreateGlobalSettingsRepository().SetEditLayout((AlpacaLayoutEnum)editLayout);
            }
            App.Services.CreateGlobalSettingsRepository().SetLoadBootstrap(cbLoadBootstrap.Checked);
            App.Services.CreateGlobalSettingsRepository().SetGoogleApiKey(tbGoogleApiKey.Text);
            App.Services.CreateGlobalSettingsRepository().SetFastHandlebars(cbFastHandlebars.Checked);


            Response.Redirect(Globals.NavigateURL(), true);
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