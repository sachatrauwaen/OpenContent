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
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components.UI;

#endregion

namespace Satrabel.OpenContent
{
    public partial class Edit : PortalModuleBase
    {
        /*
        /// <summary>
        /// Alpaca context for backward compatibility with the view
        /// </summary>
        public AlpacaContext AlpacaContext => Model?.AlpacaContext;
        */
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var pageContext = new WebFormsPageContext(Page, this);
            string itemId = Request.QueryString["id"];
            var editController = new EditControl(ModuleContext, pageContext, LocalResourceFile);
            module.Text = editController.Invoke(itemId);

            /*
            // Create client IDs container
            var clientIds = new EditControlClientIds
            {
                ScopeWrapper = ScopeWrapper.ClientID,
                Cancel = hlCancel.ClientID,
                Save = cmdSave.ClientID,
                Copy = cmdCopy.ClientID,
                Delete = hlDelete.ClientID,
                Versions = ddlVersions.ClientID
            };
            
            // Create the model using the factory method
            Model = EditModel.Create(ModuleContext, pageContext, itemId, clientIds);
            
            // Configure URLs
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            cmdCopy.NavigateUrl = Globals.NavigateURL();
            
            // Set URLs in model for reference
            Model.CancelUrl = hlCancel.NavigateUrl;
            Model.SaveUrl = cmdSave.NavigateUrl;
            Model.CopyUrl = cmdCopy.NavigateUrl;
            
            // Configure delete confirmation message
            Model.DeleteConfirmMessage = LocalizeSafeJsString("txtDeleteConfirmMessage");
            if (Model.IsMultiLingual)
            {
                Model.DeleteConfirmMessage = LocalizeSafeJsString("txtMLDeleteConfirmMessage");
            }
            Model.AlpacaContext.DeleteConfirmMessage = Model.DeleteConfirmMessage;
            
            // Set control visibility
            cmdCopy.Visible = Model.IsCopyVisible;
            hlDelete.Visible = Model.IsDeleteVisible;

            // Register Alpaca resources
            RegisterAlpacaResources();
            */
        }
        /*
        /// <summary>
        /// Registers all necessary Alpaca resources
        /// </summary>
        private void RegisterAlpacaResources()
        {
            var alpaca = new AlpacaEngine(
                Model.PageContext, 
                Model.PortalId, 
                Model.Settings.Template.ManifestFolderUri.FolderPath, 
                Model.TemplatePrefix);
            
            alpaca.RegisterAll(
                Model.Bootstrap, 
                Model.LoadBootstrap, 
                Model.LoadGlyphicons, 
                Model.BuilderV2);
        }
        */
    }
}

