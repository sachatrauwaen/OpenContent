using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class StringExtentions
    {
        public static string ToStringOrDefault(this DateTime? source, string format = "yyyy-MM-dd hh:mm:ss", string defaultValue = null)
        {
            if (source != null)
            {
                return source.Value.ToString(format);
            }
            return string.IsNullOrEmpty(defaultValue) ? string.Empty : defaultValue;
        }

        public static string SubstringBetween(this string source, string start, string end)
        {
            var p1 = source.ToLowerInvariant().IndexOf(start.ToLowerInvariant()); //position of start in the source
            var p2 = source.ToLowerInvariant().IndexOf(end.ToLowerInvariant()); //position of end in the source

            if (p1 < 0 && p2 < 0) return ""; //none where found
            if (p1 >= 0 && p2 < 0) return source.Substring(p1 + start.Length);
            if (p1 < 0 && p2 >= 0) return source.Substring(0, p2);
            return source.Substring(p1 + start.Length, p2 - p1 - start.Length);
        }

        public static string CaseIncensistiveReplace(this string source, string search, string replaceWith)
        {
            if (source == null) return null;
            if (search == null) return source;

            var pos = source.ToLowerInvariant().IndexOf(search.ToLowerInvariant());
            if (pos < 0) return source;

            var word = source.Substring(pos, search.Length);
            return source.Replace(word, replaceWith);
        }

        public static bool EqualsAny<T>(this T x, params T[] args)
        {
            return ((IList) args).Contains(x);
        }
        public static bool ContainsAny<T>(this List<T> x, params T[] args)
        {
            return args.Any(item => x.Contains(item));
        }
    }
}