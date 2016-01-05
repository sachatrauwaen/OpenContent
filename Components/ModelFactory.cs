using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Manifest;

namespace Satrabel.OpenContent.Components
{
    public class ModelFactory
    {
        readonly string dataJson;
        readonly string settingsJson;
        readonly string PhysicalTemplateFolder;
        readonly Manifest.Manifest Manifest;
        readonly TemplateManifest TemplateManifest;
        readonly TemplateFiles ManifestFiles;
        readonly ModuleInfo Module;
        readonly PortalSettings PortalSettings;
        readonly IEnumerable<OpenContentInfo> DataList = null;
        readonly int MainTabId;

        public ModelFactory(string dataJson, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles manifestFiles, ModuleInfo module, PortalSettings portalSettings, int MainTabId)
        {
            this.dataJson = dataJson;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.ManifestFiles = manifestFiles;
            this.Module = module;
            this.PortalSettings = portalSettings;
            this.TemplateManifest = templateManifest;
            this.MainTabId = MainTabId > 0 ? MainTabId : module.TabID;
        }
        public ModelFactory(IEnumerable<OpenContentInfo> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles manifestFiles, ModuleInfo module, PortalSettings portalSettings, int MainTabId)
        {
            this.DataList = dataList;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.ManifestFiles = manifestFiles;
            this.Module = module;
            this.PortalSettings = portalSettings;
            this.TemplateManifest = templateManifest;
            this.MainTabId = MainTabId > 0 ? MainTabId : module.TabID;
        }
        public dynamic GetModelAsDynamic()
        {
            if (DataList == null)
            {
                return GetModelAsDynamicFromJson();
            }
            else
            {
                return GetModelAsDynamicFromList();
            }
        }

        public JToken GetModelAsJson(bool onlyData = false)
        {
            if (DataList == null)
            {
                return GetModelAsJsonFromJson();
            }
            else
            {
                return GetModelAsJsonFromList(onlyData);
            }
        }

        private JToken GetModelAsJsonFromList(bool onlyData)
        {
            JObject model = new JObject();
            JArray items = new JArray(); ;
            model["Items"] = items;
            string editRole = Manifest == null ? "" : Manifest.EditRole;
            if (DataList != null && DataList.Any())
            {
                foreach (var item in DataList)
                {
                    string itemDataJson = item.Json;
                    if (LocaleController.Instance.GetLocales(PortalSettings.PortalId).Count > 1)
                    {
                        itemDataJson = JsonUtils.SimplifyJson(itemDataJson, DnnUtils.GetCurrentCultureCode());
                    }
                    JObject dyn = JObject.Parse(itemDataJson);
                    string url = "";
                    if (Manifest != null && !string.IsNullOrEmpty(Manifest.DetailUrl))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        url = OpenContentUtils.CleanupUrl(hbEngine.Execute(Manifest.DetailUrl, dyn));
                    }

                    JObject context = new JObject();
                    dyn["Context"] = context;

                    context["Id"] = item.ContentId;
                    context["EditUrl"] = EditUrl("id", item.ContentId.ToString());
                    context["IsEditable"] = IsEditable ||
                        (!string.IsNullOrEmpty(editRole) &&
                        OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, item.CreatedByUserId));
                    context["DetailUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), dyn["Title"] == null ? "" : OpenContentUtils.CleanupUrl(url/*dyn["Title"].ToString()*/), "id=" + item.ContentId.ToString());
                    context["MainUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), "");
                    items.Add(dyn);
                }
            }
            if (!onlyData)
            {
                CompleteModel(model);
                model["Context"]["RssUrl"] = PortalSettings.PortalAlias.HTTPAlias +
                       "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + Module.ModuleID + "&tabId=" + MainTabId;

            }
            return model;
        }

