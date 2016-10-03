using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Layout
{
    public class LayoutUtils
    {
        public static FolderUri GetDefaultFolder()
        {
            return new FolderUri(PortalSettings.Current.HomeDirectory + AppConfig.OPENCONTENT + "/Layouts/_default");
        }
    }
}