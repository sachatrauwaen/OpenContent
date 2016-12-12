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
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Json;
using System.Web.UI;
using System.Diagnostics;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client;
using Newtonsoft.Json;
using System.Text;
using System.Collections;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditCollection : DotNetNuke.Web.Razor.RazorModuleBase
    {
        RenderEngine engine;
        public AlpacaContext AlpacaContext { get; private set; }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var editLayout = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout();
            var bootstrap = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout() != AlpacaLayoutEnum.DNN;
            bool loadBootstrap = bootstrap && OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetLoadBootstrap();
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = ModuleContext.Configuration.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, settings.Template.ManifestFolderUri.FolderPath, "");
            alpaca.RegisterAll(bootstrap, loadBootstrap);
            string itemId = Request.QueryString["id"];
            AlpacaContext = new AlpacaContext(ModuleContext.PortalId, ModuleContext.ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, hlDelete.ClientID, ddlVersions.ClientID);
            AlpacaContext.Bootstrap = bootstrap;
            AlpacaContext.Horizontal = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;

            ModuleInfo module = ModuleContext.Configuration;
            IDictionary moduleSettings = new Hashtable(module.ModuleSettings);
            moduleSettings["template"] = settings.TemplateKey.Folder + "/" + "formsubmissions";
            moduleSettings["data"] = "";
            moduleSettings["query"] = "";

            /*
            var moduleClone = new ModuleInfo();
            foreach (System.Collections.DictionaryEntry item in module.ModuleSettings)
            {
                moduleClone.ModuleSettings.Add(item.Key, item.Value);
            }
            moduleClone.ModuleID = module.ModuleID;
            moduleClone.TabID = module.TabID;
            moduleClone.TabModuleID = module.TabModuleID;
            moduleClone.PortalID = module.PortalID;
            moduleClone.ModuleSettings["template"] = settings.TemplateKey.ToString();
            module = moduleClone;
            */
            engine = new RenderEngine(module, moduleSettings);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            engine.QueryString = Page.Request.QueryString;
            if (Page.Request.QueryString["id"] != null)
            {
                engine.ItemId = Page.Request.QueryString["id"];
            }
            engine.LocalResourceFile = LocalResourceFile;
            engine.ModuleContext = ModuleContext;
        }
        protected override void OnPreRender(EventArgs e)
        {
            //base.OnPreRender(e);
            //pHelp.Visible = false;
            try
            {
                engine.Render(Page);
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex);
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex);
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, ex);
            }
            if (engine.Info.Template != null && !string.IsNullOrEmpty(engine.Info.OutputString))
            {
                //Rendering was succesful.
                //var lit = new LiteralControl(Server.HtmlDecode(engine.Info.OutputString));
                //Controls.Add(lit);
                Literal1.Text = Server.HtmlDecode(engine.Info.OutputString);
                var mst = engine.Info.Template.Manifest;
                try
                {
                    engine.IncludeResourses(Page, this);
                }
                catch (Exception ex)
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, ex.Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                }
                //if (DemoData) pDemo.Visible = true;
            }
            if (LogContext.IsLogActive && !Debugger.IsAttached)
            {
                ClientResourceManager.RegisterScript(Page, Page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
                StringBuilder logScript = new StringBuilder();
                logScript.AppendLine("<script type=\"text/javascript\"> ");
                logScript.AppendLine("$(document).ready(function () { ");
                logScript.AppendLine("var logs = " + JsonConvert.SerializeObject(LogContext.Current.ModuleLogs(ModuleContext.ModuleId)) + "; ");
                logScript.AppendLine("$.fn.openContent.printLogs('Module " + ModuleContext.ModuleId + " - " + ModuleContext.Configuration.ModuleTitle + "', logs);");
                logScript.AppendLine("});");
                logScript.AppendLine("</script>");
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "logScript" + ModuleContext.ModuleId, logScript.ToString());
            }
        }

        #region Exceptions
        private void RenderTemplateException(TemplateException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template error</b></p>" + ex.MessageAsHtml, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template source</b></p>" + Server.HtmlEncode(ex.TemplateSource).Replace("\n", "<br/>"), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Template model</b></p> <pre>" + JsonConvert.SerializeObject(ex.TemplateModel, Formatting.Indented)/*.Replace("\n", "<br/>")*/+"</pre>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            //lErrorMessage.Text = ex.HtmlMessage;
            //lErrorModel.Text = "<pre>" + JsonConvert.SerializeObject(ex.TemplateModel, Formatting.Indented)/*.Replace("\n", "<br/>")*/+"</pre>";
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in tempate";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Model", ex.TemplateModel);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Source", ex.TemplateSource);
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }
        private void RenderJsonException(InvalidJsonFileException ex)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p><b>Json error</b></p>" + ex.MessageAsHtml, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in json";
                LogContext.Log(ModuleContext.ModuleId, logKey, "Error", ex.MessageAsList);
                LogContext.Log(ModuleContext.ModuleId, logKey, "Filename", ex.Filename);
                //LogContext.Log(logKey, "StackTrace", ex.StackTrace);
                //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "<p>More info is availale on de browser console (F12)</p>", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
            }
            LoggingUtils.ProcessLogFileException(this, ex);
        }
        #endregion
    }
}

