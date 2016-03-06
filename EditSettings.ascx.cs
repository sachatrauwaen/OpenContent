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
using System.Web.Hosting;
using System.Web;
using Satrabel.OpenContent.Components.Alpaca;
using System.IO;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{

    public partial class EditSettings : PortalModuleBase
    {

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();

            var settings = ModuleContext.OpenContentSettings();
            if (settings.TemplateAvailable)
            {
                AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, settings.TemplateDir.FolderPath, settings.TemplateKey.ShortKey );
                alpaca.RegisterAll();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                                
                //var settings = ModuleContext.OpenContentSettings();
                //scriptList.Items.AddRange(OpenContentUtils.GetTemplatesFiles(PortalSettings, ModuleId, settings.Template, "OpenContent", (settings.IsOtherModule ? settings.Template.Uri() : null)).ToArray());
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            /*
            ModuleController mc = new ModuleController();
            mc.UpdateModuleSetting(ModuleId, "template", OpenContentUtils.SetTemplateFolder(scriptList.SelectedValue));
            mc.UpdateModuleSetting(ModuleId, "data", HiddenField.Value);
            Response.Redirect(Globals.NavigateURL(), true);
             */
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
        public string AlpacaCulture
        {
            get
            {
                string cultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
                return AlpacaEngine.AlpacaCulture(cultureCode);
            }
        }
    }
}

