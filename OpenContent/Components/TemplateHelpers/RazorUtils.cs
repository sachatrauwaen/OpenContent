using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class RazorUtils
    {
        /// <summary>
        /// Helper to quick and easy add a Debug Break in your Razor files, a point from whereon you can start debugging with Visual Studio
        /// </summary>
        public static void Break()
        {
            Utils.DebuggerBreak();
        }

        /// <summary>
        /// Helper method to obfusticates an email address but keeping the mailto functionality.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="subject">The subject. (optional, default is string.Empty())</param>
        /// <param name="visibleText">The visible text. (optional, default is the Email Address itself)</param>
        /// <returns></returns>
        /// <example>
        /// ProtectEmail(info@domain.com)
        /// ProtectEmail(info@domain.com, "Website question")
        /// ProtectEmail(info@domain.com, "Website question", "Send an email")
        /// </example>
        public static string ProtectEmail(this string email, string subject = "", string visibleText = "")
        {
            if (string.IsNullOrEmpty(email)) return "";

            var onload = "";

            var dataAttribs = "data-contact='" + Convert.ToBase64String(Encoding.UTF8.GetBytes(email)) + "'";
            if (string.IsNullOrEmpty(visibleText))
            {
                onload = $"<img src onerror='this.outerHTML = atob(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(email))}\")'>"; //nice hack to mimic an onload event
            }
            var onfocus = "this.href='mailto:'+atob(this.dataset.contact)";
            if (!string.IsNullOrEmpty(subject))
            {
                dataAttribs = dataAttribs + " data-subj='" + Convert.ToBase64String(Encoding.UTF8.GetBytes(subject)) + "'";
                onfocus = onfocus + "+'?subject=' + atob(this.dataset.subj || '')";
            }
            var result = $"<a href='#' {dataAttribs} onfocus=\"{onfocus}\">{visibleText}{onload}</a>";
            return result;
        }

        /// <summary>
        /// Formats the date time, just like the HandleBarHelper formatDateTime does.
        /// </summary>
        /// <param name="isoDateTime">The iso date time.</param>
        /// <param name="format">The date format.</param>
        /// <param name="culture">The culture.</param>
        public static string FormatDateTime(string isoDateTime, string format = "dd/MM/yyyy", string culture = "invariant")
        {
            try
            {
                var datetime = DateTime.Parse(isoDateTime, null, DateTimeStyles.RoundtripKind);
                var formatprovider = culture == "invariant" ? CultureInfo.InvariantCulture : CultureInfo.CreateSpecificCulture(culture);
                var res = datetime.ToString(format, formatprovider);

                return res;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}