using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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


        #endregion

        #region String Utils

        /// <summary>
        /// Methods to remove HTML from strings.
        /// http://www.dotnetperls.com/remove-html-tags
        /// </summary>
        internal static class HtmlRemoval
        {
            /// <summary>
            /// Remove HTML from string with Regex.
            /// </summary>
            public static string StripTagsRegex(string source)
            {
                if (string.IsNullOrEmpty(source)) return source;
                return Regex.Replace(source, "<.*?>", string.Empty);
            }

            /// <summary>
            /// Compiled regular expression for performance.
            /// </summary>
            private static readonly Regex HtmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

            /// <summary>
            /// Remove HTML from string with compiled Regex.
            /// </summary>
            public static string StripTagsRegexCompiled(string source)
            {
                if (string.IsNullOrEmpty(source)) return source;
                return HtmlRegex.Replace(source, string.Empty);
            }

            /// <summary>
            /// Remove HTML tags from string using char array.
            /// </summary>
            public static string StripTagsCharArray(string source)
            {
                if (string.IsNullOrEmpty(source)) return source;
                char[] array = new char[source.Length];
                int arrayIndex = 0;
                bool inside = false;

                for (int i = 0; i < source.Length; i++)
                {
                    char let = source[i];
                    if (let == '<')
                    {
                        inside = true;
                        continue;
                    }
                    if (let == '>')
                    {
                        inside = false;
                        continue;
                    }
                    if (!inside)
                    {
                        array[arrayIndex] = let;
                        arrayIndex++;
                    }
                }
                return new string(array, 0, arrayIndex);
            }
        }

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
        #endregion

        public static string HtmlDecodeIfNeeded(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Contains("&lt;") && text.Contains("&gt;"))
                text = HttpUtility.HtmlDecode(text);
            return text;
        }
    }
}