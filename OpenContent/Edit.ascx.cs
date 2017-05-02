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
            var bootstrap = App.Services.GlobalSettings().GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && App.Services.GlobalSettings().GetLoadBootstrap();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            cmdCopy.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, "");
            alpaca.RegisterAll(bootstrap, loadBootstrap);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, cmdCopy.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = App.Services.GlobalSettings().GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.IsNew = settings.Template.IsListTemplate && string.IsNullOrEmpty(itemId);
        }
        public AlpacaContext AlpacaContext { get; private set; }
    }
}

