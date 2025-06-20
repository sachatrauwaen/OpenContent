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
using Satrabel.OpenContent.Components.Render;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditFormSettings : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var pageContext = new WebFormsPageContext(Page, this);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            bool bootstrap = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadBootstrap();
            bool loadGlyphicons = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadGlyphicons();
            bool builderV2 = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).IsBuilderV2();
            string apikey = App.Services.CreateGlobalSettingsRepository(PortalId).GetGoogleApiKey();
            OpenContentSettings settings = this.OpenContentSettings();
            if (settings.Manifest.BuilderVersion > 0)
            {
                builderV2 = settings.Manifest.BuilderVersion == 2;
            }
            AlpacaEngine alpaca = new AlpacaEngine(pageContext, ModuleContext.PortalId, "DeskTopModules/OpenContent", "formsettings");
            //AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, "", "");
            alpaca.RegisterAll(bootstrap, loadBootstrap, loadGlyphicons, builderV2);
            string itemId = null;
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null, null);
            AlpacaContext.Bootstrap = true;
            AlpacaContext.BuilderV2 = builderV2;
            AlpacaContext.GoogleApiKey = apikey;
        }

        public AlpacaContext AlpacaContext { get; private set; }
    }
}

