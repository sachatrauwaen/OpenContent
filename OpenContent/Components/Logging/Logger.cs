using System.Diagnostics;
using System.Net.Http;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Utility class containing several commonly used procedures by Stefan Kamphuis
    /// </summary>
    public static class Log
    {
        public static ILogAdapter Logger => App.Services.LogAdapter;

        public static void LogServiceResult(HttpResponseMessage response, string responsemessage = "")
        {
            if (Logger.IsDebugEnabled)
            {
                StackTrace st = new StackTrace();

                string method = st.GetFrame(1).GetMethod().Name == "CreateResponse"
                    ? st.GetFrame(2).GetMethod().Name
                    : st.GetFrame(1).GetMethod().Name;

                var emp = string.IsNullOrEmpty(responsemessage) ? "<empty>" : responsemessage;
                Logger.Debug($"Result from '{method}' with status '{response.StatusCode}': {emp} \r\n");
            }
        }
    }
}