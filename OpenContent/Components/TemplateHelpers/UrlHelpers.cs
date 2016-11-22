using System.Linq;
using System.Text;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class UrlHelpers
    {
        public static string NavigateUrl(int targetTabId, string detailItemId, string detailItemTitle, params string[] additionalParameters)
        {
            if (string.IsNullOrEmpty(detailItemTitle)) return null;
            if (targetTabId == 0) return null;

            detailItemTitle = detailItemTitle.CleanupUrl();
            string[] param = { "id", detailItemId };
            param = param.Concat(additionalParameters).ToArray();
            var newUrl = TestableGlobals.Instance.NavigateURL(targetTabId, false, PortalSettings.Current, string.Empty, string.Empty, detailItemTitle, param);
            return newUrl.Length <= 230 ? newUrl : newUrl.Substring(0, 230); //actual url has ?default.aspx 
        }

        public static string NavigateFileUrl(dynamic fileId)
        {
            var portalFileUri = UriFactory.CreatePortalFileUri(fileId);
            if (portalFileUri == null)
            {
                return string.Empty;
            }
            return portalFileUri.UrlFilePath;
        }

        public static string CleanupUrl(this string text)
        {
            const string replaceWith = "-";

            const string accentFrom = "ÀÁÂÃÄÅàáâãäåảạăắằẳẵặấầẩẫậÒÓÔÕÖØòóôõöøỏõọồốổỗộơớờởợÈÉÊËèéêëẻẽẹếềểễệÌÍÎÏìíîïỉĩịÙÚÛÜùúûüủũụưứừửữựÿýỳỷỹỵÑñÇçĞğİıŞş₤€ßđ";
            const string accentTo = "AAAAAAaaaaaaaaaaaaaaaaaaaOOOOOOoooooooooooooooooooEEEEeeeeeeeeeeeeIIIIiiiiiiiUUUUuuuuuuuuuuuuuyyyyyyNnCcGgIiSsLEsd";

            text = text.ToLower().Trim();

            StringBuilder result = new StringBuilder(text.Length);
            int i = 0; int last = text.ToCharArray().GetUpperBound(0);
            foreach (char c in text)
            {

                //use string for manipulation
                var ch = c.ToString();
                if (ch == " ")
                {
                    ch = replaceWith;
                }
                else if (@".[]|:;`%\\""^№".Contains(ch))
                    ch = "";
                else if (@" &$+,/=?@~#<>(){}¿¡«»!'’–*…“”".Contains(ch))
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