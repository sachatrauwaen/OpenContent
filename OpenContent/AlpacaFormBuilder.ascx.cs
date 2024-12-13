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
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;
using System.Web.UI.WebControls;
using Satrabel.OpenContent.Components.Render;

#endregion

namespace Satrabel.OpenContent
{
    public partial class AlpacaFormBuilder : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var pageContext = new WebFormsPageContext(Page, this);
            var globalSettingsController = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();

            var editLayout = globalSettingsController.GetEditLayout();
            var bootstrap = globalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && globalSettingsController.GetLoadBootstrap();
            bool loadGlyphicons = bootstrap && App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetLoadGlyphicons();
            bool builderV2 = App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).IsBuilderV2();
            string apikey = App.Services.CreateGlobalSettingsRepository(PortalId).GetGoogleApiKey();
            OpenContentSettings settings = this.OpenContentSettings();
            
            AlpacaEngine alpaca = new AlpacaEngine(pageContext, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, "builder");
            alpaca.RegisterAll(bootstrap, loadBootstrap, loadGlyphicons, builderV2);

            //string ItemId = Request.QueryString["id"];
            //AlpacaContext = new AlpacaContext(PortalId, ModuleId, ItemId, ScopeWrapper.ClientID, null, cmdSave.ClientID, null, null);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/builder/formbuilder.js", FileOrder.Js.DefaultPriority + 10);
            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/builder/formbuilder.css", FileOrder.Css.DefaultPriority);
            //ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/bootstrap/js/bootstrap.min.js", FileOrder.Js.DefaultPriority);
            //ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/bootstrap/css/bootstrap.min.css", FileOrder.Css.DefaultPriority);

            if (builderV2 || OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri))
            {
                string title = string.IsNullOrEmpty(settings.Template.Manifest.Title) ? "Data" : settings.Template.Manifest.Title + " ";
                string key = settings.Template.Collection == "Items" ? "": settings.Template.Collection;   
                ddlForms.Items.Add(new ListItem(title, key));
            }
            if (builderV2 || OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri, settings.Template.Key.ShortKey))
            {
                ddlForms.Items.Add(new ListItem("Settings", settings.Template.Key.ShortKey));
            }
            if (settings.Template.Manifest.AdditionalDataDefined())
            {
                foreach (var addData in settings.Template.Manifest.AdditionalDataDefinition)
                {
                    if (builderV2 || OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri, addData.Key))
                    {
                        string title = string.IsNullOrEmpty(addData.Value.Title) ? addData.Key : addData.Value.Title;
                        ddlForms.Items.Add(new ListItem(title, addData.Key));
                    }
                }
            }
            if (builderV2 || OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri, "form"))
            {
                ddlForms.Items.Add(new ListItem("Form", "form"));
            }

            AlpacaContext = new AlpacaContext(PortalId, ModuleId, null, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null, null);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = globalSettingsController.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.BuilderV2 = builderV2;
            AlpacaContext.GoogleApiKey = apikey;
        }
        public AlpacaContext AlpacaContext { get; private set; }


    }
}

