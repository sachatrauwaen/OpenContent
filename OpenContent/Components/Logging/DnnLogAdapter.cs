using System;
using System.Diagnostics;
using System.Net.Http;
using System.Web;
using DotNetNuke.Instrumentation;

namespace Satrabel.OpenContent.Components.Logging
{

    public class DnnLogAdapter : ILogAdapter
    {

        private readonly ILog _dnnILog;

        #region Constructors

        public DnnLogAdapter(Type type)
        {
            _dnnILog = LoggerSource.Instance.GetLogger(type);
        }

        public DnnLogAdapter(string name)
        {
            _dnnILog = LoggerSource.Instance.GetLogger(name);
        }

        #endregion
       
        public void Error(string message)
        {
            _dnnILog.Error(message);
        }

        public void Error(Exception message)
        {
            _dnnILog.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            _dnnILog.Error(Enrich(message), exception);
        }

        public void Warn(string message)
        {
            _dnnILog.Warn(message);
        }

        public void Info(string message)
        {
            _dnnILog.Info(message);
        }

        public void Debug(string message)
        {
            _dnnILog.Debug(message);
        }

        public void Trace(string message)
        {
            _dnnILog.Trace(message);
        }

        public bool IsDebugEnabled => _dnnILog.IsDebugEnabled;

        private static string Enrich(string message)
        {
            if (HttpContext.Current?.Request != null)
                message = $"{message}{Environment.NewLine}{LoggingUtils.HttpRequestLogInfo(HttpContext.Current)}";
            return message;
        }

        public void LogServiceResult(HttpResponseMessage response, string responsemessage = "")
        {
            if (IsDebugEnabled)
            {
                StackTrace st = new StackTrace();

                string method = st.GetFrame(1).GetMethod().Name == "CreateResponse"
                    ? st.GetFrame(2).GetMethod().Name
                    : st.GetFrame(1).GetMethod().Name;

                var emp = string.IsNullOrEmpty(responsemessage) ? "<empty>" : responsemessage;
                Debug($"Result from '{method}' with status '{response.StatusCode}': {emp} \r\n");
            }
        }
    }
}