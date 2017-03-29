using System;
using Satrabel.OpenContent.Components.Manifest;


namespace Satrabel.OpenContent.Components
{
    public static class AdditionalDataUtils
    {

        internal static string GetScope(string scopeType, int portalId, int tabId, int moduleId, int tabModuleId)
        {
            switch (scopeType)
            {
                case "portal":
                    if (portalId < 0) throw new ArgumentException("portalId should not be < 0");
                    return scopeType + "/" + portalId;
                case "tab":
                    if (tabId < 0) throw new ArgumentException("tabId should not be < 0");
                    return scopeType + "/" + tabId;
                case "tabmodule":
                    if (tabModuleId < 0) throw new ArgumentException("tabModuleId should not be < 0");
                    return scopeType + "/" + tabModuleId;
                case "module":
                    if (moduleId < 0) throw new ArgumentException("moduleId should not be < 0");
                    return scopeType + "/" + moduleId;
                default:
                    if (moduleId < 0) throw new ArgumentException("moduleId should not be < 0");
                    return "module/" + moduleId;
            }
        }

        internal static string GetScope(AdditionalDataManifest manifest, int portalId, int tabId, int moduleId, int tabModuleId)
        {
            return AdditionalDataUtils.GetScope(manifest.ScopeType, portalId, tabId, moduleId, tabModuleId);
        }

    }
}