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
        public static string TrimStart(this string txt, string value)
        {
            //remove any query parameters
            int qIndex = txt.IndexOf(value, StringComparison.Ordinal);
            if (qIndex == 0) txt = txt.Substring(value.Length);
            return txt;
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
                return HtmlRegex.Replace(source, string.Empty);
            }

            /// <summary>
            /// Remove HTML tags from string using char array.
            /// </summary>
            public static string StripTagsCharArray(string source)
            {
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

        #endregion

        public static string HtmlDecodeIfNeeded(string text)
        {
            if (text.Contains("&lt;") && text.Contains("&gt;"))
                text = HttpUtility.HtmlDecode(text);
            return text;
        }
    }
}