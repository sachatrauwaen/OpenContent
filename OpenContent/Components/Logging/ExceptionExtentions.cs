using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Logging
{
    public static class ExceptionExtentions
    {
        public static string MessageAsHtml(this Exception ex)
        {
            string friendlyMessage = ex.Message;
            Exception lastExc = ex;
            while (lastExc.InnerException != null)
            {
                lastExc = lastExc.InnerException;
                friendlyMessage += "<br/>" + lastExc.Message;
            }
            //FriendlyMessage += "<hr />";
            return friendlyMessage.Replace("\n", "<br />");
        }

        public static List<string> MessageAsList(this Exception ex)
        {
            List<string> lst = new List<string>();
            lst.Add(ex.Message);
            Exception lastExc = ex;
            while (lastExc.InnerException != null)
            {
                lastExc = lastExc.InnerException;
                lst.Add(lastExc.Message);
            }
            return lst;
        }
    }
}