        private JToken GetModelAsJsonFromJson()
        {
            throw new NotImplementedException(); //todo
        }
        private dynamic GetModelAsDynamicFromList()
        {
            dynamic model = new ExpandoObject();
            model.Items = new List<dynamic>();
            string editRole = Manifest == null ? "" : Manifest.EditRole;
            if (DataList != null && DataList.Any())
            {
                foreach (var item in DataList)
                {
                    string dataJson = item.Json;
                    if (LocaleController.Instance.GetLocales(PortalSettings.PortalId).Count > 1)
                    {
                        dataJson = JsonUtils.SimplifyJson(dataJson, DnnUtils.GetCurrentCultureCode());
                    }
                    dynamic dyn = JsonUtils.JsonToDynamic(dataJson);
                    string url = "";
                    try
                    {
                        if (Manifest != null && !string.IsNullOrEmpty(Manifest.DetailUrl))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            url = OpenContentUtils.CleanupUrl(hbEngine.Execute(Manifest.DetailUrl, dyn));
                        }
                        //title = OpenContentUtils.CleanupUrl(dyn.Title);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    dyn.Context = new ExpandoObject();
                    dyn.Context.Id = item.ContentId;
                    dyn.Context.EditUrl = EditUrl("id", item.ContentId.ToString());
                    dyn.Context.IsEditable = IsEditable ||
                        (!string.IsNullOrEmpty(editRole) &&
                        OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, item.CreatedByUserId));
                    dyn.Context.DetailUrl = Globals.NavigateURL(MainTabId, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), url, "id=" + item.ContentId.ToString());
                    dyn.Context.MainUrl = Globals.NavigateURL(MainTabId, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), "");
                    model.Items.Add(dyn);
                }
            }
            CompleteModel(model);
            model.Context.RssUrl = PortalSettings.PortalAlias.HTTPAlias +
                                   "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + Module.ModuleID + "&tabId=" + MainTabId;
            return model;
        }
        private dynamic GetModelAsDynamicFromJson()
        {
            string json = dataJson;
            if (LocaleController.Instance.GetLocales(PortalSettings.PortalId).Count > 1)
            {
                json = JsonUtils.SimplifyJson(dataJson, LocaleController.Instance.GetCurrentLocale(PortalSettings.PortalId).Code);
            }
            dynamic model = JsonUtils.JsonToDynamic(json);
            CompleteModel(model);

            return model;
        }

        private void CompleteModel(dynamic model)
        {
            if (ManifestFiles != null && ManifestFiles.SchemaInTemplate)
            {
                // schema
                string schemaFilename = PhysicalTemplateFolder + "schema.json";
                try
                {
                    dynamic schema = JsonUtils.JsonToDynamic(File.ReadAllText(schemaFilename));
                    model.Schema = schema;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Invalid json-schema. Please verify file {0}.", schemaFilename), ex);
                }
            }
            if (ManifestFiles != null && ManifestFiles.OptionsInTemplate)
            {
                // options
                JToken optionsJson = null;
                // default options
                string optionsFilename = PhysicalTemplateFolder + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = PhysicalTemplateFolder + "options." + DnnUtils.GetCurrentCultureCode() + ".json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        var extraJson = fileContent.ToJObject("Options cultureSpecific");
                        if (optionsJson == null)
                            optionsJson = extraJson;
                        else
                            optionsJson = optionsJson.JsonMerge(extraJson);
                    }
                }
                if (optionsJson != null)
                {
                    dynamic Options = JsonUtils.JsonToDynamic(optionsJson.ToString());
                    model.Options = Options;
                }
            }
            // settings
            if (settingsJson != null)
            {
                model.Settings = JsonUtils.JsonToDynamic(settingsJson);
            }
            string editRole = Manifest == null ? "" : Manifest.EditRole;
            // context
            model.Context = new ExpandoObject();
            model.Context.ModuleId = Module.ModuleID;
            model.Context.ModuleTitle = Module.ModuleTitle;

            model.Context.AddUrl = EditUrl();
            model.Context.IsEditable = IsEditable ||
                                      (!string.IsNullOrEmpty(editRole) &&
                                        OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, -1));
            model.Context.PortalId = PortalSettings.PortalId;
            model.Context.MainUrl = Globals.NavigateURL(MainTabId, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode());

        }

        private void CompleteModel(JObject model)
        {
            if (ManifestFiles != null && ManifestFiles.SchemaInTemplate)
            {
                // schema
                string schemaFilename = PhysicalTemplateFolder + "schema.json";
                try
                {
                    model["Schema"] = JObject.Parse(File.ReadAllText(schemaFilename));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Invalid json-schema. Please verify file {0}.", schemaFilename), ex);
                }
            }
            if (ManifestFiles != null && ManifestFiles.OptionsInTemplate)
            {
                // options
                JToken optionsJson = null;
                // default options
                string optionsFilename = PhysicalTemplateFolder + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = PhysicalTemplateFolder + "options." + DnnUtils.GetCurrentCultureCode() + ".json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        var extraJson = fileContent.ToJObject("Options cultureSpecific");
                        if (optionsJson == null)
                            optionsJson = extraJson;
                        else
                            optionsJson = optionsJson.JsonMerge(extraJson);
                    }
                }
                if (optionsJson != null)
                {
                    model["Options"] = optionsJson;
                }
            }
            // settings
            if (settingsJson != null)
            {
                model["Settings"] = JObject.Parse(settingsJson);
            }
            string editRole = Manifest == null ? "" : Manifest.EditRole;
            // context
            JObject context = new JObject();
            model["Context"] = context;
            context["ModuleId"] = Module.ModuleID;
            context["ModuleTitle"] = Module.ModuleTitle;
            context["AddUrl"] = EditUrl();
            context["IsEditable"] = IsEditable ||
                                      (!string.IsNullOrEmpty(editRole) &&
                                        OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, -1));
            context["PortalId"] = PortalSettings.PortalId;
            context["MainUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode());
        }

        private string EditUrl()
        {
            return EditUrl("", "", "Edit");
        }
        private string EditUrl(string controlKey)
        {
            return EditUrl("", "", controlKey);
        }

        private string EditUrl(string keyName, string keyValue)
        {
            return EditUrl(keyName, keyValue, "Edit");
        }

        private string EditUrl(string keyName, string keyValue, string controlKey)
        {
            var parameters = new string[] { };
            return EditUrl(keyName, keyValue, controlKey, parameters);
        }

        private string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            string key = controlKey;
            if (string.IsNullOrEmpty(key))
            {
                key = "Edit";
            }
            string moduleIdParam = string.Empty;
            if (Module != null)
            {
                moduleIdParam = string.Format("mid={0}", Module.ModuleID);
            }

            string[] parameters;
            if (!string.IsNullOrEmpty(keyName) && !string.IsNullOrEmpty(keyValue))
            {
                parameters = new string[2 + additionalParameters.Length];
                parameters[0] = moduleIdParam;
                parameters[1] = string.Format("{0}={1}", keyName, keyValue);
                Array.Copy(additionalParameters, 0, parameters, 2, additionalParameters.Length);
            }
            else
            {
                parameters = new string[1 + additionalParameters.Length];
                parameters[0] = moduleIdParam;
                Array.Copy(additionalParameters, 0, parameters, 1, additionalParameters.Length);
            }

            return NavigateUrl(PortalSettings.ActiveTab.TabID, key, false, parameters);
        }

        private string NavigateUrl(int tabId, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return NavigateUrl(tabId, controlKey, Globals.glbDefaultPage, pageRedirect, additionalParameters);
        }

        private string NavigateUrl(int tabId, string controlKey, string pageName, bool pageRedirect, params string[] additionalParameters)
        {
            var isSuperTab = Globals.IsHostTab(tabId);
            var settings = PortalSettings;
            var language = GetCultureCode(tabId, isSuperTab, settings);
            var url = Globals.NavigateURL(tabId, isSuperTab, settings, controlKey, language, pageName, additionalParameters);

            // Making URLs call popups
            if (PortalSettings != null && PortalSettings.EnablePopUps)
            {
                if (!UIUtilities.IsLegacyUI(Module.ModuleID, controlKey, settings.PortalId) && (url.Contains("ctl")))
                {
                    url = UrlUtils.PopUpUrl(url, null, PortalSettings, false, pageRedirect);
                }
            }
            return url;
        }

        private static string GetCultureCode(int tabId, bool isSuperTab, PortalSettings settings)
        {
            string cultureCode = Null.NullString;
            if (settings != null)
            {
                TabController tc = new TabController();
                TabInfo linkTab = tc.GetTab(tabId, isSuperTab ? Null.NullInteger : settings.PortalId, false);
                if (linkTab != null)
                {
                    cultureCode = linkTab.CultureCode;
                }
                if (string.IsNullOrEmpty(cultureCode))
                {
                    cultureCode = Thread.CurrentThread.CurrentCulture.Name;
                }
            }

            return cultureCode;
        }
        private bool? _isEditable;
        private bool IsEditable
        {
            get
            {
                //Perform tri-state switch check to avoid having to perform a security
                //role lookup on every property access (instead caching the result)
                if (!_isEditable.HasValue)
                {
                    bool blnPreview = (PortalSettings.UserMode == PortalSettings.Mode.View);
                    if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                    {
                        blnPreview = false;
                    }
                    bool blnHasModuleEditPermissions = false;
                    if (Module != null)
                    {
                        blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", Module);
                    }
                    if (blnPreview == false && blnHasModuleEditPermissions)
                    {
                        _isEditable = true;
                    }
                    else
                    {
                        _isEditable = false;
                    }
                }
                return _isEditable.Value;
            }
        }
    }
}