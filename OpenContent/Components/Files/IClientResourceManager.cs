using Satrabel.OpenContent.Components.Render;
using System.Web.UI;

namespace Satrabel.OpenContent.Components
{
    public interface IClientResourceManager
    {
        void RegisterStyleSheet(Page page, string resolveUrl);

        void RegisterStyleSheet(IPageContext page, string resolveUrl);
        void RegisterScript(Page page, string resolveUrl, int priority = 0);

        void RegisterScript(IPageContext page, string resolveUrl, int priority = 0);
    }
}