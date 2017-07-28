using System;
using System.Text;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class RazorUtils
    {
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
    }
}