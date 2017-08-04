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
            var globalSettingsController = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController(ModuleContext.PortalId);
            var bootstrap = globalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && globalSettingsController.GetLoadBootstrap();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            cmdCopy.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();

            string prefix = (string.IsNullOrEmpty(settings.Template.Collection) || settings.Template.Collection == "Items") ? "" : settings.Template.Collection;

            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, prefix);
            alpaca.RegisterAll(bootstrap, loadBootstrap);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, cmdCopy.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = globalSettingsController.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.IsNew = settings.Template.IsListTemplate && string.IsNullOrEmpty(itemId);
        }
        public AlpacaContext AlpacaContext { get; private set; }
    }
}

