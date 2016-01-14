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
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{
    public partial class AddEdit : PortalModuleBase
    {
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var settings = ModuleContext.OpenContentSettings();
            Manifest manifest = settings.Manifest;

            if (settings.TemplateKey.Extention != ".manifest")
            {
                manifest = ManifestUtils.GetFileManifest(settings.TemplateKey.TemplateDir);
            }

            if (manifest != null)
            {
                string addEditControl = manifest.AdditionalEditControl;
                if (!string.IsNullOrEmpty(addEditControl))
                {
                    var contr = LoadControl(addEditControl);
                    PortalModuleBase mod = contr as PortalModuleBase;
                    if (mod != null)
                    {
                        mod.ModuleConfiguration = this.ModuleConfiguration;
                        mod.ModuleId = this.ModuleId;
                        mod.LocalResourceFile = this.LocalResourceFile;
                    }
                    this.Controls.Add(contr);
                }
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
            }
        }
        #endregion
        public string CurrentCulture
        {
            get
            {
                return LocaleController.Instance.GetCurrentLocale(PortalId).Code;
            }
        }
    }
}

