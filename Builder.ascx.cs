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
using Satrabel.OpenContent.Components.Manifest;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;

#endregion

namespace Satrabel.OpenContent
{
    public partial class Builder : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();

            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, "" /*settings.Template.Uri().FolderPath*/, "builder");
            alpaca.RegisterAll(true);
            string ItemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, ItemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/builder/formbuilder.js", FileOrder.Js.DefaultPriority);
            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/builder/formbuilder.css", FileOrder.Css.DefaultPriority);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/bootstrap/js/bootstrap.min.js", FileOrder.Js.DefaultPriority);
            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/bootstrap/css/bootstrap.min.css", FileOrder.Css.DefaultPriority);
        }
        public AlpacaContext AlpacaContext { get; private set ; }


    }
}

