using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Satrabel.OpenContent.Components.Loging
{
    public class ExceptionUtils
    {
        public static void ProcessModuleLoadException(Control ctrl, Exception exc)
        {
            string FriendlyMessage = exc.Message;
            Exception lastExc = exc;
            while (lastExc.InnerException != null)
            {
                lastExc = exc.InnerException;
                FriendlyMessage += "\n" + lastExc.Message;
            }
            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(FriendlyMessage, ctrl, exc);
        }
    }
}