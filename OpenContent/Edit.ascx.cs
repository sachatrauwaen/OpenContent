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
            bool builderV2 = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).IsBuilderV2();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            cmdCopy.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();

            string prefix = (string.IsNullOrEmpty(settings.Template.Collection) || settings.Template.Collection == "Items") ? "" : settings.Template.Collection;

            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, prefix);
            alpaca.RegisterAll(bootstrap, loadBootstrap, loadGlyphicons, builderV2);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId,  ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, cmdCopy.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.IsNew = settings.Template.IsListTemplate && string.IsNullOrEmpty(itemId);
            AlpacaContext.DeleteConfirmMessage = LocalizeSafeJsString("txtDeleteConfirmMessage");
            if (DnnLanguageUtils.IsMultiLingualPortal(PortalId))
            {
                AlpacaContext.DeleteConfirmMessage = LocalizeSafeJsString("txtMLDeleteConfirmMessage");
            }
            AlpacaContext.BuilderV2 = builderV2;
        }
        public AlpacaContext AlpacaContext { get; private set; }
    }
}

