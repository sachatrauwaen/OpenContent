using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Logging
{
    using DotNetNuke.Instrumentation;

    namespace Satrabel.OpenFiles.Components
    {
        public static class Log
        {
            [Obsolete("This method is obsolete since aug 2017; use App.Services.Logger instead")]
            public static ILog Logger
            {
                get
                {
                    return LoggerSource.Instance.GetLogger(App.Config.Opencontent);
                }
            }
        }
    }
}