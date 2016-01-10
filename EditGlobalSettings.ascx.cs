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
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Framework;
using Satrabel.OpenContent.Components;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using System.Web.UI.WebControls;
using DotNetNuke.Security.Roles;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Index;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Lucene.Config;

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
                var pc = new PortalController();
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
            }
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            if (ddlRoles.SelectedIndex > 0)
                PortalController.UpdatePortalSetting(PortalId, "OpenContent_EditorsRoleId", ddlRoles.SelectedValue, true);
            else
                PortalController.DeletePortalSetting(PortalId, "OpenContent_EditorsRoleId");

            PortalController.UpdatePortalSetting(PortalId, "OpenContent_AutoAttach", cbMLContent.Checked.ToString(), true);

            Response.Redirect(Globals.NavigateURL(), true);
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
        }
        #endregion

       
    }
}