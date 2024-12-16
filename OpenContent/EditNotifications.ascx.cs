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
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Render;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditNotifications : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var pageContext = new WebFormsPageContext(Page, this);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            var bootstrap = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadBootstrap();
            bool loadGlyphicons = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadGlyphicons();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(pageContext, ModuleContext.PortalId, "DeskTopModules/OpenContent", "notifications");
            //AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, "", "");
            alpaca.RegisterAll(bootstrap, loadBootstrap, loadGlyphicons, false);
            string itemId = null;
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null, null);
            AlpacaContext.Bootstrap = true;
            AlpacaContext.Horizontal = true;
        }

        public AlpacaContext AlpacaContext { get; private set; }
    }
}

