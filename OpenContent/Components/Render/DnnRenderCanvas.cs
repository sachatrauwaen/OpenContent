using System;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor.Helpers;
using Satrabel.OpenContent.Components.Dnn;

namespace Satrabel.OpenContent.Components
{
    public interface IRenderCanvas
    {
        string EditUrl(string key = "");
        string EditUrl(string key, string value, string control = "");
        void InitHelpers(OpenContentWebPage webPage, string localResourceFile);


        string PageUrl { get; }
        int ModuleId { get; }
    }

    public class DnnRenderCanvas : IRenderCanvas
    {
        private readonly ModuleInstanceContext _moduleContext;

        public DnnRenderCanvas(object moduleContext)
        {
            _moduleContext = (ModuleInstanceContext)moduleContext;
        }

        public string EditUrl(string key = "")
        {
            if (string.IsNullOrEmpty(key))
                return _moduleContext.EditUrl();
            else
                return _moduleContext.EditUrl(key);
        }

        public string EditUrl(string key, string value, string control = "")
        {
            if (string.IsNullOrEmpty(control))
                return _moduleContext.EditUrl(key, value);
            else
                return _moduleContext.EditUrl(key, value, control);
        }

        public string PageUrl => DnnUrlUtils.NavigateUrl(_moduleContext.TabId);
        public int ModuleId => _moduleContext.ModuleId;

        public void InitHelpers(OpenContentWebPage webPage, string localResourceFile)
        {
            if (_moduleContext != null)
            {
                webPage.Dnn = new DnnHelper(_moduleContext);
                webPage.Html = new HtmlHelper(_moduleContext, localResourceFile);
                webPage.Url = new UrlHelper(_moduleContext);
            }
        }
    }
}