using System;
using System.Web;
using DotNetNuke.Web.Api;
using Satrabel.OpenContent.Components.Dnn;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using System.Web.UI;
using Satrabel.OpenContent.Components.Render;

namespace Satrabel.OpenContent.Components.Logging
{
    public static class LoggingUtils
    {
        private static string PrepareErrorMessage(RenderEngine renderEngine, Exception exc)
        {
            var ps = PortalSettings.Current;
            string friendlyMessage;
            if (ps == null)
            {
                friendlyMessage = string.Format("Alias: {3} \nTab: {4} - {5} \nModule: {0} \nContext: {2} \nError: {1}",
                    renderEngine.ModuleContext.ModuleId,
                    exc.Message,
                    LoggingUtils.HttpRequestLogInfo(HttpContext.Current),
                    "unknown",
                    "unknown",
                    DnnUrlUtils.NavigateUrl(renderEngine.ModuleContext.TabId)
                );
            }
            else
            {
                friendlyMessage = string.Format("Alias: {3} \nTab: {4} - {5} \nModule: {0} \nContext: {2} \nError: {1}",
                   renderEngine.ModuleContext.ModuleId,
                   exc.Message,
                   LoggingUtils.HttpRequestLogInfo(HttpContext.Current),
                   ps.PortalAlias.HTTPAlias,
                   ps.ActiveTab.TabID,
                   DnnUrlUtils.NavigateUrl(ps.ActiveTab.TabID)
                   );
            }
            Exception lastExc = exc;
            while (lastExc.InnerException != null)
            {
                lastExc = lastExc.InnerException;
                friendlyMessage += "\n" + lastExc.Message;
            }
            return friendlyMessage;
        }



        private static string PrepareErrorMessage(ModuleInfo module, Exception exc)
        {
            var ps = PortalSettings.Current;
            string friendlyMessage = string.Format("Alias: {3} \nTab: {4} - {5} \nModule: {0} \nContext: {2} \nError: {1}",
                module.ModuleID,
                exc.Message,
                LoggingUtils.HttpRequestLogInfo(HttpContext.Current),
                ps.PortalAlias.HTTPAlias,
                ps.ActiveTab.TabID,
                DnnUrlUtils.NavigateUrl(ps.ActiveTab.TabID)
                );
            Exception lastExc = exc;
            while (lastExc.InnerException != null)
            {
                lastExc = lastExc.InnerException;
                friendlyMessage += "\n" + lastExc.Message;
            }
            return friendlyMessage;
        }
        private static string PrepareErrorMessage(DotNetNuke.Web.Razor.RazorModuleBase ctrl, Exception exc)
        {
            string friendlyMessage = string.Format("\n{1} \n\nAlias: {3} \nTab: {4} - {5} \nModule: {0} \n{2} ",
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
                lastExc = lastExc.InnerException;
                friendlyMessage += "\n" + lastExc.Message;
            }
            return friendlyMessage;
        }
        private static string PrepareErrorMessage(DnnApiController ctrl, Exception exc)
        {
            string friendlyMessage = string.Format("\n{1} \n\n PortalId: {3} \nTab: {4} - {5} \nModule: {0} \n{2} ",
                ctrl.ActiveModule.ModuleID,
                exc.Message,
                LoggingUtils.HttpRequestLogInfo(HttpContext.Current),
                ctrl.ActiveModule.PortalID,
                ctrl.ActiveModule.TabID,
                DnnUrlUtils.NavigateUrl(ctrl.ActiveModule.TabID)
                );
            Exception lastExc = exc;
            while (lastExc.InnerException != null)
            {
                lastExc = lastExc.InnerException;
                friendlyMessage += "\n" + lastExc.Message;
            }
            return friendlyMessage;
        }

        public static void ProcessLogFileException(DotNetNuke.Web.Razor.RazorModuleBase ctrl, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(ctrl, exc);
            App.Services.Logger.Error(friendlyMessage);
        }
        public static void ProcessLogFileException(Control ctrl, ModuleInfo module, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(module, exc);
            App.Services.Logger.Error(friendlyMessage);
        }

        public static void ProcessApiLoadException(DnnApiController ctrl, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(ctrl, exc);
            App.Services.Logger.Error(friendlyMessage);
        }
        public static void RenderEngineException(RenderEngine ctrl, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(ctrl, exc);
            App.Services.Logger.Error(friendlyMessage);
        }

        public static void ProcessModuleLoadException(DotNetNuke.Web.Razor.RazorModuleBase ctrl, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(ctrl, exc);
            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(friendlyMessage.Replace("\n", "<br />"), ctrl, exc);
        }
        public static void ProcessModuleLoadException(Control ctrl, ModuleInfo module, Exception exc)
        {
            string friendlyMessage = PrepareErrorMessage(module, exc);
            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(friendlyMessage, ctrl, exc);
        }
        public static string HttpRequestLogInfo(HttpContext context)
        {
            string url = "-unknown-";
            string referrer = "-unknown-";
            if (context != null)
            {
                url = context.Request.Url.AbsoluteUri;
                referrer = context.Request.UrlReferrer == null ? "" : "\nReferrer: " + context.Request.UrlReferrer.AbsoluteUri;
            }
            string retval = $"Called from {url}. {referrer}.";

            return retval;
        }
    }
}