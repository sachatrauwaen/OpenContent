using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Skins;
using System;
using System.Linq;
using System.Web.UI;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Json;


namespace Satrabel.OpenContent
{
    public partial class RenderModule : SkinObjectBase
    {
        public int ModuleId { get; set; }
        public int TabId { get; set; }
        public string HideOnTabIds { get; set; }
        public bool ShowOnAdminTabs { get; set; }
        public bool ShowOnHostTabs { get; set; }
        public string Template { get; set; }
        private void InitializeComponent()
        {
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int[] hideTabs = new int[1] { TabId };
            if (string.IsNullOrEmpty(HideOnTabIds))
            {
                try
                {
                    hideTabs = HideOnTabIds.Split(',', ' ', ';').Select(s => int.Parse(s)).ToArray();
                }
                catch (Exception)
                {
                }
            }
            var activeTab = PortalSettings.ActiveTab;
            if (hideTabs.Contains(activeTab.TabID)) return;
            if (!ShowOnAdminTabs && activeTab.ParentId == PortalSettings.AdminTabId) return;
            if (!ShowOnHostTabs && activeTab.IsSuperTab) return;

            ModuleController mc = new ModuleController();
            var module = mc.GetModule(ModuleId, TabId, false);
            if (module == null)
            {
                DotNetNuke.UI.Skins.Skin.AddPageMessage(Page, "OpenContent RenderModule SkinObject", $"No module exist for TabId {TabId} and ModuleId {ModuleId} ", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                return;
            }
            if (!string.IsNullOrEmpty(Template))
            {
                var moduleClone = new ModuleInfo();
                foreach (System.Collections.DictionaryEntry item in module.ModuleSettings)
                {
                    moduleClone.ModuleSettings.Add(item.Key, item.Value);
                }
                moduleClone.ModuleID = module.ModuleID;
                moduleClone.TabID = module.TabID;
                moduleClone.TabModuleID = module.TabModuleID;
                moduleClone.PortalID = module.PortalID;
                moduleClone.ModuleSettings["template"] = Template;
                module = moduleClone;
            }
            var engine = new RenderEngine(new OpenContentModuleInfo(module, PortalSettings));
            try
            {
                engine.Render(Page);
            }
            catch (TemplateException ex)
            {
                RenderTemplateException(ex, module);
            }
            catch (InvalidJsonFileException ex)
            {
                RenderJsonException(ex, module);
            }
            catch (Exception ex)
            {
                LoggingUtils.ProcessModuleLoadException(this, module, ex);
            }
            if (engine.Info.Template != null && !string.IsNullOrEmpty(engine.Info.OutputString))
            {
                //Rendering was succesful.
                var lit = new LiteralControl(Server.HtmlDecode(engine.Info.OutputString));
                Controls.Add(lit);
                try
                {
                    engine.IncludeResourses(Page, this);
                }
                catch (Exception ex)
                {
                    DotNetNuke.UI.Skins.Skin.AddPageMessage(Page, "OpenContent RenderModule SkinObject", ex.Message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
                }
            }
        }
        private void RenderTemplateException(TemplateException ex, ModuleInfo module)
        {
            DotNetNuke.UI.Skins.Skin.AddPageMessage(Page, "OpenContent RenderModule SkinObject", "<p><b>Template error</b></p>" + ex.MessageAsHtml(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in tempate";
                LogContext.Log(module.ModuleID, logKey, "Error", ex.MessageAsList());
                LogContext.Log(module.ModuleID, logKey, "Model", ex.TemplateModel);
                LogContext.Log(module.ModuleID, logKey, "Source", ex.TemplateSource);
            }
            LoggingUtils.ProcessLogFileException(this, module, ex);
        }
        private void RenderJsonException(InvalidJsonFileException ex, ModuleInfo module)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(Page, "OpenContent RenderModule SkinObject", "<p><b>Json error</b></p>" + ex.MessageAsHtml(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            if (LogContext.IsLogActive)
            {
                var logKey = "Error in json";
                LogContext.Log(module.ModuleID, logKey, "Error", ex.MessageAsList());
                LogContext.Log(module.ModuleID, logKey, "Filename", ex.Filename);
            }
            LoggingUtils.ProcessLogFileException(this, module, ex);
        }

    }
}