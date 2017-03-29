using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using DotNetNuke.Common.Utilities;
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

        public static void RemoveBreadCrumb(int tabid)
        {
            for (int i = 0; i < PortalSettings.Current.ActiveTab.BreadCrumbs.Count; i++)
            {
                TabInfo item = (TabInfo)PortalSettings.Current.ActiveTab.BreadCrumbs[i];
                if (item.TabID == tabid)
                {
                    PortalSettings.Current.ActiveTab.BreadCrumbs.RemoveAt(i);
                    break;
                }
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
        public static string GetBreadCrumbOld(string separator = " > ", int intRootLevel = 0, string strCssClass = "breadcrumbLink", bool useTitle = false)
        {

            string strBreadCrumbs = "";
            int intTab;
            for (intTab = intRootLevel; intTab <= PortalSettings.Current.ActiveTab.BreadCrumbs.Count - 1; intTab++)
            {
                var objTab = (TabInfo)PortalSettings.Current.ActiveTab.BreadCrumbs[intTab];
                if (intTab != intRootLevel)
                {
                    strBreadCrumbs += HttpUtility.HtmlDecode(separator);
                }
                string strLabel = objTab.LocalizedTabName;
                if (useTitle && !string.IsNullOrEmpty(objTab.Title))
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

        /// <summary>
        /// Gets the breadcrumb. (a copy of the DNN version)
        /// </summary>
        /// <param name="separator">The string separator.</param>
        /// <param name="rootLevel">The int root level.</param>
        /// <param name="cssClass">The string CSS class.</param>
        /// <param name="useTitle">if set to <c>true</c> [use title].</param>
        /// <returns></returns>
        public static string GetBreadCrumb(string separator = " > ", int rootLevel = 0, string cssClass = "breadcrumbLink", bool useTitle = false)
        {

            bool showRoot = false;
            string homeUrl = "";
            string homeTabName = "Root";
            var portalSettings = PortalSettings.Current;
            var position = 1;

            if (rootLevel < 0)
            {
                showRoot = true;
                rootLevel = 0;
            }

            StringBuilder breadcrumb = new StringBuilder("<span itemscope itemtype=\"http://schema.org/BreadcrumbList\">");

            // Without checking if the current tab is the home tab, we would duplicate the root tab
            if (showRoot && portalSettings.ActiveTab.TabID != portalSettings.HomeTabId)
            {
                // Add the current protocal to the current URL
                homeUrl = DotNetNuke.Common.Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias);

                // Make sure we have a home tab ID set
                if (portalSettings.HomeTabId != -1)
                {
                    homeUrl = DotNetNuke.Common.Globals.NavigateURL(portalSettings.HomeTabId);

                    var tc = new TabController();
                    var homeTab = tc.GetTab(portalSettings.HomeTabId, portalSettings.PortalId, false);
                    homeTabName = homeTab.LocalizedTabName;

                    // Check if we should use the tab's title instead
                    if (useTitle && !string.IsNullOrEmpty(homeTab.Title))
                    {
                        homeTabName = homeTab.Title;
                    }
                }

                // Append all of the HTML for the root breadcrumb
                breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                breadcrumb.Append("<a href=\"" + homeUrl + "\" class=\"" + cssClass + "\" itemprop=\"item\" ><span itemprop=\"name\">" + homeTabName + "</span></a>");
                breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                breadcrumb.Append("</span>");

                // Add a separator
                breadcrumb.Append(separator);
            }

            //process bread crumbs
            for (var i = rootLevel; i < portalSettings.ActiveTab.BreadCrumbs.Count; ++i)
            {
                // Only add separators if we're past the root level
                if (i > rootLevel)
                {
                    breadcrumb.Append(separator);
                }

                // Grab the current tab
                var tab = (TabInfo)portalSettings.ActiveTab.BreadCrumbs[i];

                var tabName = tab.LocalizedTabName;

                // Determine if we should use the tab's title instead of tab name
                if (useTitle && !string.IsNullOrEmpty(tab.Title))
                {
                    tabName = tab.Title;
                }

                // Get the absolute URL of the tab
                var tabUrl = tab.FullUrl;

                // 
                if (ProfileUserId > -1)
                {
                    tabUrl = DotNetNuke.Common.Globals.NavigateURL(tab.TabID, "", "UserId=" + ProfileUserId);
                }

                // 
                if (GroupId > -1)
                {
                    tabUrl = DotNetNuke.Common.Globals.NavigateURL(tab.TabID, "", "GroupId=" + GroupId);
                }

                // Begin breadcrumb
                breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");

                // Is this tab disabled? If so, only render the text
                if (tab.DisableLink)
                {
                    breadcrumb.Append("<span class=\"" + cssClass + "\" itemprop=\"name\">" + tabName + "</span>");
                }
                else
                {
                    breadcrumb.Append("<a href=\"" + tabUrl + "\" class=\"" + cssClass + "\" itemprop=\"item\"><span itemprop=\"name\">" + tabName + "</span></a>");
                }

                breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />"); // Notice we post-increment the position variable
                breadcrumb.Append("</span>");
            }

            breadcrumb.Append("</span>"); //End of BreadcrumbList

            return breadcrumb.ToString();

        }

        private static int ProfileUserId
        {
            get
            {
                return string.IsNullOrEmpty(HttpContext.Current.Request.Params["UserId"])
                    ? Null.NullInteger
                    : int.Parse(HttpContext.Current.Request.Params["UserId"]);
            }
        }
        private static int GroupId
        {
            get
            {
                return string.IsNullOrEmpty(HttpContext.Current.Request.Params["GroupId"])
                    ? Null.NullInteger
                    : int.Parse(HttpContext.Current.Request.Params["GroupId"]);
            }
        }
    }


    public struct BreadCrumb
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}