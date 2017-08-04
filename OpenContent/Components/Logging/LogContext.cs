using System;
using System.Collections.Generic;
using System.Web;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Logging
{
    public class LogContext
    {
        private Dictionary<int, ModuleLogInfo> Logs { get; set; }

        private LogContext()
        {
            Logs = new Dictionary<int, ModuleLogInfo>();
        }

        //private static LogContext Context = null;
        public static LogContext Current
        {
            get
            {
                LogContext context;
                if (HttpContext.Current.Items.Contains("OpenContentLogs"))
                {
                    context = (LogContext)HttpContext.Current.Items["OpenContentLogs"];
                }
                else
                {
                    context = new LogContext();
                    HttpContext.Current.Items.Add("OpenContentLogs", context);
                }
                return context;
            }
        }
        public static bool IsLogActive
        {
            get
            {
                var ps = PortalSettings.Current;
                if (ps == null) return false;
                string openContentLogging = PortalController.GetPortalSetting("OpenContent_Logging", ps.PortalId, "none");
                return openContentLogging == "allways" || (openContentLogging == "host" && ps.UserInfo.IsSuperUser);
            }
        }
        public static void Log(int moduleId, string title, string label, object message)
        {
            if (IsLogActive)
            {
                ModuleLogInfo module;
                List<LogInfo> messages;
                if (Current.Logs.ContainsKey(moduleId))
                {
                    module = Current.Logs[moduleId];
                    if (module.Logs.ContainsKey(title))
                    {
                        messages = module.Logs[title];
                    }
                    else
                    {
                        messages = new List<LogInfo>();
                        module.Logs.Add(title, messages);
                    }
                }
                else
                {
                    module = new ModuleLogInfo();
                    Current.Logs.Add(moduleId, module);
                    messages = new List<LogInfo>();
                    module.Logs.Add(title, messages);
                }
                messages.Add(new LogInfo()
                {
                    Date = DateTime.Now,
                    Label = label,
                    Message = message
                });
            }
        }

        public Dictionary<string, List<LogInfo>> ModuleLogs(int moduleId)
        {
            if (Logs.ContainsKey(moduleId))
            {
                return Logs[moduleId].Logs;
            }
            return new Dictionary<string, List<LogInfo>>();
        }

    }
    public class ModuleLogInfo
    {
        public ModuleLogInfo()
        {
            Logs = new Dictionary<string, List<LogInfo>>();
        }
        [JsonProperty("logs")]
        public Dictionary<string, List<LogInfo>> Logs { get; private set; }
    }
    public class LogInfo
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("message")]
        public object Message { get; set; }

        public override string ToString()
        {
            return $"{Date} - {Label} - {Message}";
        }
    }
}