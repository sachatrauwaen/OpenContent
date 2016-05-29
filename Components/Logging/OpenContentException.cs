using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Logging
{
    public class OpenContentException : Exception
    {
        public OpenContentException(string message, Exception innerException) : base(message, innerException)
        {

        }
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
                return FriendlyMessage.Replace("\n", "<br />");
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