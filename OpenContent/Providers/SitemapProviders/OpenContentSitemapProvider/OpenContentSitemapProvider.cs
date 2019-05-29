#region Copyright
// 
// Satrabel - http://www.satrabel.be
// Copyright (c) 2002-2018
// by Satrabel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Sitemap;
using Satrabel.OpenContent.Components.UrlRewriter;
using DotNetNuke.Instrumentation;

#endregion

namespace Satrabel.OpenContent.Providers.SitemapProviders
{
    public class OpenContentSitemapProvider : SitemapProvider
    {

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OpenContentSitemapProvider));

        private bool includeHiddenPages;
        private float minPagePriority;

        private PortalSettings ps;
        private bool useLevelBasedPagePriority;

        /// <summary>
        ///   Includes page urls on the sitemap
        /// </summary>
        /// <remarks>
        ///   Pages that are included:
        ///   - are not deleted
        ///   - are not disabled
        ///   - are normal pages (not links,...)
        ///   - are visible (based on date and permissions)
        /// </remarks>
        public override List<SitemapUrl> GetUrls(int portalId, PortalSettings ps, string version)
        {
            //Logger.Error("Opencontent sitemap generation page : start");



            //var objTabs = new TabController();
            OpenSitemapUrl pageUrl = null;
            var urls = new List<SitemapUrl>();

            useLevelBasedPagePriority = bool.Parse(PortalController.GetPortalSetting("SitemapLevelMode", portalId, "False"));
            minPagePriority = float.Parse(PortalController.GetPortalSetting("SitemapMinPriority", portalId, "0.1"), CultureInfo.InvariantCulture);
            includeHiddenPages = bool.Parse(PortalController.GetPortalSetting("SitemapIncludeHidden", portalId, "False"));

            this.ps = ps;
            var Locales = ps.ContentLocalizationEnabled ?
                                    LocaleController.Instance.GetPublishedLocales(ps.PortalId).Values :
                                    LocaleController.Instance.GetLocales(ps.PortalId).Values;

            bool MultiLanguage = Locales.Count > 1;


            var currentLanguage = ps.CultureCode;
            if (string.IsNullOrEmpty(currentLanguage))
            {
                currentLanguage = Localization.GetPageLocale(ps).Name;
            }
            var languagePublished = LocaleController.Instance.GetLocale(ps.PortalId, currentLanguage).IsPublished;
            var tabs = TabController.Instance.GetTabsByPortal(portalId).Values
                        .Where(t => /* !t.IsSystem && */ !ps.ContentLocalizationEnabled || (languagePublished && t.CultureCode.Equals(currentLanguage, StringComparison.InvariantCultureIgnoreCase)));
            foreach (TabInfo objTab in tabs)
            {
                if (objTab.CultureCode == currentLanguage || objTab.IsNeutralCulture)
                {
                    if (!objTab.IsDeleted && !objTab.DisableLink && objTab.TabType == TabType.Normal
                        && (Null.IsNull(objTab.StartDate) || objTab.StartDate < DateTime.Now)
                        && (Null.IsNull(objTab.EndDate) || objTab.EndDate > DateTime.Now)
                        && IsTabPublic(objTab.TabPermissions)
                        )
                    {
                        var allowIndex = true;
                        if ((!objTab.TabSettings.ContainsKey("AllowIndex") || !bool.TryParse(objTab.TabSettings["AllowIndex"].ToString(), out allowIndex) || allowIndex) &&
                             (includeHiddenPages || objTab.IsVisible))
                        {

                            // modules urls
                            var rules = OpenContentUrlProvider.GetRules(portalId);
                            foreach (var rule in rules.Where(r => r.TabId == objTab.TabID && r.InSitemap == true))
                            {

                                if (rule.CultureCode == null || rule.CultureCode == currentLanguage)
                                {
                                    string[] pars = rule.Parameters.Split('&');
                                    pageUrl = GetPageUrl(objTab, MultiLanguage ? currentLanguage : null, pars);
                                    // if module support ML
                                    //pageUrl.Alternates.AddRange(GetAlternates(objTab.TabID, pars));                                            
                                    urls.Add(pageUrl);
                                    //Logger.Error("Opencontent sitemap generation page : " + pageUrl.Url);
                                }
                            }
                        }
                    }
                }
            }


            return urls;
        }

        private List<OpenSitemapAlternate> GetAlternates(int tabId, params string[] AdditionalParameters)
        {
            List<OpenSitemapAlternate> alternates = new List<OpenSitemapAlternate>();
            var Locales = LocaleController.Instance.GetLocales(ps.PortalId).Values;
            if (Locales.Count > 1)
            {
                foreach (Locale loc in Locales)
                {
                    if (!ps.ContentLocalizationEnabled || loc.IsPublished)
                    {
                        string LocaleUrl;
                        if (Locales.Count(l => l.Code.Substring(0, 2) == loc.Code.Substring(0, 2)) > 1)
                            LocaleUrl = loc.Code;
                        else
                            LocaleUrl = loc.Code.Substring(0, 2);

                        bool CanViewPage;
                        var alt = new OpenSitemapAlternate()
                        {
                            Hreflang = LocaleUrl,
                            Href = newUrl(tabId, loc.Code, out CanViewPage, AdditionalParameters)
                        };


                        if (CanViewPage && !string.IsNullOrEmpty(alt.Href))
                        {
                            alternates.Add(alt);
                        }
                    }
                }
                /*
                if (ps.ActiveTab.TabID == ps.HomeTabId)
                {
                    var altLink = new HtmlLink();
                    altLink.Href = page.Request.Url.Scheme + "://" + PortalSettings.Current.PortalAlias.HTTPAlias;
                    altLink.Attributes["rel"] = "alternate";
                    altLink.Attributes["hreflang"] = "x-default";
                    page.Header.Controls.Add(altLink);
                }
                 */
            }
            if (alternates.Count == 1)
                alternates.Clear();

            return alternates;
        }

        private string newUrl(int tabId, string newLanguage, out bool canViewPage, params string[] additionalParameters)
        {
            canViewPage = true;
            string Url = "";
            Locale newLocale = LocaleController.Instance.GetLocale(newLanguage);
            //Ensure that the current ActiveTab is the culture of the new language
            //bool islocalized = false;
            TabInfo localizedTab = new TabController().GetTabByCulture(tabId, ps.PortalId, newLocale);
            if (localizedTab != null)
            {
                tabId = localizedTab.TabID;
                if (localizedTab.IsDeleted || localizedTab.TabType != TabType.Normal || !IsTabPublic(localizedTab.TabPermissions))
                {
                    canViewPage = false;
                }
                Url = Globals.NavigateURL(localizedTab.TabID, localizedTab.IsSuperTab, ps, "", newLanguage, additionalParameters);
            }
            else
            {
                canViewPage = false;
            }
            /*
            if (Url.ToLower().IndexOf(ps.PortalAlias.HTTPAlias.ToLower()) == -1)
            {
                // code to fix a bug in dnn5.1.2 for navigateurl
                if ((HttpContext.Current != null))
                {
                    Url = Globals.AddHTTP(HttpContext.Current.Request.Url.Host + Url);
                }
                else
                {
                    // try to use the portalalias
                    Url = Globals.AddHTTP(ps.PortalAlias.HTTPAlias.ToLower()) + Url;
                }
            }
            */
            return Url;

        }
        /// <summary>
        ///   Return the sitemap url node for the page
        /// </summary>
        /// <param name = "objTab">The page being indexed</param>
        /// <param name="language">Culture code to use in the URL</param>
        /// <returns>A SitemapUrl object for the current page</returns>
        /// <remarks>
        /// </remarks>
        private OpenSitemapUrl GetPageUrl(TabInfo objTab, string language, params string[] AdditionalParameters)
        {
            var pageUrl = new OpenSitemapUrl();
            pageUrl.Url = Globals.NavigateURL(objTab.TabID, objTab.IsSuperTab, ps, "", language, AdditionalParameters);

            if (pageUrl.Url.ToLower().IndexOf(ps.PortalAlias.HTTPAlias.ToLower()) == -1)
            {
                // code to fix a bug in dnn5.1.2 for navigateurl
                if ((HttpContext.Current != null))
                {
                    pageUrl.Url = Globals.AddHTTP(HttpContext.Current.Request.Url.Host + pageUrl.Url);
                }
                else
                {
                    // try to use the portalalias
                    pageUrl.Url = Globals.AddHTTP(ps.PortalAlias.HTTPAlias.ToLower()) + pageUrl.Url;
                }
            }
            pageUrl.Priority = GetPriority(objTab);
            pageUrl.LastModified = objTab.LastModifiedOnDate;
            var modCtrl = new ModuleController();
            foreach (ModuleInfo m in modCtrl.GetTabModules(objTab.TabID).Values)
            {
                if (m.LastModifiedOnDate > objTab.LastModifiedOnDate)
                {
                    pageUrl.LastModified = m.LastModifiedOnDate;
                }
            }
            pageUrl.ChangeFrequency = SitemapChangeFrequency.Daily;

            return pageUrl;
        }

        /// <summary>
        ///   When page level priority is used, the priority for each page will be computed from 
        ///   the hierarchy level of the page. 
        ///   Top level pages will have a value of 1, second level 0.9, third level 0.8, ...
        /// </summary>
        /// <param name = "objTab">The page being indexed</param>
        /// <returns>The priority assigned to the page</returns>
        /// <remarks>
        /// </remarks>
        protected float GetPriority(TabInfo objTab)
        {
            float priority = objTab.SiteMapPriority;
            //if (useLevelBasedPagePriority)
            //{
            //    if (objTab.Level >= 9)
            //    {
            //        priority = 0.1F;
            //    }
            //    else
            //    {
            //        priority = Convert.ToSingle(1 - (objTab.Level * 0.1));
            //    }
            //    if (priority < minPagePriority)
            //    {
            //        priority = minPagePriority;
            //    }
            //}

            if (priority < 0.1)
            {
                priority = 0.1F;
            }
            return priority;
        }

        #region "Security Check"

        public virtual bool IsTabPublic(TabPermissionCollection objTabPermissions)
        {
            string roles = objTabPermissions.ToString("VIEW");
            bool hasPublicRole = false;


            if ((roles != null))
            {
                // permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
                foreach (string role in roles.Split(new[] { ';' }))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        // Deny permission
                        if (role.StartsWith("!"))
                        {
                            string denyRole = role.Replace("!", "");
                            if ((denyRole == Globals.glbRoleUnauthUserName || denyRole == Globals.glbRoleAllUsersName))
                            {
                                hasPublicRole = false;
                                break;
                            }
                            // Grant permission
                        }
                        else
                        {
                            if ((role == Globals.glbRoleUnauthUserName || role == Globals.glbRoleAllUsersName))
                            {
                                hasPublicRole = true;
                                break;
                            }
                        }
                    }
                }
            }

            return hasPublicRole;
        }

        #endregion
    }
}
