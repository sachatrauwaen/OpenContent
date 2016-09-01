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
    public static class DnnUtils
    {
        /// <summary>
        /// Gets the list of the DNN modules by friendlyName.
        /// </summary>
        /// <param name="friendlyName">Friendly name of the module.</param>
        /// <returns></returns>
        internal static ModuleInfo GetLastModuleByFriendlyName(string friendlyName)
        {
            //DesktopModuleController dmc = new DesktopModuleController();
            //DesktopModuleController.GetDesktopModuleByFriendlyName
            int portalid = PortalSettings.Current.PortalId;
            string culture = PortalSettings.Current.CultureCode;
            TabController tc = new TabController();
            ModuleController mc = new ModuleController();
            var modules = mc.GetModulesByDefinition(portalid, friendlyName).Cast<ModuleInfo>().OrderByDescending(m => m.ModuleID);
            foreach (var mod in modules)
            {
                var tab = tc.GetTab(mod.TabID, portalid, false);
                if (tab.CultureCode == culture || string.IsNullOrEmpty(tab.CultureCode))
                {
                    return mod;
                }
            }
            return modules.FirstOrDefault();
        }

        /// <summary>
        /// Gets the DNN tab by URL and culture.
        /// </summary>
        /// <param name="pageUrl">The page URL.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static TabInfo GetDnnTabByUrl(string pageUrl, string culture)
        {
            var alternativeLocale = LocaleController.Instance.GetLocale(culture);
            TabController tc = new TabController();
            var alternativeTab = tc.GetTabByCulture(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current.PortalId, alternativeLocale);
            throw new NotImplementedException();
        }

        internal static string ToUrl(this IFileInfo fileInfo)
        {
            if (fileInfo == null) return "";
            var url = FileManager.Instance.GetUrl(fileInfo);
            return url;
        }

        internal static string ToUrlWithoutLinkClick(this IFileInfo fileInfo)
        {
            if (fileInfo == null) return "";

            var url = FileManager.Instance.GetUrl(fileInfo);
            if (url.ToLower().Contains("linkclick"))
            {
                //this method works also for linkclick
                url = "/" + fileInfo.PhysicalPath.Replace(new FolderUri("/").PhysicalFullDirectory, "").Replace("\\", "/");
            }
            return url;
        }

        public static string GetCurrentCultureCode()
        {
            if (PortalSettings.Current == null)
                throw new Exception("No Portalsettings available in this context. Are you in the context of a Dnn Scheduler? It does not have Portalsettings");

            //strange issues with getting the correct culture.
            if (PortalSettings.Current.ActiveTab != null && PortalSettings.Current.ActiveTab.IsNeutralCulture) {
                if (!string.IsNullOrEmpty(PortalSettings.Current.PortalAlias.CultureCode))
                    return PortalSettings.Current.PortalAlias.CultureCode;
                else
                    return PortalSettings.Current.CultureCode;
            }
            if (PortalSettings.Current.ActiveTab != null) { 
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

        public static string GetCurrentCultureCode(ModuleInfo modInfo)
        {
            throw new NotImplementedException();
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
        public static OpenContentSettings OpenContentSettings(this ModuleInfo module)
        {
            return new OpenContentSettings(module.ModuleSettings);
        }
        public static OpenContentSettings OpenContentSettings(this ModuleInstanceContext module)
        {
            return new OpenContentSettings(module.Settings);
        }
        public static OpenContentSettings OpenContentSettings(this PortalModuleBase module)
        {
            return new OpenContentSettings(module.Settings);
        }

        internal static void RegisterScript(Page page, string sourceFolder, string jsfilename, int jsOrder)
        {
            if (page == null) return;
            if (string.IsNullOrEmpty(jsfilename)) return;

            if (!jsfilename.StartsWith("/") && !jsfilename.Contains("//"))
            {
                jsfilename = sourceFolder + jsfilename;
            }
            else if (!jsfilename.Contains("//"))
            {
                var file = new FileUri(jsfilename);
                jsfilename = file.UrlFilePath;
            }
            ClientResourceManager.RegisterScript(page, jsfilename, jsOrder);
            //ClientResourceManager.RegisterScript(page, page.ResolveUrl(jsfilename), jsOrder);
        }
    }
}