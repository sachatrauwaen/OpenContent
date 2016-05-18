using System;
using System.Web;
using System.Web.UI;

namespace Satrabel.OpenContent.Components.Logging
{
    public static class LoggingUtils
    {
        public static void ProcessModuleLoadException(Control ctrl, Exception exc)
        {
            string friendlyMessage = exc.Message;
            Exception lastExc = exc;
            while (lastExc.InnerException != null)
            {
                lastExc = exc.InnerException;
                friendlyMessage += "\n" + lastExc.Message;
            }
            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(friendlyMessage, ctrl, exc);
        }
        public static string HttpRequestLoggingInfo(HttpContext context)
        {
            string url = "-unknown-";
            string referrer = "-unknown-";
            if (context != null)
            {
                url = context.Request.UrlReferrer == null ? "???" : context.Request.UrlReferrer.AbsoluteUri;
                referrer = context.Request.UrlReferrer == null ? "???" : context.Request.UrlReferrer.AbsoluteUri;
            }
            string retval = string.Format("Called from {0}. Referrer: {1}.", url, referrer);

            return retval;
        }
    }
}