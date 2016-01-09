using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Mail;

namespace Satrabel.OpenContent.Components
{
    public static class PageUtils
    {
        /// <summary>
        /// Sets the page title. (use from a module)
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="title">The title.</param>
        public static void SetPageTitle(Page page, string title)
        {
            var dnnpage = page as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                dnnpage.Header.Title = title;
                dnnpage.Title = title;
            }
        }

        /// <summary>
        /// Sets the page title. (use from Razor file)
        /// </summary>
        /// <param name="context">The context.  (use from Razor file)</param>
        /// <param name="title">The title.</param>
        public static void SetPageTitle(HttpContextBase context, string title)
        {
            if (context == null) return;
            if (string.IsNullOrWhiteSpace(title)) return;
            var pageObj = context.CurrentHandler as System.Web.UI.Page;
            if (pageObj != null)
            {
                SetPageTitle(pageObj, title);
            }
        }

        /// <summary>
        /// Sets the page description. Works from Razor too.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="description">The description.</param>
        public static void SetPageDescription(Page page, string description)
        {
            var dnnpage = page as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                dnnpage.Header.Description = description;
                dnnpage.Description = description;
                dnnpage.MetaDescription = description;
                var metaDescription = (HtmlMeta)dnnpage.FindControl("Head").FindControl("MetaDescription");
                if (metaDescription != null)
                {
                    metaDescription.Visible = true;
                    metaDescription.Content = description;
                }
            }
        }

        public static void SetPageDescription(HttpContextBase context, string description)
        {
            var pageObj = context.CurrentHandler as System.Web.UI.Page;
            if (pageObj != null)
            {
                SetPageDescription(pageObj, HttpUtility.HtmlDecode(Mail.ConvertToText(description)));
            }
        }

        public static void SetPageMeta(Page page, string meta)
        {
            var dnnpage = page as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                var htmlMeta = new LiteralControl(meta); 
                dnnpage.FindControl("Head").Controls.Add(htmlMeta);
            }
        }

        public static void AddBreadCrumbs(List<BreadCrumb> breadCrumbs)
        {
            int idx = 0;
            foreach (var item in breadCrumbs)
            {
                var tab = new DotNetNuke.Entities.Tabs.TabInfo();
                tab.TabID = -8888 + idx;
                tab.TabName = item.Name;
                tab.Url = item.Url;
                PortalSettings.Current.ActiveTab.BreadCrumbs.Add(tab);
                idx++;
            }

        }

        public static void AddBreadCrumb(string name, string url)
        {
            int idx = -8888;
            if (PortalSettings.Current.ActiveTab.BreadCrumbs.Count > 0)
            {
                TabInfo last = (TabInfo)DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.BreadCrumbs[PortalSettings.Current.ActiveTab.BreadCrumbs.Count - 1];
                if (last.TabID < -100)
                {
                    idx = last.TabID + 1;
                }
            }
            var tab = new DotNetNuke.Entities.Tabs.TabInfo();
            tab.TabID = idx;
            tab.TabName = name;
            tab.Url = url;
            PortalSettings.Current.ActiveTab.BreadCrumbs.Add(tab);
        }

        /// <summary>
        /// Clears the breadcrumbs and sets them again with a list of breadcrumbs items.
        /// </summary>
        /// <param name="breadCrumbs">The bread crumbs.</param>
        public static void SetBreadCrumbs(List<BreadCrumb> breadCrumbs)
        {
            PortalSettings.Current.ActiveTab.BreadCrumbs.Clear();
            int idx = 0;
            foreach (var item in breadCrumbs)
            {
                var tab = new DotNetNuke.Entities.Tabs.TabInfo();
                tab.TabID = -8888 + idx;
                tab.TabName = item.Name;
                tab.Url = item.Url;
                PortalSettings.Current.ActiveTab.BreadCrumbs.Add(tab);
                idx++;
            }
        }

        /// <summary>
        /// Clears the breadcrumbs on a Tab. You'll probably use AddBreadCrumb() after this call to fill Breadcrumbs list again.
        /// </summary>
        public static void ClearBreadCrumbs()
        {
            PortalSettings.Current.ActiveTab.BreadCrumbs.Clear();
        }

        /// <summary>
        /// Gets the breadcrumb. (a copy of the DNN version)
        /// </summary>
        /// <param name="separator">The string separator.</param>
        /// <param name="intRootLevel">The int root level.</param>
        /// <param name="strCssClass">The string CSS class.</param>
        /// <param name="useTitle">if set to <c>true</c> [use title].</param>
        /// <returns></returns>
        public static string GetBreadCrumb(string separator = " > ", int intRootLevel = 0, string strCssClass = "breadcrumbLink", bool useTitle = false)
        {
            string strBreadCrumbs = "";
            int intTab;
            for (intTab = intRootLevel; intTab <= PortalSettings.Current.ActiveTab.BreadCrumbs.Count - 1; intTab++)
            {
                var objTab = (TabInfo)PortalSettings.Current.ActiveTab.BreadCrumbs[intTab];
                if (objTab.TabID > 0)
                {
                    continue;
                }
                if (intTab != intRootLevel)
                {
                    strBreadCrumbs += HttpUtility.HtmlDecode(separator);
                }
                string strLabel = objTab.LocalizedTabName;
                if (useTitle && !String.IsNullOrEmpty(objTab.Title))
                {
                    strLabel = objTab.Title;
                }
                var tabUrl = objTab.FullUrl;
                if (objTab.DisableLink)
                {
                    strBreadCrumbs += "<span class=\"" + strCssClass + "\">" + strLabel + "</span>";
                }
                else
                {
                    strBreadCrumbs += "<a href=\"" + tabUrl + "\" class=\"" + strCssClass + "\">" + strLabel + "</a>";
                }
            }
            return strBreadCrumbs;
        }
    }

    public struct BreadCrumb
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}