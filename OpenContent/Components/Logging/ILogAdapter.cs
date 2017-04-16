using System;

namespace Satrabel.OpenContent.Components
{
    public interface ILogAdapter
    {
        ILogAdapter GetLogAdapter(Type type);
        void Error(string message);
        void Error(Exception message);
        void Error(string message, Exception exception);
        void Warn(string message);
        void Info(string message);
        void Debug(string message);
        void Trace(string message);
        bool IsDebugEnabled { get; }
    }
}