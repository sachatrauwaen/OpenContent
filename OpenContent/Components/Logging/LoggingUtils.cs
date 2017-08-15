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
                friendlyMessage = $"Alias: unknown \nTab: unknown - {DnnUrlUtils.NavigateUrl(renderEngine.ModuleConfig.TabId)} \nModule: {renderEngine.ModuleConfig.ModuleId} \nContext: {HttpRequestLogInfo(HttpContext.Current)} \nError: {exc.Message}";
            }
            else
            {
                friendlyMessage = $"Alias: {ps.PortalAlias.HTTPAlias} \nTab: {ps.ActiveTab.TabID} - {DnnUrlUtils.NavigateUrl(ps.ActiveTab.TabID)} \nModule: {renderEngine.ModuleConfig.ModuleId} \nContext: {HttpRequestLogInfo(HttpContext.Current)} \nError: {exc.Message}";
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
            var friendlyMessage = $"Alias: {ps.PortalAlias.HTTPAlias} \nTab: {ps.ActiveTab.TabID} - {DnnUrlUtils.NavigateUrl(ps.ActiveTab.TabID)} \nModule: {module.ModuleID} \nContext: {LoggingUtils.HttpRequestLogInfo(HttpContext.Current)} \nError: {exc.Message}";
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
            string friendlyMessage = $"\n{exc.Message} \n\nAlias: {ctrl.ModuleContext.PortalAlias.HTTPAlias} \nTab: {ctrl.ModuleContext.TabId} - {DnnUrlUtils.NavigateUrl(ctrl.ModuleContext.TabId)} \nModule: {ctrl.ModuleContext.ModuleId} \n{LoggingUtils.HttpRequestLogInfo(HttpContext.Current)} ";
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
            string friendlyMessage;
            if (ctrl?.ActiveModule == null)
            {
                friendlyMessage = $"\n{exc.Message} \n{LoggingUtils.HttpRequestLogInfo(HttpContext.Current)} ";
            }
            else
            {
                friendlyMessage = $"\n{exc.Message} \n\n PortalId: {ctrl.ActiveModule.PortalID} \nTab: {ctrl.ActiveModule.TabID} - {DnnUrlUtils.NavigateUrl(ctrl.ActiveModule.TabID)} \nModule: {ctrl.ActiveModule.ModuleID} \n{LoggingUtils.HttpRequestLogInfo(HttpContext.Current)} ";
            }
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
            string ip = "-unknown-";
            if (context != null)
            {
                url = context.Request.Url.AbsoluteUri;
                referrer = context.Request.UrlReferrer == null ? "-unknown-" : context.Request.UrlReferrer.AbsoluteUri;
                ip = context.Request.UserHostAddress;
            }
            string retval = $"Called from {url}. Referred by {referrer}. Via IP {ip}";

            return retval;
        }
    }
}