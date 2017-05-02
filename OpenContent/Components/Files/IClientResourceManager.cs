using System.Web.UI;
using DotNetNuke.Web.Client;

namespace Satrabel.OpenContent.Components
{
    public interface IClientResourceManager
    {
        void RegisterStyleSheet(Page page, string resolveUrl);
        void RegisterScript(Page page, string resolveUrl, int priority = 0);
    }
}