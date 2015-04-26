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
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;
using System.IO;
using Satrabel.OpenContent.Components;
using Newtonsoft.Json.Linq;

#endregion

namespace Satrabel.OpenContent
{

    public partial class EditData : PortalModuleBase
    {
        public string ModuleTemplateDirectory
        {
            get
            {
                return PortalSettings.HomeDirectory + "OpenContent/Templates/" + ModuleId.ToString() + "/";
            }
        }
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cmdSave.Click += cmdSave_Click;
            cmdCancel.Click += cmdCancel_Click;
            //ServicesFramework.Instance.RequestAjaxScriptSupport();
            //ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                OpenContentController ctrl = new OpenContentController();
                OpenContentInfo data = ctrl.GetFirstContent(ModuleId);
                if (data != null)
                {
                    txtSource.Text = data.Json;
                }
            }
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            JObject json = JObject.Parse(txtSource.Text);
            OpenContentController ctrl = new OpenContentController();
            var data = ctrl.GetFirstContent(ModuleId);
            if (data == null)
            {
                data = new OpenContentInfo()
                {
                    ModuleId = ModuleId,
                    Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString(),
                    CreatedByUserId = UserInfo.UserID,
                    CreatedOnDate = DateTime.Now,
                    LastModifiedByUserId = UserInfo.UserID,
                    LastModifiedOnDate = DateTime.Now,
                    Html = "",
                    Json = txtSource.Text
                };
                ctrl.AddContent(data);
            }
            else
            {
                data.Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString();
                data.LastModifiedByUserId = UserInfo.UserID;
                data.LastModifiedOnDate = DateTime.Now;
                data.Json = txtSource.Text;
                ctrl.UpdateContent(data);
            }

            if (json["ModuleTitle"] != null && json["ModuleTitle"].Type == JTokenType.String)
            {
                string ModuleTitle = json["ModuleTitle"].ToString();
                OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
            }

            Response.Redirect(Globals.NavigateURL(), true);
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }

        #endregion

    }
}

