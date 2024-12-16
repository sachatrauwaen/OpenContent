#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Render;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using IDataSource = Satrabel.OpenContent.Components.Datasource.IDataSource;
using SecurityAccessLevel = DotNetNuke.Security.SecurityAccessLevel;
using Newtonsoft.Json.Serialization;
using System.Reflection;

#endregion

namespace Satrabel.OpenContent
{
    /// <summary>
    /// This view will look in the settings if a template and all the necessary extra has already been defined.
    /// If so, it will render the template
    /// If not, it will display a 
    /// </summary>
    public partial class View : DotNetNuke.Web.Razor.RazorModuleBase, IActionable
    {
        private RenderInfo _renderinfo;
        private OpenContentSettings _settings;
        private RenderEngine _engine;
        private WebFormsPageContext _pageContext;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _pageContext= new WebFormsPageContext(this.Page, this);

            ModuleInfo module = ModuleContext.Configuration;

            if (App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetAutoAttach())
            {
                // auto attach a ContentLocalized OpenContent module to the reference module of the default language
                DnnModuleUtils.AutoAttachLocalizedModule(ref module);
            }
            _engine = new RenderEngine(OpenContentModuleConfig.Create(module, PortalSettings.Current), new DnnRenderContext(ModuleContext), LocalResourceFile);
            _renderinfo = _engine.Info;
            _settings = _engine.Settings;
            _engine.QueryString = Page.Request.QueryString;
            if (Page.Request.QueryString["id"] != null)
            {
                _engine.ItemId = Page.Request.QueryString["id"];
            }

            //initialize TemplateInitControl
            //TemplateInit ti = (TemplateInit)TemplateInitControl;
            //ti.ModuleContext = ModuleContext;
            //ti.Settings = _settings;
            //ti.Renderinfo = _renderinfo;
            //ti.ResourceFile = LocalResourceFile;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                DnnModuleUtils.AddEditorRole(ModuleContext.Configuration);
                //if (!Page.IsPostBack)
                //{
                //    AutoEditMode();
                //}
            }
            _engine.RenderWithTryCatch(_pageContext);
            
