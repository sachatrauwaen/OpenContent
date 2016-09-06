using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace Satrabel.OpenContent.Components
{
    public static class DnnLanguageUtils
    {
        public static string GetCurrentCultureCode()
        {
            if (PortalSettings.Current == null)
                throw new Exception("No Portalsettings available in this context. Are you in the context of a Dnn Scheduler? It does not have Portalsettings");

            //strange issues with getting the correct culture.
            if (PortalSettings.Current.ActiveTab != null && PortalSettings.Current.ActiveTab.IsNeutralCulture)
            {
                if (!string.IsNullOrEmpty(PortalSettings.Current.PortalAlias.CultureCode))
                    return PortalSettings.Current.PortalAlias.CultureCode;
                else
                    return PortalSettings.Current.CultureCode;
            }
            if (PortalSettings.Current.ActiveTab != null)
            {
                return PortalSettings.Current.ActiveTab.CultureCode;
            }
            return LocaleController.Instance.GetCurrentLocale(PortalSettings.Current.PortalId).Code;
        }
        public static CultureInfo GetCurrentCulture()
        {
            return new CultureInfo(GetCurrentCultureCode());
        }
        internal static string GetCultureCode(int tabId, bool isSuperTab, PortalSettings settings)
        {
            string cultureCode = Null.NullString;
            if (settings != null)
            {
                TabController tc = new TabController();
                TabInfo linkTab = tc.GetTab(tabId, isSuperTab ? Null.NullInteger : settings.PortalId, false);
                if (linkTab != null)
                {
                    cultureCode = linkTab.CultureCode;
                }
                if (string.IsNullOrEmpty(cultureCode))
                {
                    cultureCode = Thread.CurrentThread.CurrentCulture.Name;
                }
            }

            return cultureCode;
        }

        public static bool IsMultiLingualPortal(int portalId)
        {
            return LocaleController.Instance.GetLocales(portalId).Count > 1;
        }
        public static Dictionary<string, Locale> GetPortalLocales(int portalId)
        {
            return LocaleController.Instance.GetLocales(portalId);
        }

        public static int GetTabByCurrentCulture(int portalId, int tabId, string cultureCode)
        {
            var tc = new TabController();
            Locale locale = LocaleController.Instance.GetLocale(cultureCode);
            var tab = tc.GetTabByCulture(tabId, portalId, locale);
            if (tab != null)
            {
                return tab.TabID;
            }
            else
            {
                return tabId;
            }
        }

    }
}