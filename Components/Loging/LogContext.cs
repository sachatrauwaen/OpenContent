using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Loging
{
    public class LogContext
    {
        public Dictionary<int, ModuleLogInfo> Logs { get; private set; }
        private LogContext()
        {
            Logs = new Dictionary<int, ModuleLogInfo>();
        }

        //private static LogContext Context = null;
        public static LogContext Curent
        {
            get
            {
                LogContext Context;
                if (HttpContext.Current.Items.Contains("OpenContentLogs"))
                {
                    Context = (LogContext)HttpContext.Current.Items["OpenContentLogs"];
                }
                else
                {
                    Context = new LogContext();
                    HttpContext.Current.Items.Add("OpenContentLogs", Context);
                }
                return Context;
            }
        }
        public static bool IsLogActive
        {
            get
            {
                var ps = PortalSettings.Current;
                string OpenContent_Logging = PortalController.GetPortalSetting("OpenContent_Logging", ps.PortalId, "none");
                return OpenContent_Logging == "allways" || (OpenContent_Logging == "host" && ps.UserInfo.IsSuperUser);
            }
        }
        public static void Log(int moduleId, string key, string label, object message)
        {
            {
                ModuleLogInfo module;
                List<LogInfo> messages;
                if (Curent.Logs.ContainsKey(moduleId))
                {
                    module = Curent.Logs[moduleId];
                    if (module.Logs.ContainsKey(key))
                    {
                        messages = module.Logs[key];
                    }
                    else
                    {
                        messages = new List<LogInfo>();
                        module.Logs.Add(key, messages);
                    }
                }
                else
                {
                    module = new ModuleLogInfo();
                    Curent.Logs.Add(moduleId, module);
                    messages = new List<LogInfo>();
                    module.Logs.Add(key, messages);
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
    }
}