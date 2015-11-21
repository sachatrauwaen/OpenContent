using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public static class Utils
    {
        #region Url utils

        public static string RemoveQueryParams(this string url)
        {
            //remove any query parameters
            int qIndex = url.IndexOf('?');
            if (qIndex >= 0) url = url.Remove(qIndex);
            return url;
        }
        public static string TrimStart(this string txt, string value)
        {
            //remove any query parameters
            int qIndex = txt.IndexOf(value, StringComparison.Ordinal);
            if (qIndex == 0) txt = txt.Substring(value.Length);
            return txt;
        }

        #endregion
    }
}