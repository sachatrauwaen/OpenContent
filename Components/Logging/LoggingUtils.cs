using System;
using System.Web;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Web.Razor.Helpers;
using Satrabel.OpenContent.Components.Dnn;

namespace Satrabel.OpenContent.Components.Logging
{
    public static class LoggingUtils
    {
        private static string PrepareErrorMessage(DotNetNuke.Web.Razor.RazorModuleBase ctrl, Exception exc)
        {
            string friendlyMessage = string.Format("Alias: {3} \nTab: {4} - {5} \nModule: {0} \nContext: {2} \nError: {1}",
                ctrl.ModuleContext.ModuleId,
                exc.Message,
                LoggingUtils.HttpRequestLogInfo(HttpContext.Current),
                ctrl.ModuleContext.PortalAlias.HTTPAlias,
                ctrl.ModuleContext.TabId,
                DnnUrlUtils.NavigateUrl(ctrl.ModuleContext.TabId)
                );
            Exception lastExc = exc;
            while (lastExc.InnerException != null)
            {
                lastExc = exc.InnerException;
                friendlyMessage += "\n" + lastExc.Message;
            }
            return friendlyMessage;
        }
        public static void ProcessLogFileException(DotNetNuke.Web.Razor.RazorModuleBase ctrl, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(ctrl, exc);
            Log.Logger.Error(friendlyMessage);
        }
        public static void ProcessModuleLoadException(DotNetNuke.Web.Razor.RazorModuleBase ctrl, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(ctrl, exc);
            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(friendlyMessage, ctrl, exc);
        }
        public static string HttpRequestLogInfo(HttpContext context)
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