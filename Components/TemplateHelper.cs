using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.WebPages;

namespace Satrabel.OpenContent.Components
{
    public static class TemplateHelper
    {
        public static void RegisterStyleSheet(this WebPageBase page, string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.StartsWith("/"))
                filePath = page.VirtualPath + filePath;

            ClientResourceManager.RegisterStyleSheet((Page)HttpContext.Current.CurrentHandler, filePath, FileOrder.Css.ModuleCss);

        }

        public static void RegisterScript(this WebPageBase page, string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.StartsWith("/"))
                filePath = page.VirtualPath + filePath;

            ClientResourceManager.RegisterScript((Page)HttpContext.Current.CurrentHandler, filePath, FileOrder.Js.DefaultPriority);

        }
    }
}