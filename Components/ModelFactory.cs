using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using DotNetNuke.Services.Personalization;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.TemplateHelpers;

namespace Satrabel.OpenContent.Components
{
    public class ModelFactory
    {
        JToken dataJson;
        readonly IDataItem Data;
        readonly IEnumerable<IDataItem> DataList = null;
        readonly string settingsJson;
        readonly string PhysicalTemplateFolder;
        readonly Manifest.Manifest Manifest;
        readonly TemplateManifest TemplateManifest;
        readonly TemplateFiles TemplateFiles;
        readonly ModuleInfo Module;
        readonly PortalSettings PortalSettings;
        readonly int PortalId;
        readonly int MainTabId;
        readonly int MainModuleId;
        readonly string CultureCode;

        public JObject Options { get; set; } // alpaca options.json format

        public ModelFactory(JToken dataJson, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, ModuleInfo module, PortalSettings portalSettings, int mainTabId, int mainModuleId)
        {
            this.dataJson = dataJson;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.TemplateFiles = templateFiles;
            this.Module = module;
            this.PortalSettings = portalSettings;
            this.PortalId = portalSettings.PortalId;
            this.TemplateManifest = templateManifest;
            this.MainTabId = mainTabId > 0 ? mainTabId : module.TabID;
            this.MainTabId = DnnUtils.GetTabByCurrentCulture(this.PortalId, this.MainTabId, GetCurrentCultureCode());
            this.MainModuleId = mainModuleId > 0 ? mainModuleId : module.ModuleID;
        }
        public ModelFactory(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, ModuleInfo module, PortalSettings portalSettings, int mainTabId, int mainModuleId)
        {
            this.dataJson = data.Data;
            this.Data = data;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.TemplateFiles = templateFiles;
            this.Module = module;
            this.PortalSettings = portalSettings;
            this.PortalId = portalSettings.PortalId;
            this.TemplateManifest = templateManifest;
            this.MainTabId = mainTabId > 0 ? mainTabId : module.TabID;
            this.MainTabId = DnnUtils.GetTabByCurrentCulture(this.PortalId, this.MainTabId, GetCurrentCultureCode());
            this.MainModuleId = mainModuleId > 0 ? mainModuleId : module.ModuleID;
        }
        public ModelFactory(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, ModuleInfo module, int portalId, string cultureCode, int mainTabId, int mainModuleId)
        {
            this.dataJson = data.Data;
            this.Data = data;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.TemplateFiles = templateFiles;
            this.Module = module;
            this.PortalId = portalId;
            this.CultureCode = cultureCode;
            this.TemplateManifest = templateManifest;
            this.MainTabId = mainTabId > 0 ? mainTabId : module.TabID;
            this.MainTabId = DnnUtils.GetTabByCurrentCulture(this.PortalId, this.MainTabId, GetCurrentCultureCode());
            this.MainModuleId = mainModuleId > 0 ? mainModuleId : module.ModuleID;
        }

        public ModelFactory(IEnumerable<IDataItem> dataList, ModuleInfo module, PortalSettings portalSettings, int mainTabId)
        {
            OpenContentSettings settings = module.OpenContentSettings();
            this.DataList = dataList;
            this.settingsJson = settings.Data;
            this.PhysicalTemplateFolder = settings.Template.ManifestFolderUri.PhysicalFullDirectory + "\\";
            this.Manifest = settings.Template.Manifest;
            this.TemplateFiles = settings.Template != null ? settings.Template.Main : null;
            this.Module = module;
            this.PortalSettings = portalSettings;
            this.PortalId = portalSettings.PortalId;
            this.TemplateManifest = settings.Template;
            this.MainTabId = mainTabId > 0 ? mainTabId : module.TabID;
            this.MainTabId = DnnUtils.GetTabByCurrentCulture(this.PortalId, this.MainTabId, GetCurrentCultureCode());
            this.MainModuleId = settings.ModuleId > 0 ? settings.ModuleId : module.ModuleID;
        }

        public ModelFactory(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, ModuleInfo module, PortalSettings portalSettings, int mainTabId, int mainModuleId)
        {
            this.DataList = dataList;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.TemplateFiles = templateFiles;
            this.Module = module;
            this.PortalSettings = portalSettings;
            this.PortalId = portalSettings.PortalId;
            this.TemplateManifest = templateManifest;
            this.MainTabId = mainTabId > 0 ? mainTabId : module.TabID;
            this.MainTabId = DnnUtils.GetTabByCurrentCulture(this.PortalId, this.MainTabId, GetCurrentCultureCode());
            this.MainModuleId = mainModuleId > 0 ? mainModuleId : module.ModuleID;
        }


