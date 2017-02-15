using System;

namespace Satrabel.OpenContent.Components.Logging
{
    public class TemplateException : Exception
    {

        public TemplateException(string message, Exception innerException, dynamic templateModel, string templateSource)
            : base(message, innerException)
        {
            TemplateModel = templateModel;
            TemplateSource = templateSource;
        }
        public dynamic TemplateModel { get; private set; }
        public string TemplateSource { get; private set; }


    }
}