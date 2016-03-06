using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class MetaTags
    {

        #region PageHeader Helpers

        public static void SetPageTitle(HttpContextBase context, string title)
        {
            if (context == null) return;
            title = Utils.HtmlDecodeIfNeeded(title);
            title = Utils.HtmlRemoval.StripTagsRegexCompiled(title);
            if (string.IsNullOrWhiteSpace(title)) return;
            var dnnpage = context.DnnPage();
            if (dnnpage != null)
            {
                dnnpage.Header.Title = title;
                dnnpage.Title = title;
            }
        }

        /// <summary>
        /// Sets the page description. Works from Razor too.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="description">The description.</param>
        public static void SetPageDescription(HttpContextBase context, string description)
        {
            if (context == null) return;
            description = Utils.HtmlDecodeIfNeeded(description);
            description = Utils.HtmlRemoval.StripTagsRegexCompiled(description);
            if (string.IsNullOrWhiteSpace(description)) return;
            var dnnpage = context.DnnPage();
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

        public static void SetPageKeywords(HttpContextBase context, string keywords)
        {
            if (context == null) return;
            if (string.IsNullOrWhiteSpace(keywords)) return;
            var dnnpage = context.DnnPage();
            if (dnnpage != null)
            {
                dnnpage.Header.Keywords = keywords;
            }
        }
        #endregion

        #region Private Methods

        private static DotNetNuke.Framework.CDefault DnnPage(this HttpContextBase context)
        {
            var pageObj = context.CurrentHandler as DotNetNuke.Framework.CDefault;
            return pageObj;
        }

        #endregion
    }
}