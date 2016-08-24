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
            var editLayout = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout();
            var bootstrap = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetLoadBootstrap();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, settings.Template.ManifestFolderUri.FolderPath, "");
            alpaca.RegisterAll(bootstrap, loadBootstrap);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
        }
        public AlpacaContext AlpacaContext { get; private set; }
    }
}

