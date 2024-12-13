using DotNetNuke.Framework.Providers;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Satrabel.OpenContent.Components.Render
{
    public class WebFormsPageContext : IPageContext
    {
        private Page page;
        private Control module;

        public WebFormsPageContext(Page page, Control module)
        {
            this.page = page;
            this.module = module;
        }

        public string Title
        {
            get
            {
                return page.Title;
            }
            set
            {
                page.Title = value;
            }
        }

        public void AddLiteral(string str)
        {
            if (module != null)
            {
                var litPartial = new LiteralControl(str);
                module.Controls.Add(litPartial);
            }
        }

        public void AddModuleMessage(string messsage, ModuleMessage.ModuleMessageType moduleMessageType)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(page,messsage, moduleMessageType);
        }

        public void ProcessModuleLoadException(string friendlyMessage, Exception exc)
        {
            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(friendlyMessage, page, exc);
        }

        public void RegisterClientScriptBlock(Type type, string key, string script, bool addScriptTag)
        {
            page.ClientScript.RegisterClientScriptBlock(type, key, script, addScriptTag);
        }

        public void RegisterClientVariable(string strVar, string strValue, bool overwrite)
        {
            DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(page, strVar, strValue, overwrite);
        }

        public void RegisterScript(string filePath, FileOrder.Js priority = FileOrder.Js.DefaultPriority, string provider = "DnnBodyProvider")
        {
            ClientResourceManager.RegisterScript(page, filePath, priority, provider);
        }

        public void RegisterScript(string filePath, int jsOrder, string provider)
        {
            ClientResourceManager.RegisterScript(page, filePath, jsOrder, provider);
        }

        public void RegisterStartupScript(Type type, string key, string script, bool addScriptTags)
        {
            ScriptManager.RegisterStartupScript(page, GetType(), key, script, addScriptTags);
        }

        public void RegisterStyleSheet(string filePath, FileOrder.Css order = FileOrder.Css.DefaultPriority)
        {
            ClientResourceManager.RegisterStyleSheet(page, filePath, order);
        }

        public string ResolveUrl(string relativeFilePath)
        {
            return VirtualPathUtility.ToAbsolute(relativeFilePath);
        }

        public void SetPageDescription(string metaDescription)
        {
            PageUtils.SetPageDescription(page, metaDescription);
        }

        public void SetPageMeta(string metaOther)
        {
            PageUtils.SetPageMeta(page, metaOther);
        }
    }
}