            /*
            if (App.Services.CreateGlobalSettingsRepository(ModuleContext.PortalId).GetCompositeCss())
            {
                //var absUrl = Utils.GetFullUrl(Request, Page.ResolveUrl($"~/DesktopModules/OpenContent/API/Resource/Css?tabid={activeTab.TabID}&portalid={activeTab.PortalID}"));
                //var absUrl = Utils.GetFullUrl(Request, Page.ResolveUrl($"~/API/OpenContent/Resource/Css?tabid={ModuleContext.TabId}&portalid={ModuleContext.PortalId}"));
                //App.Services.ClientResourceManager.RegisterStyleSheet(Page, absUrl);

                //var cssControl = Page.Header.FindControl("OpenContentCss");
                //if (cssControl == null)
                //{                    
                //    System.Web.UI.HtmlControls.HtmlLink css = new System.Web.UI.HtmlControls.HtmlLink();
                //    css.Href = Page.ResolveUrl($"~/API/OpenContent/Resource/Css?tabid={ModuleContext.TabId}&portalid={ModuleContext.PortalId}&cdv={ModuleContext.PortalSettings.CdfVersion}");
                //    css.Attributes["rel"] = "stylesheet";
                //    css.Attributes["type"] = "text/css";
                //    css.ID = "OpenContentCss";
                //    Page.Header.Controls.Add(css);
                //}
            }
            */
        }

        protected override void OnPreRender(EventArgs e)
        {
            //base.OnPreRender(e);
            //pHelp.Visible = false;
            pInit.Visible = false;
            GenerateAndRenderDemoData();
            if (_renderinfo.Template != null && !string.IsNullOrEmpty(_renderinfo.OutputString))
            {
                //Rendering was succesful.
                var lit = new LiteralControl(Server.HtmlDecode(_renderinfo.OutputString));
                Controls.Add(lit);
                var mst = _renderinfo.Template.Manifest;
                bool editWitoutPostback = mst != null && mst.EditWitoutPostback;
                if (ModuleContext.PortalSettings.EnablePopUps && ModuleContext.IsEditable && editWitoutPostback)
                {
                    AJAX.WrapUpdatePanelControl(lit, true);
                }
                _engine.IncludeMeta(_pageContext);
            }
            if (LogContext.IsLogActive && !Debugger.IsAttached)
            {
                //VirtualPathUtility.ToAbsolute

                _pageContext.RegisterScript(_pageContext.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
                var json = JsonConvert.SerializeObject(LogContext.Current.ModuleLogs(ModuleContext.ModuleId));
                json = json.Replace("<script>", "*script*");
                json = json.Replace("</script>", "*/script*");
                StringBuilder logScript = new StringBuilder();
                //logScript.AppendLine("<script type=\"text/javascript\"> ");
                logScript.AppendLine("$(document).ready(function () { ");
                logScript.AppendLine("var logs = " + json + "; ");
                logScript.AppendLine("$.fn.openContent.printLogs(\"Module " + ModuleContext.ModuleId + " - " + ModuleContext.Configuration.ModuleTitle + "\", logs);");
                logScript.AppendLine("});");
                //logScript.AppendLine("</script>");
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "logScript" + ModuleContext.ModuleId, /*DotNetNuke.UI.Utilities.ClientAPI.EscapeForJavascript*/(logScript.ToString()), true);
            }
        }

        #endregion

        #region Private Methods

        private void GenerateAndRenderDemoData()
        {
            bool otherModuleWithFilterSettings = _settings.IsOtherModule && _settings.Query.Exists();
            if (_renderinfo.ShowInitControl && !otherModuleWithFilterSettings)
            {
                /* thows error because _renderinfo.Module is null
                var templatemissing = OpenContentUtils.CheckOpenContentSettings(_renderinfo.Module, _settings);
                if (templatemissing)
                {
                    //todo: show message on screen
                }
                */
                // no data exist and ... -> show initialization
                if (ModuleContext.EditMode)
                {
                    // edit mode
                    if (_renderinfo.Template == null || ModuleContext.IsEditable)
                    {
                        RenderInitForm();
                        if (_renderinfo.ShowDemoData)
                        {
                            _engine.RenderDemoData(_pageContext);
                        }
                    }
                    else if (_renderinfo.Template != null)
                    {
                        _engine.RenderDemoData(_pageContext);
                    }
                }
                else if (_renderinfo.Template != null)
                {
                    _engine.RenderDemoData(_pageContext);
                }
            }
        }

        private void RenderInitForm()
        {
            //TemplateInit ti = (TemplateInit)TemplateInitControl;
            //ti.RenderInitForm();
            pInit.Visible = true;
            //App.Services.ClientResourceManager.RegisterStyleSheet(page, cssfilename.UrlFilePath);
            _pageContext.RegisterScript("~/DesktopModules/OpenContent/js/vue/vue.js");

            _pageContext.RegisterScript("~/DesktopModules/OpenContent/lama/dist/js/chunk-vendors.js", FileOrder.Js.DefaultPriority + 10);
            _pageContext.RegisterScript("~/DesktopModules/OpenContent/lama/dist/js/init.js", FileOrder.Js.DefaultPriority + 10);


        }
        public string Resource(string key)
        {
            return Localization.GetString(key + ".Text", LocalResourceFile);
        }
        #endregion

        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                var actionDefinitions = _engine.GetMenuActions();

                foreach (var item in actionDefinitions)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        item.Title,
                        item.ActionType.ToDnnActionType(),
                        "",
                        item.Image,
                        item.Url,
                        false,
                        item.AccessLevel.ToDnnSecurityAccessLevel(),
                        true,
                        item.NewWindow);
                }

                return actions;
            }
        }

        #endregion
    }
}