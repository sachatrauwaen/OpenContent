using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class OtherExtentions
    {
        public static string ToStringOrDefault(this DateTime? source, string format = "yyyy-MM-dd hh:mm:ss", string defaultValue = null)
        {
            if (source != null)
            {
                return source.Value.ToString(format);
            }
            return string.IsNullOrEmpty(defaultValue) ? string.Empty : defaultValue;
        }
        public static string CleanupUrl(this string url)
        {
            const string replaceWith = "-";

            const string accentFrom = "ÀÁÂÃÄÅàáâãäåảạăắằẳẵặấầẩẫậÒÓÔÕÖØòóôõöøỏõọồốổỗộơớờởợÈÉÊËèéêëẻẽẹếềểễệÌÍÎÏìíîïỉĩịÙÚÛÜùúûüủũụưứừửữựÿýỳỷỹỵÑñÇçĞğİıŞş₤€ßđ";
            const string accentTo = "AAAAAAaaaaaaaaaaaaaaaaaaaOOOOOOoooooooooooooooooooEEEEeeeeeeeeeeeeIIIIiiiiiiiUUUUuuuuuuuuuuuuuyyyyyyNnCcGgIiSsLEsd";

            url = url.ToLower().Trim();

            StringBuilder result = new StringBuilder(url.Length);
            int i = 0; int last = url.ToCharArray().GetUpperBound(0);
            foreach (char c in url)
            {

                //use string for manipulation
                var ch = c.ToString();
                if (ch == " ")
                {
                    ch = replaceWith;
                }
                else if (@".[]|:;`%\\""".Contains(ch))
                    ch = "";
                else if (@" &$+,/=?@~#<>()¿¡«»!'’–*…".Contains(ch))
                    ch = replaceWith;
                else
                {
                    for (int ii = 0; ii < accentFrom.Length; ii++)
                    {
                        if (ch == accentFrom[ii].ToString())
                        {
                            ch = accentTo[ii].ToString();
                        }
                    }
                }

                if (i == last)
                {
                    if (!(ch == "-" || ch == replaceWith))
                    {   //only append if not the same as the replacement character
                        result.Append(ch);
                    }
                }
                else
                    result.Append(ch);
                i++;//increment counter
            }
            string retval = result.ToString();
            if (!string.IsNullOrEmpty(replaceWith))
            {
                while (retval.Contains(replaceWith + replaceWith))
                {
                    retval = retval.Replace(replaceWith + replaceWith, replaceWith);
                }
                foreach (char c in replaceWith)
                {
                    retval = retval.Trim(c);
                }
            }

            return retval;
        }

    }
}