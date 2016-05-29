using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using DotNetNuke.UI.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Manifest;


namespace Satrabel.OpenContent.Components
{
    public static class AdditionalDataUtils
    {

        internal static string GetScope(string scopeType, int portalId, int tabId, int moduleId, int tabModuleId)
        {
            if (scopeType == "portal")
            {
                return scopeType + "/" + portalId;
            }
            else if (scopeType == "tab")
            {
                return scopeType + "/" + tabId;
            }
            else if (scopeType == "tabmodule")
            {
                return scopeType + "/" + tabModuleId;
            }
            else if (scopeType == "module")
            {
                return scopeType + "/" + moduleId;
            }
            else
            {
                return "module/" + moduleId;
            }
        }

        internal static string GetScope(AdditionalDataManifest manifest, int portalId, int tabId, int moduleId, int tabModuleId)
        {
            return AdditionalDataUtils.GetScope(manifest.ScopeType, portalId, tabId, moduleId, tabModuleId);
        }
    }
}