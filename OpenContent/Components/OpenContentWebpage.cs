using System.Web;
using System.Web.UI;
using System.Web.WebPages;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Razor.Helpers;
using Satrabel.OpenContent.Components.TemplateHelpers;

namespace Satrabel.OpenContent.Components
{
    public abstract class OpenContentWebPage : WebPageBase
    {
        public dynamic Model { get; set; }

        #region Helpers

        protected internal DnnHelper Dnn { get; internal set; }

        protected internal HtmlHelper Html { get; internal set; }

        protected internal UrlHelper Url { get; internal set; }

        int JSOrder = (int)FileOrder.Js.DefaultPriority;
        int CSSOrder = (int)FileOrder.Css.ModuleCss;

        public void RegisterStyleSheet(string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.StartsWith("/"))
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

        public void RegisterScript(string jsfilename)
        {
            DnnUtils.RegisterScript((Page)HttpContext.Current.CurrentHandler, VirtualPath, jsfilename, JSOrder);
            JSOrder++;
        }
        public void RegisterForm(string view = "bootstrap")
        {
            var page = (Page)HttpContext.Current.CurrentHandler;
            FormHelpers.RegisterForm(page, VirtualPath, view, ref JSOrder);
        }
        public void RegisterEditForm(string prefix = "")
        {
            var page = (Page)HttpContext.Current.CurrentHandler;
            FormHelpers.RegisterEditForm(page, VirtualPath, Dnn.Portal.PortalId, prefix, ref JSOrder);
        }

        #endregion

        #region BaseClass Overrides

        /// <summary>
        /// When RenderPage() is called inside a Razor template, this method is called.
        /// </summary>
        /// <param name="parentPage">The parent page from which to read configuration information.</param>
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
        //public T Model { get; set; }
    }

}