using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.UI;
using System.Web;
using System.Web.Mvc;

namespace Satrabel.OpenContent.Views
{
    public static class OpenContentHtmlHelper
    {
        public static IHtmlString GetLocalizedString(this HtmlHelper<EditModel> helper, string key)
        {
            return new MvcHtmlString(Localization.GetString(key, helper.ViewData.Model.ResourceFile));
        }
    }
}