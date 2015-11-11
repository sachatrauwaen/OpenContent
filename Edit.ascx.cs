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
using DotNetNuke.Services.Localization;
using System.IO;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;
using System.Web.Hosting;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using System.Web;

#endregion

namespace Satrabel.OpenContent
{

    public partial class Edit : PortalModuleBase
    {
        public bool ListMode
        {
            get
            {
                return true;
            }
        }

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();

            var template = OpenContentUtils.GetTemplate(Settings);
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, template.PhysicalRelativeDirectory, "");
            alpaca.RegisterAll();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {

            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            //ModuleController.Instance.UpdateModuleSetting(ModuleId, "field", txtField.Text);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Update Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }


        protected void cmdCancel_Click(object sender, EventArgs e)
        {
        }

        #endregion

        public string CurrentCulture
        {
            get
            {
                return LocaleController.Instance.GetCurrentLocale(PortalId).Code;
            }
        }
        public string DefaultCulture
        {
            get
            {
                return LocaleController.Instance.GetDefaultLocale(PortalId).Code;
            }
        }
        public string NumberDecimalSeparator
        {
            get
            {
                return LocaleController.Instance.GetCurrentLocale(PortalId).Culture.NumberFormat.NumberDecimalSeparator;
            }
        }
    }
}

