using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
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

        public static IEnumerable<OpenContentModuleInfo> GetDnnOpenContentModules(int portalId)
        {
            ModuleController mc = new ModuleController();
            ArrayList modules = mc.GetModulesByDefinition(portalId, AppConfig.OPENCONTENT);
            return modules.OfType<ModuleInfo>().Select(module => new OpenContentModuleInfo(module));
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

        public static bool IsPublishedTab(this TabInfo tab)
        {
            return !tab.IsDeleted &&
                   (tab.StartDate == Null.NullDate || tab.StartDate < DateTime.Now) &&
                   (tab.EndDate == Null.NullDate || tab.EndDate > DateTime.Now);
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

        public static bool CheckIfEditable(this ModuleInfo activeModule, PortalSettings portalSettings)
        {
            bool isEditable;
            //first check some weird Dnn issue
            if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
            {
                var personalization = (PersonalizationInfo)HttpContext.Current.Items["Personalization"];
                if (personalization != null && personalization.UserId == -1)
                {
                    //this should never happen. 
                    //Let us make sure that the wrong value is no longer cached 
                    HttpContext.Current.Items.Remove("Personalization");
                }
            }
            bool blnPreview = (portalSettings.UserMode == PortalSettings.Mode.View);
            if (Globals.IsHostTab(portalSettings.ActiveTab.TabID))
            {
                blnPreview = false;
            }

            bool blnHasModuleEditPermissions = HasEditRightsOnModule(activeModule);


            if (blnPreview == false && blnHasModuleEditPermissions)
            {
                isEditable = true;
            }
            else
            {
                isEditable = false;
            }
            return isEditable;
        }

        public static bool HasEditRightsOnModule(this ModuleInfo activeModule)
        {
            bool blnHasModuleEditPermissions = false;
            if (activeModule != null)
            {
                //DNN already checks SuperUser and Administrator
                blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", activeModule);
            }
            return blnHasModuleEditPermissions;
        }

        // for openform compatibility
        public static string GetCurrentCultureCode()
        {
            return DnnLanguageUtils.GetCurrentCultureCode();
        }

    }
}