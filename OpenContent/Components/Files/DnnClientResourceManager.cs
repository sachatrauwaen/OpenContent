using System.Web.UI;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace Satrabel.OpenContent.Components
{
    public class DnnClientResourceManager : IClientResourceManager
    {
        public void RegisterStyleSheet(Page page, string relativeFilePath)
        {
            ClientResourceManager.RegisterStyleSheet(page, page.ResolveUrl(relativeFilePath), FileOrder.Css.PortalCss);
        }

        public void RegisterScript(Page page, string relativeFilePath, int priority = 0)
        {
            ClientResourceManager.RegisterScript(page, page.ResolveUrl(relativeFilePath), FileOrder.Js.DefaultPriority + priority);
        }
    }
}