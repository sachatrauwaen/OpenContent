using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class ModelFactory
    {
        private JToken _dataJson;
        private readonly IDataItem _data;
        private readonly IEnumerable<IDataItem> _dataList = null;
        private readonly string _settingsJson;
        private readonly string _physicalTemplateFolder;
        private readonly Manifest.Manifest _manifest;
        private readonly TemplateManifest _templateManifest;
        private readonly TemplateFiles _templateFiles;
        private readonly OpenContentModuleInfo _module;
        private readonly PortalSettings _portalSettings;
        private readonly int _portalId;
        private readonly int _detailTabId;
        private readonly string _cultureCode;

        public JObject Options { get; set; } // alpaca options.json format

        public ModelFactory(JToken dataJson, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            this._dataJson = dataJson;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public ModelFactory(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            this._dataJson = data.Data;
            this._data = data;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }
        public ModelFactory(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, int portalId, string cultureCode, int mainTabId, int mainModuleId)
        {
            this._dataJson = data.Data;
            this._data = data;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalId = portalId;
            this._cultureCode = cultureCode;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public ModelFactory(IEnumerable<IDataItem> dataList, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            OpenContentSettings settings = module.Settings;
            this._dataList = dataList;
            this._settingsJson = settings.Data;
            this._physicalTemplateFolder = settings.Template.ManifestFolderUri.PhysicalFullDirectory + "\\";
            this._manifest = settings.Template.Manifest;
            this._templateFiles = settings.Template != null ? settings.Template.Main : null;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = settings.Template;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public ModelFactory(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            this._dataList = dataList;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }


        public ModelFactory(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, int portalId, string cultureCode)
        {
            this._dataList = dataList;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalId = portalId;
            this._cultureCode = cultureCode;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public dynamic GetModelAsDynamic(bool onlyData = false)
        {
            if (_portalSettings == null) onlyData = true;

            JToken model = GetModelAsJson(onlyData);
            return JsonUtils.JsonToDynamic(model.ToString());
        }

        /// <summary>
        /// Gets the model as dynamic list, used by Url Rewriter
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> GetModelAsDynamicList()
        {
            var completeModel = new JObject();
            EnhanceModel(completeModel, true);
            if (_dataList != null)
            {
                foreach (var item in _dataList)
                {
                    var model = item.Data as JObject;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
                    }
                    if (_manifest.AdditionalDataExists() && completeModel["AdditionalData"] != null && completeModel["Options"] != null)
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
            if (_portalSettings == null) onlyData = true;

            if (_dataList == null)
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
                EnhanceModel(model, onlyData);
                model["Context"]["RssUrl"] = _portalSettings.PortalAlias.HTTPAlias +
                       "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + _module.ViewModule.ModuleID + "&tabId=" + _detailTabId;

            }
            JArray items = new JArray(); ;
            model["Items"] = items;
            //string editRole = Manifest.GetEditRole();
            if (_dataList != null && _dataList.Any())
            {
                foreach (var item in _dataList)
                {
                    JObject dyn = item.Data as JObject;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(dyn, GetCurrentCultureCode());
                    }
                    if (_manifest != null && _manifest.AdditionalDataExists() && model["AdditionalData"] != null && model["Options"] != null)
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
                        if (_manifest != null && !string.IsNullOrEmpty(_manifest.DetailUrl))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            dynamic dynForHBS = JsonUtils.JsonToDynamic(dyn.ToString());
                            url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                            url = HttpUtility.HtmlDecode(url);
                        }

                        var editStatus = !_manifest.DisableEdit && GetEditStatus(item.CreatedByUserId);
                        context["IsEditable"] = editStatus;
                        context["EditUrl"] = editStatus ? DnnUrlUtils.EditUrl("id", item.Id, _module.ViewModule.ModuleID, _portalSettings) : "";
                        context["DetailUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), url.CleanupUrl(), "id=" + item.Id);
                        context["MainUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), "");
                    }
                    items.Add(dyn);
                }
            }
            return model;
        }

        private JToken GetModelAsJsonFromJson(bool onlyData)
        {
            var model = _dataJson as JObject;
            if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
            {
                JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            }

            var enhancedModel = new JObject();
            EnhanceModel(enhancedModel, onlyData);
            if (_manifest.AdditionalDataExists() && enhancedModel["AdditionalData"] != null && enhancedModel["Options"] != null)

            {
                JsonUtils.LookupJson(model, enhancedModel["AdditionalData"] as JObject, enhancedModel["Options"] as JObject);
            }
            JsonUtils.Merge(model, enhancedModel);
            return model;
        }

        private void EnhanceModel(JObject model, bool onlyData)
        {
            if (!onlyData && _templateFiles != null && _templateFiles.SchemaInTemplate)
                // include SCHEMA info in the Model
                if (!onlyData && _templateFiles != null && _templateFiles.SchemaInTemplate)
                {
                    // schema
                    string schemaFilename = _physicalTemplateFolder + "schema.json";
                    model["Schema"] = JsonUtils.GetJsonFromFile(schemaFilename);
                }

            // include OPTIONS info in the Model
            if (_templateFiles != null && _templateFiles.OptionsInTemplate)
            {
                // options
                JToken optionsJson = null;
                // default options
                string optionsFilename = _physicalTemplateFolder + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = _physicalTemplateFolder + "options." + GetCurrentCultureCode() + ".json";
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

            // include additional data in the Model
            if (_templateFiles != null && _templateFiles.AdditionalDataInTemplate && _manifest.AdditionalDataExists())
            {
                var additionalData = model["AdditionalData"] = new JObject();
                foreach (var item in _manifest.AdditionalData)
                {
                    var dataManifest = item.Value;
                    var ds = DataSourceManager.GetDataSource(_manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(_module);
                    var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? item.Key);
                    JToken additionalDataJson = new JObject();
                    if (dsItem?.Data != null)
                    {
                        if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                        {
                            JsonUtils.SimplifyJson(dsItem.Data, GetCurrentCultureCode());
                        }
                        additionalDataJson = dsItem.Data;
                    }
                    additionalData[(item.Value.ModelKey ?? item.Key).ToLowerInvariant()] = additionalDataJson;
                }
            }

            // include settings in the Model
            if (!onlyData && _templateManifest.SettingsNeeded() && !string.IsNullOrEmpty(_settingsJson))
            {
                try
                {
                    _dataJson = JToken.Parse(_settingsJson);
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(_dataJson, GetCurrentCultureCode());
                    }
                    model["Settings"] = _dataJson;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error parsing Json of Settings", ex);
                }
            }

            // include static localization in the Model
            if (!onlyData)
            {
                JToken localizationJson = null;
                string localizationFilename = _physicalTemplateFolder + GetCurrentCultureCode() + ".json";
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
                // include CONTEXT in the Model
                JObject context = new JObject();
                model["Context"] = context;
                context["ModuleId"] = _module.ViewModule.ModuleID;
                context["GoogleApiKey"] = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetGoogleApiKey();
                context["ModuleTitle"] = _module.ViewModule.ModuleTitle;
                context["AddUrl"] = DnnUrlUtils.EditUrl(_module.ViewModule.ModuleID, _portalSettings);
                var editStatus = !_manifest.DisableEdit && GetEditStatus(-1);
                context["IsEditable"] = editStatus;
                context["PortalId"] = _portalId;
                context["MainUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode());
                if (_data != null)
                {
                    string url = "";
                    if (_manifest != null && !string.IsNullOrEmpty(_manifest.DetailUrl))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        dynamic dynForHBS = JsonUtils.JsonToDynamic(model.ToString());
                        url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                        url = HttpUtility.HtmlDecode(url);
                    }
                    context["DetailUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), url.CleanupUrl(), "id=" + _data.Id);
                    context["Id"] = _data.Id;
                    context["EditUrl"] = editStatus ? DnnUrlUtils.EditUrl("id", _data.Id, _module.ViewModule.ModuleID, _portalSettings) : "";
                }
            }
        }

        private bool GetEditStatus(int createdByUser)
        {
            string editRole = _manifest.GetEditRole();
            return (IsEditable || OpenContentUtils.HasEditRole(_portalSettings, editRole, createdByUser)) // edit Role can edit whtout be in edit mode
                    && OpenContentUtils.HasEditPermissions(_portalSettings, _module.ViewModule, editRole, createdByUser);
        }

        private string GetCurrentCultureCode()
        {
            if (string.IsNullOrEmpty(_cultureCode))
            {
                return DnnLanguageUtils.GetCurrentCultureCode();
            }
            else
            {
                return _cultureCode;
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
                    _isEditable = _module.DataModule.CheckIfEditable(PortalSettings.Current);

                }
                return _isEditable.Value;
            }
        }
    }
}