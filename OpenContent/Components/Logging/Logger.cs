using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
          
namespace Satrabel.OpenContent.Components
{
        public static class Log
        {
            [Obsolete("This method is obsolete since aug 2017; use App.Services.Logger instead")]
            public static ILogAdapter Logger => App.Services.Logger;
        }
}