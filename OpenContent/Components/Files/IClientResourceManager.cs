using System.Web.UI;

namespace Satrabel.OpenContent.Components
{
    public interface IClientResourceManager
    {
        void RegisterStyleSheet(Page page, string resolveUrl);
        void RegisterScript(Page page, string resolveUrl, int priority = 0);
    }
}