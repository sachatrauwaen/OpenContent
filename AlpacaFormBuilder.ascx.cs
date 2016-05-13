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
using System.Web.UI.WebControls;

#endregion

namespace Satrabel.OpenContent
{
    public partial class AlpacaFormBuilder : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, "" /*settings.Template.Uri().FolderPath*/, "builder");
            alpaca.RegisterAll(false);
            //string ItemId = Request.QueryString["id"];
            //AlpacaContext = new AlpacaContext(PortalId, ModuleId, ItemId, ScopeWrapper.ClientID, null, cmdSave.ClientID, null, null);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/builder/formbuilder.js", FileOrder.Js.DefaultPriority);
            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/builder/formbuilder.css", FileOrder.Css.DefaultPriority);
            //ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/bootstrap/js/bootstrap.min.js", FileOrder.Js.DefaultPriority);
            //ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/bootstrap/css/bootstrap.min.css", FileOrder.Css.DefaultPriority);

            if (OpenContentUtils.BuilderExist(settings.Template.ManifestDir))
            {
                string title = string.IsNullOrEmpty(settings.Template.Manifest.Title) ? "Data" : settings.Template.Manifest.Title + " ";
                ddlForms.Items.Add(new ListItem(title, ""));
            }
            if (OpenContentUtils.BuilderExist(settings.Template.ManifestDir, settings.Template.Key.ShortKey))
            {
                ddlForms.Items.Add(new ListItem("Settings", settings.Template.Key.ShortKey));
            }
            if (settings.Template.Manifest.AdditionalData != null)
            {
                foreach (var addData in settings.Template.Manifest.AdditionalData)
                {
                    if (OpenContentUtils.BuilderExist(settings.Template.ManifestDir, addData.Key))
                    {
                        string title = string.IsNullOrEmpty(addData.Value.Title) ? addData.Key : addData.Value.Title;
                        ddlForms.Items.Add(new ListItem(title, addData.Key));
                    }
                }
            }

        }
        //public AlpacaContext AlpacaContext { get; private set ; }


    }
}

