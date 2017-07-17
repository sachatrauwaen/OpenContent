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
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using DotNetNuke.Common.Utilities;

#endregion

namespace Satrabel.OpenContent
{
    public partial class Edit : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!CanEditOrAddContent())
            {
                // If the user is not authorized to add/edit this content redirect them back to the default view control.
                this.Response.Redirect(Globals.NavigateURL(this.TabId));
            }

            var bootstrap = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetLoadBootstrap();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            cmdCopy.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, settings.Template.ManifestFolderUri.FolderPath, "");
            alpaca.RegisterAll(bootstrap, loadBootstrap);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, cmdCopy.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            AlpacaContext.IsNew = settings.Template.IsListTemplate && string.IsNullOrEmpty(itemId);
        }

        public AlpacaContext AlpacaContext { get; private set; }

        /// <summary>Determines whether the current user is authorized to edit this content.</summary>
        /// <returns><c>true</c> if this current user is authroized to edit this content; otherwise, <c>false</c>.</returns>
        private bool CanEditOrAddContent()
        {
            var id = this.Request.QueryString["id"];
            var moduleInfo = new OpenContentModuleInfo(this.ModuleConfiguration);
            var createdByUserid = string.IsNullOrEmpty(id) ? Null.NullInteger : GetCreatedByUserIdForEditContent(moduleInfo, id);
            return !moduleInfo.Settings.Manifest.DisableEdit
                && OpenContentUtils.HasEditPermissions(PortalSettings, moduleInfo.ViewModule, moduleInfo.Settings.Manifest.GetEditRole(), createdByUserid);
        }

        /// <summary>Gets the created by user ID for the content.</summary>
        /// <param name="moduleInfo">The module information.</param>
        /// <param name="id">The content ID.</param>
        /// <returns>The created by user ID or -1 if the content was not found.</returns>
        private int GetCreatedByUserIdForEditContent(OpenContentModuleInfo moduleInfo, string id)
        {
            var ds = DataSourceManager.GetDataSource(moduleInfo.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(moduleInfo);
            IDataItem dsItem = null;

            if (moduleInfo.IsListMode() && !string.IsNullOrEmpty(id))
            {
                dsItem = ds.Get(dsContext, id);
            }
            else
            {
                dsContext.Single = true;
                dsItem = ds.Get(dsContext, null);
            }

            return dsItem?.CreatedByUserId ?? Null.NullInteger;
        }
    }
}

