using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.WebPages;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Razor.Helpers;


namespace Satrabel.OpenContent.Components
{
    public abstract class OpenContentWebPage : WebPageBase
    {
        #region Helpers

        protected internal DnnHelper Dnn { get; internal set; }

        protected internal HtmlHelper Html { get; internal set; }

        protected internal UrlHelper Url { get; internal set; }

        int JSOrder = (int)FileOrder.Js.DefaultPriority;
        int CSSOrder = (int)FileOrder.Css.ModuleCss;

        public void RegisterStyleSheet(string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.Contains("/"))
            {
                filePath = VirtualPath + filePath;
            }
            if (!filePath.StartsWith("http"))
            {
                var file = new FileUri(filePath);
                filePath = file.UrlFilePath;
            }

            ClientResourceManager.RegisterStyleSheet((Page)HttpContext.Current.CurrentHandler, filePath, CSSOrder);
            CSSOrder++;
        }

        public void RegisterScript(string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.StartsWith("/"))
            {
                filePath = VirtualPath + filePath;
            }
            else if (!filePath.StartsWith("http"))
            {
                var file = new FileUri(filePath);
                filePath = file.UrlFilePath;
            }

            ClientResourceManager.RegisterScript((Page)HttpContext.Current.CurrentHandler, filePath, JSOrder);
            JSOrder++;
        }

        #endregion

        #region BaseClass Overrides

        protected override void ConfigurePage(WebPageBase parentPage)
        {
            base.ConfigurePage(parentPage);

            //Child pages need to get their context from the Parent
            Context = parentPage.Context;
        }

        #endregion
    }

    public abstract class OpenContentWebPage<T> : OpenContentWebPage
    {
        public T Model { get; set; }
    }

}