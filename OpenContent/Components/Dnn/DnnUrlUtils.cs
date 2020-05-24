using System;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.UI;

namespace Satrabel.OpenContent.Components.Dnn
{
    public static class DnnUrlUtils
    {
        public static string EditUrl(int moduleId, PortalSettings ps, params string[] additionalParameters)
        {
            return EditUrl("", "", "Edit", moduleId, ps, additionalParameters);
        }
        public static string EditUrl(string controlKey, int moduleId, PortalSettings ps, params string[] additionalParameters)
        {
            return EditUrl("", "", controlKey, moduleId, ps, additionalParameters);
        }
        public static string EditUrl(string keyName, string keyValue, int moduleId, PortalSettings ps, params string[] additionalParameters)
        {
            return EditUrl(keyName, keyValue, "Edit", moduleId, ps, additionalParameters);
        }
        public static string EditAddDataUrl(string keyName, string keyValue, int moduleId, PortalSettings ps, params string[] additionalParameters)
        {
            return EditUrl(keyName, keyValue, "EditAddData", moduleId, ps, additionalParameters);
        }

        //
        //private static string EditUrl(string keyName, string keyValue, string controlKey, int moduleId, PortalSettings ps)
        //{
        //    var parameters = new string[] { };
        //    return EditUrl(keyName, keyValue, controlKey, moduleId, ps, parameters);
        //}

        private static string EditUrl(string keyName, string keyValue, string controlKey, int moduleId, PortalSettings ps, params string[] additionalParameters)
        {
            string key = controlKey;
            if (string.IsNullOrEmpty(key))
            {
                key = "Edit";
            }
            string moduleIdParam = string.Empty;
            if (moduleId != 0)
            {
                moduleIdParam = $"mid={moduleId}";
            }

            string[] parameters;
            if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(keyValue))
            {
                parameters = new string[2 + additionalParameters.Length];
                parameters[0] = moduleIdParam;
                parameters[1] = $"{keyName}={keyValue}";
                Array.Copy(additionalParameters, 0, parameters, 2, additionalParameters.Length);
            }
            else
            {
                parameters = new string[1 + additionalParameters.Length];
                parameters[0] = moduleIdParam;
                Array.Copy(additionalParameters, 0, parameters, 1, additionalParameters.Length);
            }

            return NavigateUrl(ps.ActiveTab.TabID, moduleId, key, false, ps, parameters);
        }

        public static string NavigateUrl(int tabId)
        {
            return Globals.NavigateURL(tabId);
        }

        internal static string NavigateUrl(int tabId, PortalSettings portalSettings, string currentCultureCode, params string[] additionalParameters)
        {
            var isSuperTab = Globals.IsHostTab(tabId);
            return Globals.NavigateURL(tabId, isSuperTab, portalSettings, "", currentCultureCode, additionalParameters);
        }

        internal static string NavigateUrl(int detailTabId, string currentCultureCode, PortalSettings portalSettings, string pagename, params string[] additionalParameters)
        {
            var isSuperTab = Globals.IsHostTab(detailTabId);
            var url = Globals.NavigateURL(detailTabId, isSuperTab, portalSettings, "", currentCultureCode, pagename, additionalParameters);
            return url;
        }

        private static string NavigateUrl(int tabId, int moduleId, string controlKey, bool pageRedirect, PortalSettings ps, params string[] additionalParameters)
        {
            return NavigateUrl(tabId, moduleId, controlKey, Globals.glbDefaultPage, pageRedirect, ps, additionalParameters);
        }

        private static string NavigateUrl(int tabId, int moduleId, string controlKey, string pageName, bool pageRedirect, PortalSettings ps, params string[] additionalParameters)
        {
            var isSuperTab = Globals.IsHostTab(tabId);
            var settings = ps;
            var language = DnnLanguageUtils.GetCultureCode(tabId, isSuperTab, settings);
            var url = Globals.NavigateURL(tabId, isSuperTab, settings, controlKey, language, pageName, additionalParameters);

            // Making URLs call popups
            if (ps != null && ps.EnablePopUps)
            {
                if (!UIUtilities.IsLegacyUI(moduleId, controlKey, settings.PortalId) && (url.Contains("ctl")))
                {
                    url = UrlUtils.PopUpUrl(url, null, ps, false, pageRedirect);
                }
            }
            return url;
        }
    }
}