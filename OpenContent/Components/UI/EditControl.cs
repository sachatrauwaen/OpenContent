#region Copyright

// 
// Copyright (c) 2015-2016
// by Satrabel
// 

#endregion

#region Using Statements

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Razor;
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components.Settings;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;

#endregion

namespace Satrabel.OpenContent.Components.UI
{
    /// <summary>
    /// Controller for handling OpenContent Edit functionality
    /// </summary>
    public class EditControl : BaseControl
    {
        private const string RazorScriptFile = "~/DesktopModules/Opencontent/Views/Edit.cshtml";
        #region Private Fields

        private readonly ModuleInstanceContext _moduleContext;
        private readonly IPageContext _pageContext;
        private readonly string _resourceFile;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EditController class
        /// </summary>
        /// <param name="moduleContext">The module context</param>
        /// <param name="pageContext">The page context</param>
        public EditControl(ModuleInstanceContext moduleContext, IPageContext pageContext, string resourceFile)
        {
            _moduleContext = moduleContext ?? throw new ArgumentNullException(nameof(moduleContext));
            _pageContext = pageContext ?? throw new ArgumentNullException(nameof(pageContext));
            _resourceFile = resourceFile ?? throw new ArgumentNullException(nameof(resourceFile));
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Default action for displaying the edit form
        /// </summary>
        /// <param name="id">The item ID to edit (optional)</param>
        /// <returns>View with EditModel</returns>
        public string Invoke2(string id = null)
        {
            try
            {
                var model = CreateEditModel(id);
                RegisterAlpacaResources(model);
                var RenderContext = new DnnRenderContext(_moduleContext);

                var writer = new StringWriter();
                try
                {
                    var razorEngine = new RazorEngine(RazorScriptFile, RenderContext, _resourceFile);
                    razorEngine.Render(writer, model);
                }
                catch (Exception ex)
                {
                    // LoggingUtils.RenderEngineException(this, ex);
                    string stack = string.Join("\n", ex.StackTrace.Split('\n').Where(s => s.Contains("\\Portals\\") && s.Contains("in")).Select(s => s.Substring(s.IndexOf("in"))).ToArray());
                    throw new TemplateException("Failed to render Razor template " + "\n" + stack, ex, model, RazorScriptFile);
                }
                return writer.ToString();
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use DNN's logging framework)
                throw new Exception($"Error initializing edit form: {ex.Message}", ex);
            }
        }

        public string Invoke(string id = null)
        {
            try
            {
                var model = CreateEditModel(id);
                RegisterAlpacaResources(model);
                var res = string.Empty;
                res = RazorEngineHelper.RenderPartialFromPath(RazorScriptFile, model);

                return res;
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use DNN's logging framework)
                throw new Exception($"Error initializing edit form: {ex.Message}", ex);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Creates and initializes the EditModel
        /// </summary>
        /// <param name="itemId">The item ID being edited</param>
        /// <returns>Configured EditModel</returns>
        private EditModel CreateEditModel(string itemId)
        {
            // Generate unique client IDs (similar to WebForms)
            var clientIds = GenerateClientIds();

            // Create the model using the factory method
           
       
            var model = new EditModel();
            model.ResourceFile = _resourceFile;

            // Load global settings
            var globalSettingsRepo = App.Services.CreateGlobalSettingsRepository(_moduleContext.PortalId);
            model.Bootstrap = globalSettingsRepo.GetEditLayout() != AlpacaLayoutEnum.DNN;
            model.LoadBootstrap = model.Bootstrap && globalSettingsRepo.GetLoadBootstrap();
            model.LoadGlyphicons = model.Bootstrap && globalSettingsRepo.GetLoadGlyphicons();
            model.BuilderV2 = globalSettingsRepo.IsBuilderV2();
            model.IsHorizontalLayout = globalSettingsRepo.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
            model.GoogleApiKey = globalSettingsRepo.GetGoogleApiKey();

            // Set module context
            model.PortalId = _moduleContext.PortalId;
            model.ModuleId = _moduleContext.ModuleId;
            model.ItemId = itemId;
            model.PageContext = _pageContext;

            // Load OpenContent settings using the extension method
            model.Settings = new OpenContentSettings(ComponentSettingsInfo.Create(_moduleContext.Configuration.ModuleSettings, _moduleContext.Configuration.TabModuleSettings));

            // Override builder version from manifest if specified
            if (model.Settings.Manifest != null && model.Settings.Manifest.BuilderVersion > 0)
            {
                model.BuilderV2 = model.Settings.Manifest.BuilderVersion == 2;
            }

            // Set template prefix
            model.TemplatePrefix = (string.IsNullOrEmpty(model.Settings.Template?.Collection) || model.Settings.Template.Collection == "Items") 
                ? "" : model.Settings.Template.Collection;

            // Set client IDs
            model.ScopeWrapperClientId = clientIds.ScopeWrapper;
            model.CancelClientId = clientIds.Cancel;
            model.SaveClientId = clientIds.Save;
            model.CopyClientId = clientIds.Copy;
            model.DeleteClientId = clientIds.Delete;
            model.VersionsClientId = clientIds.Versions;

            // Create Alpaca context
            model.AlpacaContext = new AlpacaContext(
                model.PortalId, 
                model.ModuleId, 
                itemId, 
                model.ScopeWrapperClientId, 
                model.CancelClientId, 
                model.SaveClientId, 
                model.CopyClientId, 
                model.DeleteClientId, 
                model.VersionsClientId);

            model.AlpacaContext.Bootstrap = model.Bootstrap;
            model.AlpacaContext.Horizontal = model.IsHorizontalLayout;
            model.AlpacaContext.IsNew = model.Settings.Template?.IsListTemplate == true && string.IsNullOrEmpty(itemId);
            model.AlpacaContext.BuilderV2 = model.BuilderV2;
            model.AlpacaContext.GoogleApiKey = model.GoogleApiKey;

            // Set state properties
            model.IsNew = model.AlpacaContext.IsNew;
            model.IsMultiLingual = DnnLanguageUtils.IsMultiLingualPortal(model.PortalId);

            // Set visibility properties
            model.IsCopyVisible = model.Settings.Template?.Manifest?.DisableCopy != true;
            model.IsDeleteVisible = model.Settings.Template?.Manifest?.DisableDelete != true;


            // Configure URLs
            ConfigureUrls(model);

            // Configure localization and delete confirmation
            ConfigureLocalization(model);

            return model;
        }


        /// <summary>
        /// Generates client IDs for form controls
        /// </summary>
        /// <returns>EditControlClientIds with generated IDs</returns>
        private EditControlClientIds GenerateClientIds()
        {
            var moduleId = _moduleContext.ModuleId;

            return new EditControlClientIds
            {
                ScopeWrapper = $"ScopeWrapper_{moduleId}",
                Cancel = $"hlCancel_{moduleId}",
                Save = $"cmdSave_{moduleId}",
                Copy = $"cmdCopy_{moduleId}",
                Delete = $"hlDelete_{moduleId}",
                Versions = $"ddlVersions_{moduleId}"
            };
        }

        /// <summary>
        /// Configures navigation URLs for the model
        /// </summary>
        /// <param name="model">The edit model to configure</param>
        private void ConfigureUrls(EditModel model)
        {
            // Configure URLs - using DNN's navigation URL generation
            model.CancelUrl = Globals.NavigateURL();
            model.SaveUrl = Globals.NavigateURL();
            model.CopyUrl = Globals.NavigateURL();
        }

        /// <summary>
        /// Configures localization and delete confirmation messages
        /// </summary>
        /// <param name="model">The edit model to configure</param>
        private void ConfigureLocalization(EditModel model)
        {
            // Configure delete confirmation message
            model.DeleteConfirmMessage = GetLocalizedSafeJsString("txtDeleteConfirmMessage");

            if (model.IsMultiLingual)
            {
                model.DeleteConfirmMessage = GetLocalizedSafeJsString("txtMLDeleteConfirmMessage");
            }

            // Update the AlpacaContext with the confirmation message
            model.AlpacaContext.DeleteConfirmMessage = model.DeleteConfirmMessage;
        }

        /// <summary>
        /// Gets a localized string safe for JavaScript usage
        /// </summary>
        /// <param name="key">The localization key</param>
        /// <returns>JavaScript-safe localized string</returns>
        private string GetLocalizedSafeJsString(string key)
        {
            var localizedString = DotNetNuke.Services.Localization.Localization.GetSafeJSString(key, _resourceFile);
            if (string.IsNullOrEmpty(localizedString))
            {
                return key;
            }

            // Make the string safe for JavaScript
            return localizedString
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
        }

        /// <summary>
        /// Registers all necessary Alpaca resources
        /// </summary>
        /// <param name="model">The edit model containing configuration</param>
        private void RegisterAlpacaResources(EditModel model)
        {
            if (model.Settings?.Template?.ManifestFolderUri?.FolderPath == null)
            {
                // No template configured yet, skip resource registration
                return;
            }

            var alpaca = new AlpacaEngine(
                model.PageContext,
                model.PortalId,
                model.Settings.Template.ManifestFolderUri.FolderPath,
                model.TemplatePrefix);

            alpaca.RegisterAll(
                model.Bootstrap,
                model.LoadBootstrap,
                model.LoadGlyphicons,
                model.BuilderV2);
        }

        #endregion
    }
}