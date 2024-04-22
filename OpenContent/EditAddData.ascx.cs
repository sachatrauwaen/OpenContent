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
using Satrabel.OpenContent.Components.Alpaca;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditAddData : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var globalSettingsController = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId);
            var bootstrap = globalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && globalSettingsController.GetLoadBootstrap();
            bool loadGlyphicons = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadGlyphicons();
            bool builderV2 = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).IsBuilderV2();
            string apikey = App.Services.CreateGlobalSettingsRepository(PortalId).GetGoogleApiKey();
            Key = Request.QueryString["key"];
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, Key);
            alpaca.RegisterAll(bootstrap,loadBootstrap, loadGlyphicons, builderV2);
            string itemId = null;//Request.QueryString["id"] == null ? -1 : int.Parse(Request.QueryString["id"]);
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null, null);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = globalSettingsController.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.BuilderV2 = builderV2;
            AlpacaContext.GoogleApiKey = apikey;
        }

        public AlpacaContext AlpacaContext { get; private set; }

        public string Key { get; private set; }
    }
}

