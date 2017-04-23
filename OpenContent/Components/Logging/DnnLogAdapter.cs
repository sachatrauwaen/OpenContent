using System;
using System.Net.Http;
using System.Web;
using DotNetNuke.Instrumentation;

namespace Satrabel.OpenContent.Components.Logging
{

    public class DnnLogAdapter : ILogAdapter
    {

        private readonly ILog _dnnILog;

        #region Constructors

        private DnnLogAdapter(Type type)
        {
            _dnnILog = LoggerSource.Instance.GetLogger(type);
        }

        private DnnLogAdapter(string name)
        {
            _dnnILog = LoggerSource.Instance.GetLogger(name);
        }

        #endregion

        public static ILogAdapter GetLogAdapter(string name)
        {
            return new DnnLogAdapter(name);
        }

        public ILogAdapter GetLogAdapter(Type type)
        {
            return new DnnLogAdapter(type);
        }

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
                message = $"{message} {Environment.NewLine}Called from {HttpContext.Current.Request.RawUrl}.";
            return message;
        }
    }
}