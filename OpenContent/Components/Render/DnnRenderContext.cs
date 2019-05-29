using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor.Helpers;

namespace Satrabel.OpenContent.Components
{
    public interface IRenderContext
    {
        string EditUrl(string key = "");
        string EditUrl(string key, string value, string control = "");
        void InitHelpers(OpenContentWebPage webPage, string localResourceFile);
    }

    public class DnnRenderContext : IRenderContext
    {
        private readonly ModuleInstanceContext _moduleContext;

        public DnnRenderContext(ModuleInstanceContext moduleContext)
        {
            _moduleContext = moduleContext;
        }

        public string EditUrl(string key = "")
        {
            return string.IsNullOrEmpty(key) ? _moduleContext.EditUrl() : _moduleContext.EditUrl(key);
        }

        public string EditUrl(string key, string value, string control = "")
        {
            return string.IsNullOrEmpty(control) ? _moduleContext.EditUrl(key, value) : _moduleContext.EditUrl(key, value, control);
        }

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