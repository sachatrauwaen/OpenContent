#region Copyright

// 
// Copyright (c) 2015-2016
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using System.Web;

#endregion

namespace Satrabel.OpenContent
{
    public partial class Edit : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var bootstrap = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadBootstrap();
            bool loadGlyphicons = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadGlyphicons();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            cmdCopy.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            if (ManifestUtils.CheckSocialGroupFilter(settings.Manifest))
            {
                hlCancel.NavigateUrl = SocialGroupUtils.CreateSocialGroupReturnUrl(settings.Manifest, hlCancel.NavigateUrl);
                cmdCopy.NavigateUrl = SocialGroupUtils.CreateSocialGroupReturnUrl(settings.Manifest, cmdCopy.NavigateUrl);
                cmdSave.NavigateUrl = SocialGroupUtils.CreateSocialGroupReturnUrl(settings.Manifest, cmdSave.NavigateUrl);
            }
            string prefix = (string.IsNullOrEmpty(settings.Template.Collection) || settings.Template.Collection == "Items") ? "" : settings.Template.Collection;

            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, prefix);
            alpaca.RegisterAll(bootstrap, loadBootstrap, loadGlyphicons);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, cmdCopy.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.IsNew = settings.Template.IsListTemplate && string.IsNullOrEmpty(itemId);
            AlpacaContext.GroupId = Request.QueryString["groupid"];
        }
        public AlpacaContext AlpacaContext { get; private set; }
    }
}

