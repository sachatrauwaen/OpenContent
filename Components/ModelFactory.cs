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
using Satrabel.OpenContent.Components.Manifest;

namespace Satrabel.OpenContent.Components
{
    public class ModelFactory
    {
        readonly string dataJson;
        readonly string settingsJson;
        readonly string PhysicalTemplateFolder;
        readonly Manifest.Manifest Manifest;
        readonly TemplateFiles ManifestFiles;
        readonly ModuleInfo Module;
        readonly PortalSettings PortalSettings;
        readonly IEnumerable<OpenContentInfo> DataList = null;

        public ModelFactory(string dataJson, string settingsJson, string PhysicalTemplateFolder, Manifest.Manifest Manifest, TemplateFiles ManifestFiles, ModuleInfo Module, PortalSettings PortalSettings)
        {
            this.dataJson = dataJson;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = PhysicalTemplateFolder;
            this.Manifest = Manifest;
            this.ManifestFiles = ManifestFiles;
            this.Module = Module;
            this.PortalSettings = PortalSettings;
        }
        public ModelFactory(IEnumerable<OpenContentInfo> DataList, string settingsJson, string PhysicalTemplateFolder, Manifest.Manifest Manifest, TemplateFiles ManifestFiles, ModuleInfo Module, PortalSettings PortalSettings)
        {
            this.DataList = DataList;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = PhysicalTemplateFolder;
            this.Manifest = Manifest;
            this.ManifestFiles = ManifestFiles;
            this.Module = Module;
            this.PortalSettings = PortalSettings;
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
                    string dataJson = item.Json;
                    if (LocaleController.Instance.GetLocales(PortalSettings.PortalId).Count > 1)
                    {
                        dataJson = JsonUtils.SimplifyJson(dataJson, LocaleController.Instance.GetCurrentLocale(PortalSettings.PortalId).Code);
                    }
                    JObject dyn = JObject.Parse(dataJson);
                    JObject context = new JObject();
                    dyn["Context"] = context;

                    context["Id"] = item.ContentId;
                    context["EditUrl"] = EditUrl("id", item.ContentId.ToString());
                    context["IsEditable"] = IsEditable ||
                        (!string.IsNullOrEmpty(editRole) &&
                        OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, item.CreatedByUserId));
                    context["DetailUrl"] = Globals.NavigateURL(Module.TabID, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), /*OpenContentUtils.CleanupUrl(dyn.Title)*/"", "id=" + item.ContentId.ToString());
                    context["MainUrl"] = Globals.NavigateURL(Module.TabID, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), /*OpenContentUtils.CleanupUrl(dyn.Title)*/"");
                    items.Add(dyn);
                }
                
            }
            if (!onlyData)
            {
                CompleteModel(model);
            }
            return model;
        }

        private JToken GetModelAsJsonFromJson()
        {
            throw new NotImplementedException();
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
                        dataJson = JsonUtils.SimplifyJson(dataJson, LocaleController.Instance.GetCurrentLocale(PortalSettings.PortalId).Code);
                    }
                    dynamic dyn = JsonUtils.JsonToDynamic(dataJson);

                    dyn.Context = new ExpandoObject();
                    dyn.Context.Id = item.ContentId;
                    dyn.Context.EditUrl = EditUrl("id", item.ContentId.ToString());
                    dyn.Context.IsEditable = IsEditable ||
                        (!string.IsNullOrEmpty(editRole) &&
                        OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, item.CreatedByUserId));
                    dyn.Context.DetailUrl = Globals.NavigateURL(Module.TabID, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), /*OpenContentUtils.CleanupUrl(dyn.Title)*/"", "id=" + item.ContentId.ToString());
                    dyn.Context.MainUrl = Globals.NavigateURL(Module.TabID, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode(), /*OpenContentUtils.CleanupUrl(dyn.Title)*/"");

                    model.Items.Add(dyn);
                }
            }
            CompleteModel(model);
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
                string schemaFilename = PhysicalTemplateFolder + "\\" + "schema.json";
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
                string optionsFilename = PhysicalTemplateFolder + "\\" + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = PhysicalTemplateFolder + "\\" + "options." + DnnUtils.GetCurrentCultureCode() + ".json";
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
            model.Context.MainUrl = Globals.NavigateURL(Module.TabID, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode());

        }

        private void CompleteModel(JObject model)
        {
            if (ManifestFiles != null && ManifestFiles.SchemaInTemplate)
            {
                // schema
                string schemaFilename = PhysicalTemplateFolder + "\\" + "schema.json";
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
                string optionsFilename = PhysicalTemplateFolder + "\\" + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = PhysicalTemplateFolder + "\\" + "options." + DnnUtils.GetCurrentCultureCode() + ".json";
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
            context["MainUrl"] = Globals.NavigateURL(Module.TabID, false, PortalSettings, "", DnnUtils.GetCurrentCultureCode());

        }
        
        public string EditUrl()
        {
            return EditUrl("", "", "Edit");
        }

        public string EditUrl(string controlKey)
        {
            return EditUrl("", "", controlKey);
        }

        public string EditUrl(string keyName, string keyValue)
        {
            return EditUrl(keyName, keyValue, "Edit");
        }

        public string EditUrl(string keyName, string keyValue, string controlKey)
        {
            var parameters = new string[] { };
            return EditUrl(keyName, keyValue, controlKey, parameters);
        }
        public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
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

        public string NavigateUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return NavigateUrl(tabID, controlKey, Globals.glbDefaultPage, pageRedirect, additionalParameters);
        }

        public string NavigateUrl(int tabID, string controlKey, string pageName, bool pageRedirect, params string[] additionalParameters)
        {
            var isSuperTab = Globals.IsHostTab(tabID);
            var settings = PortalSettings;
            var language = GetCultureCode(tabID, isSuperTab, settings);
            var url = Globals.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);

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

        internal static string GetCultureCode(int TabID, bool IsSuperTab, PortalSettings settings)
        {
            string cultureCode = Null.NullString;
            if (settings != null)
            {
                TabInfo linkTab = TabController.Instance.GetTab(TabID, IsSuperTab ? Null.NullInteger : settings.PortalId, false);
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
        public bool IsEditable
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