using System;
using System.Diagnostics;
using System.Net.Http;
using ClientDependency.Core;
using DotNetNuke.Instrumentation;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Utility class containing several commonly used procedures by Stefan Kamphuis
    /// </summary>
    public static class Log
    {
        public static ILog Logger
        {
            get
            {
                return LoggerSource.Instance.GetLogger("OpenContent");
            }
        }

        public static void LogServiceResult(HttpResponseMessage response, string responsemessage = "")
        {
            if (Logger.IsDebugEnabled)
            {
                StackTrace st = new StackTrace();

                string method = st.GetFrame(1).GetMethod().Name == "CreateResponse"
                    ? st.GetFrame(2).GetMethod().Name
                    : st.GetFrame(1).GetMethod().Name;

                Logger.DebugFormat("Result from '{0}' with status '{1}': {2} \r\n", method, response.StatusCode.ToString(), String.IsNullOrEmpty(responsemessage) ? "<empty>" : responsemessage);
            }
        }
    }
}