        public ModelFactory(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, ModuleInfo module, int portalId, string cultureCode, int mainTabId, int mainModuleId)
        {
            this.DataList = dataList;
            this.settingsJson = settingsJson;
            this.PhysicalTemplateFolder = physicalTemplateFolder;
            this.Manifest = manifest;
            this.TemplateFiles = templateFiles;
            this.Module = module;
            this.PortalId = portalId;
            this.CultureCode = cultureCode;
            this.TemplateManifest = templateManifest;
            this.MainTabId = mainTabId > 0 ? mainTabId : module.TabID;
            this.MainTabId = DnnUtils.GetTabByCurrentCulture(this.PortalId, this.MainTabId, GetCurrentCultureCode());
            this.MainModuleId = mainModuleId > 0 ? mainModuleId : module.ModuleID;
        }

        public dynamic GetModelAsDynamic(bool onlyData = false)
        {
            if (PortalSettings == null) onlyData = true;

            JToken model = GetModelAsJson(onlyData);
            return JsonUtils.JsonToDynamic(model.ToString());
            //if (DataList == null)
            //{
            //    return GetModelAsDynamicFromJson();
            //}
            //else
            //{
            //    return GetModelAsDynamicFromList();
            //}
        }
        /*
         * For Url Rewriter
         */
        public IEnumerable<dynamic> GetModelAsDynamicList()
        {
            var completeModel = new JObject();
            CompleteModel(completeModel, true);
            if (DataList != null)
            {
                foreach (var item in DataList)
                {
                    var model = item.Data as JObject;
                    if (LocaleController.Instance.GetLocales(PortalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
                    }
                    if (Manifest.AdditionalDataExists() && completeModel["AdditionalData"] != null && completeModel["Options"] != null)
                    {
                        JsonUtils.LookupJson(model, completeModel["AdditionalData"] as JObject, completeModel["Options"] as JObject);
                    }
                    //JsonUtils.Merge(model, completeModel);
                    JObject context = new JObject();
                    model["Context"] = context;
                    context["Id"] = item.Id;
                    yield return JsonUtils.JsonToDynamic(model.ToString());
                }
            }
        }

        public JToken GetModelAsJson(bool onlyData = false)
        {
            if (PortalSettings == null) onlyData = true;

            if (DataList == null)
            {
                return GetModelAsJsonFromJson(onlyData);
            }
            else
            {
                return GetModelAsJsonFromList(onlyData);
            }
        }

        private JToken GetModelAsJsonFromList(bool onlyData)
        {
            JObject model = new JObject();
            if (!onlyData)
            {
                CompleteModel(model, onlyData);
                model["Context"]["RssUrl"] = PortalSettings.PortalAlias.HTTPAlias +
                       "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + Module.ModuleID + "&tabId=" + MainTabId;
            }
            JArray items = new JArray(); ;
            model["Items"] = items;
            string editRole = Manifest.GetEditRole();
            if (DataList != null && DataList.Any())
            {
                foreach (var item in DataList)
                {
                    JObject dyn = item.Data as JObject;
                    if (LocaleController.Instance.GetLocales(PortalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(dyn, GetCurrentCultureCode());
                    }
                    if (Manifest != null && Manifest.AdditionalDataExists() && model["AdditionalData"] != null && model["Options"] != null)
                    {
                        JsonUtils.LookupJson(dyn, model["AdditionalData"] as JObject, model["Options"] as JObject);
                    }
                    if (Options != null && model["Options"] != null)
                    {
                        JsonUtils.ImagesJson(dyn, Options, model["Options"] as JObject, IsEditable);
                    }
                    JObject context = new JObject();
                    dyn["Context"] = context;
                    context["Id"] = item.Id;
                    if (onlyData)
                    {
                        if (model["Settings"] != null)
                        {
                            model.Remove("Settings");
                        }
                        if (model["Schema"] != null)
                        {
                            model.Remove("Schema");
                        }
                        if (model["Options"] != null)
                        {
                            model.Remove("Options");
                        }
                        if (model["AdditionalData"] != null)
                        {
                            model.Remove("AdditionalData");
                        }
                    }
                    else
                    {
                        string url = "";
                        if (Manifest != null && !string.IsNullOrEmpty(Manifest.DetailUrl))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            dynamic dynForHBS = JsonUtils.JsonToDynamic(dyn.ToString());
                            url = hbEngine.Execute(Manifest.DetailUrl, dynForHBS);
                        }

                        var editStatus = GetEditStatus(item.CreatedByUserId);
                        context["IsEditable"] = editStatus;
                        context["EditUrl"] = editStatus ? DnnUrlUtils.EditUrl("id", item.Id, Module.ModuleID, PortalSettings) : "";
                        context["DetailUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", GetCurrentCultureCode(), url.CleanupUrl(), "id=" + item.Id);
                        context["MainUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", GetCurrentCultureCode(), "");
                    }
                    items.Add(dyn);
                }
            }
            return model;
        }

        private JToken GetModelAsJsonFromJson(bool onlyData)
        {
            var model = dataJson as JObject;
            if (LocaleController.Instance.GetLocales(PortalId).Count > 1)
            {
                JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            }
            var completeModel = new JObject();
            CompleteModel(completeModel, onlyData);
            if (Manifest.AdditionalDataExists() && completeModel["AdditionalData"] != null && completeModel["Options"] != null)
            {
                JsonUtils.LookupJson(model, completeModel["AdditionalData"] as JObject, completeModel["Options"] as JObject);
            }
            JsonUtils.Merge(model, completeModel);
            return model;
        }

        private void CompleteModel(JObject model, bool onlyData)
        {
            if (!onlyData && TemplateFiles != null && TemplateFiles.SchemaInTemplate)
            {
                // schema
                string schemaFilename = PhysicalTemplateFolder + "schema.json";
                model["Schema"] = JsonUtils.GetJsonFromFile(schemaFilename);
            }
            if (TemplateFiles != null && TemplateFiles.OptionsInTemplate)
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
                optionsFilename = PhysicalTemplateFolder + "options." + GetCurrentCultureCode() + ".json";
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

            // additional data
            if (TemplateFiles != null && TemplateFiles.AdditionalDataInTemplate && Manifest.AdditionalDataExists())
            {
                var additionalData = model["AdditionalData"] = new JObject();
                foreach (var item in Manifest.AdditionalData)
                {
                    var dataManifest = item.Value;
                    var ds = DataSourceManager.GetDataSource(Manifest.DataSource);
                    var dsContext = new DataSourceContext()
                    {
                        PortalId = PortalId,
                        TabId = Module.TabID,
                        ModuleId = MainModuleId,
                        TabModuleId = Module.TabModuleID,
                        Config = Manifest.DataSourceConfig,
                    };
                    var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? item.Key);
                    JToken additionalDataJson = new JObject();
                    if (dsItem != null && dsItem.Data != null)
                    {
                        if (LocaleController.Instance.GetLocales(PortalId).Count > 1)
                        {
                            JsonUtils.SimplifyJson(dsItem.Data, GetCurrentCultureCode());
                        }
                        additionalDataJson = dsItem.Data;
                    }
                    additionalData[(item.Value.ModelKey ?? item.Key).ToLowerInvariant()] = additionalDataJson;
                }
            }

            // settings
            if (!onlyData && TemplateManifest.SettingsNeeded() && !string.IsNullOrEmpty(settingsJson))
            {
                try
                {
                    dataJson = JToken.Parse(settingsJson);
                    if (LocaleController.Instance.GetLocales(PortalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(dataJson, GetCurrentCultureCode());
                    }
                    model["Settings"] = dataJson;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error parsing Json of Settings", ex);
                }
            }

            // static localization
            if (!onlyData)
            {
                JToken localizationJson = null;
                string localizationFilename = PhysicalTemplateFolder + GetCurrentCultureCode() + ".json";
                if (File.Exists(localizationFilename))
                {
                    string fileContent = File.ReadAllText(localizationFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        localizationJson = fileContent.ToJObject("Localization: " + localizationFilename);
                    }
                }
                if (localizationJson != null)
                {
                    model["Localization"] = localizationJson;
                }
            }

            if (!onlyData)
            {
                // context
                JObject context = new JObject();
                model["Context"] = context;
                context["ModuleId"] = Module.ModuleID;
                context["ModuleTitle"] = Module.ModuleTitle;
                context["AddUrl"] = DnnUrlUtils.EditUrl(Module.ModuleID, PortalSettings);
                var editStatus = GetEditStatus(-1);
                context["IsEditable"] = editStatus;
                context["PortalId"] = PortalId;
                context["MainUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", GetCurrentCultureCode());
                if (Data != null)
                {
                    string url = "";
                    if (Manifest != null && !string.IsNullOrEmpty(Manifest.DetailUrl))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        dynamic dynForHBS = JsonUtils.JsonToDynamic(model.ToString());
                        url = hbEngine.Execute(Manifest.DetailUrl, dynForHBS);
                    }
                    context["DetailUrl"] = Globals.NavigateURL(MainTabId, false, PortalSettings, "", GetCurrentCultureCode(), url.CleanupUrl(), "id=" + Data.Id);
                    context["Id"] = Data.Id;
                    context["EditUrl"] = editStatus ? DnnUrlUtils.EditUrl("id", Data.Id, Module.ModuleID, PortalSettings) : "";
                }
            }
        }

        private bool GetEditStatus(int createdByUser)
        {
            string editRole = Manifest.GetEditRole();
            return IsEditable || OpenContentUtils.HasEditPermissions(PortalSettings, Module, editRole, createdByUser);
        }

        private string GetCurrentCultureCode()
        {
            if (string.IsNullOrEmpty(CultureCode))
            {
                return DnnLanguageUtils.GetCurrentCultureCode();
            }
            else
            {
                return CultureCode;
            }
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
                    _isEditable = Module.CheckIfEditable(PortalSettings.Current);

                }
                return _isEditable.Value;
            }
        }
    }
}