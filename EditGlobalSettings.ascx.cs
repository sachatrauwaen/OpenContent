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
                var hc = HostController.Instance;
                cbEditWitoutPostback.Checked = hc.GetBoolean("EditWitoutPostback", false);
                tbAddEditControl.Text = PortalController.GetPortalSetting("OpenContent_AddEditControl", ModuleContext.PortalId, "");
			}
		}
		protected void cmdSave_Click(object sender, EventArgs e)
		{
            var hc = HostController.Instance;
            hc.Update("EditWitoutPostback", cbEditWitoutPostback.Checked.ToString(),true);
            PortalController.UpdatePortalSetting(PortalId,"OpenContent_AddEditControl", tbAddEditControl.Text, true);
            Response.Redirect(Globals.NavigateURL(), true);
		}
		protected void cmdCancel_Click(object sender, EventArgs e)
		{
		}
		#endregion
	}
}