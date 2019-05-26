using System;
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

        public static string GetFullUrl(HttpRequest request, string relativeUrl)
        {
            return String.Format("{0}://{1}{2}",
                            request.Url.Scheme,
                            request.Url.Authority,
                            VirtualPathUtility.ToAbsolute(relativeUrl));
        }

        #endregion

        #region String Utils

        /// <summary>
        /// Returns a string starting from the (last occurence of the) value provided until the end.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string TrimStart(this string text, string value)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int qIndex = text.IndexOf(value, StringComparison.Ordinal);
            if (qIndex == 0) text = text.Substring(value.Length);
            return text;
        }

        /// <summary>
        /// Removes the end of the string, starting from the (last occurence of the) value provided until the end.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns></returns>
        public static string TrimEnd(this string text, string value)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int qIndex = text.LastIndexOf(value, StringComparison.Ordinal);
            if (qIndex != -1) text = text.Substring(0, qIndex);
            return text;
        }

        public static DateTime TicksToDateTime(this string ticks)
        {
            if (string.IsNullOrEmpty(ticks)) return DateTime.MinValue;
            return new DateTime(long.Parse(ticks));
        }
        public static DateTime TicksToDateTime(this long ticks)
        {
            return new DateTime(ticks);
        }

        #endregion

       
    }
}