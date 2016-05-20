using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.UrlRewriter
{
    public class OpenContentUrlRule
    {
        public string CultureCode { get; set; }
        public bool InSitemap { get; set; }
        public string Parameters { get; set; }
        public string RedirectDestination { get; set; }
        public int RedirectStatus { get; set; }
        public bool RemoveTab { get; set; }
        public int TabId { get; set; }
        public string Url { get; set; }
    }
}