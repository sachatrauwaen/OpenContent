using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Loging
{
    public class TemplateException : Exception
    {
        public TemplateException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
        public TemplateException(string message, Exception innerException, dynamic templateModel, string templateSource)
            : base(message, innerException)
        {
            TemplateModel = templateModel;
            TemplateSource = templateSource;
        }
        public dynamic TemplateModel { get; private set; }
        public string TemplateSource { get; private set; }

        public string MessageAsHtml
        {
            get
            {
                string FriendlyMessage = this.Message;
                Exception lastExc = this;
                while (lastExc.InnerException != null)
                {
                    lastExc = lastExc.InnerException;
                    FriendlyMessage += "<br/>" + lastExc.Message;
                }
                //FriendlyMessage += "<hr />";
                return FriendlyMessage;
            }
        }
        public List<string> MessageAsList
        {
            get
            {
                List<string> lst = new List<string>();
                lst.Add(this.Message);
                Exception lastExc = this;
                while (lastExc.InnerException != null)
                {
                    lastExc = lastExc.InnerException;
                    lst.Add(lastExc.Message);
                }
                return lst;
            }
        }
    }
}