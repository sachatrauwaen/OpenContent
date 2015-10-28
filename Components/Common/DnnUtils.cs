using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

namespace Satrabel.OpenContent.Components
{
    public static class DnnUtils
    {
        /// <summary>
        /// Gets the list of the DNN modules by friendlyName.
        /// </summary>
        /// <param name="friendlyName">Friendly name of the module.</param>
        /// <param name="tabFileManager"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static List<ModuleInfo> GetDnnModulesByFriendlyName(string friendlyName, int tabFileManager)
        {
            throw new NotImplementedException();
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
            var alternativeTab = TabController.Instance.GetTabByCulture(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current.PortalId, alternativeLocale);
            throw new NotImplementedException();
        }

        internal static string ToUrl(this IFileInfo file)
        {
            return FileManager.Instance.GetUrl(file);
        }
    }
}