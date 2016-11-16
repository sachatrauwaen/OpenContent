using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Json;
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

        internal static JArray ToAdditionalDataArray(this IDataItems dataItems, int portalId, string currentCultureCode)
        {
            JArray jsonList = new JArray();
            foreach (var dataItem in dataItems.Items)
            {
                var data = dataItem.Data;
                if (data != null)
                {
                    if (LocaleController.Instance.GetLocales(portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(data, currentCultureCode);
                    }
                    data["Id"] = dataItem.Id; //add the contentItem Id to the json   //ContentId
                    jsonList.Add(data);
                }
            }
            return jsonList;
        }
    }
}