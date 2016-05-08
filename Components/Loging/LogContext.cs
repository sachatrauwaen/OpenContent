using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Loging
{
    public class LogContext
    {
        private LogContext(){
            Logs = new Dictionary<string, List<LogInfo>>(); 
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
        public Dictionary<string, List<LogInfo>> Logs { get; private set; }

        public static void Log(string key, string label, object message)
        {
            List<LogInfo> messages;
            if (Curent.Logs.ContainsKey(key))
            {
                messages = Curent.Logs[key];
            }
            else
            {
                messages = new List<LogInfo>();
                Curent.Logs.Add(key ,messages);
            }
            messages.Add(new LogInfo() { 
                Date = DateTime.Now,
                Label = label,
                Message = message
            });
        }

    }
    public class LogInfo
    {
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public object Message { get; set; }
    }
}