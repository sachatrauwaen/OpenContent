using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class UrlHelpers
    {
        public static string NavigateUrl(int targetTabId, string detailItemId, string detailItemTitle, params string[] additionalParameters)
        {
            if (string.IsNullOrEmpty(detailItemTitle)) return null;
            if (targetTabId == 0) return null;

            detailItemTitle = detailItemTitle.CleanupUrl();
            string[] param = { "id", detailItemId };
            param = param.Concat(additionalParameters).ToArray();
            var newUrl = TestableGlobals.Instance.NavigateURL(targetTabId, false, PortalSettings.Current, string.Empty, string.Empty, detailItemTitle, param);
            return newUrl.Length <= 230 ? newUrl : newUrl.Substring(0, 230); //actual url has ?default.aspx 
        }
    }
}