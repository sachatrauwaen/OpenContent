using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Querying;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;


namespace Satrabel.OpenContent.Components.Render
{
    public abstract class ModelFactoryBase
    {
        private readonly string _settingsJson;
        protected readonly TemplateFiles _templateFiles;
        protected readonly int _portalId;
        private readonly string _cultureCode;
        protected readonly string _collection;

        protected JObject _schemaJson = null;
        protected JObject _optionsJson = null;
        private JObject _additionalData = null;

        protected IDataSource _ds;
        protected DataSourceContext _dsContext;

        // only multiple
        protected readonly Manifest.Manifest _manifest;
        protected readonly TemplateManifest _templateManifest;
        protected readonly OpenContentModuleConfig _module;
        protected readonly int _detailTabId;

        protected ModelFactoryBase(string settingsJson, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module)
        {
            this._settingsJson = settingsJson;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalId = module.PortalId;
            this._templateManifest = templateManifest;
            this._collection = templateManifest == null ? App.Config.DefaultCollection : templateManifest.Collection;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());

            _ds = DataSourceManager.GetDataSource(_manifest.DataSource);
            _dsContext = OpenContentUtils.CreateDataContext(_module);
        }

        protected ModelFactoryBase(string settingsJson, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module, int portalId, string cultureCode)
        {
            this._settingsJson = settingsJson;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._cultureCode = cultureCode;
            this._portalId = portalId;
            this._templateManifest = templateManifest;
            this._collection = templateManifest.Collection;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
            _ds = DataSourceManager.GetDataSource(_manifest.DataSource);
            _dsContext = OpenContentUtils.CreateDataContext(_module);
        }

        protected ModelFactoryBase(OpenContentModuleConfig module)
        {
            OpenContentSettings settings = module.Settings;
            this._settingsJson = settings.Data;
            this._manifest = settings.Template.Manifest;
            this._templateFiles = settings.Template?.Main;
            this._module = module;
            this._portalId = module.PortalId;
            this._templateManifest = settings.Template;
            this._collection = _templateManifest.Collection;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
            _ds = DataSourceManager.GetDataSource(_manifest.DataSource);
            _dsContext = OpenContentUtils.CreateDataContext(_module);
        }
        protected ModelFactoryBase(OpenContentModuleConfig module, string collection)
        {
            OpenContentSettings settings = module.Settings;
            this._settingsJson = settings.Data;
            this._manifest = settings.Template.Manifest;
            this._templateFiles = settings.Template?.Main;
            this._module = module;
            this._portalId = module.PortalId;
            this._templateManifest = settings.Template;
            this._collection = collection;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
            _ds = DataSourceManager.GetDataSource(_manifest.DataSource);
            _dsContext = OpenContentUtils.CreateDataContext(_module);
        }

        public JObject Options { get; set; } // alpaca options.json format


        public dynamic GetModelAsDynamic(bool onlyData = false)
        {
            if (_module.CanvasUnavailable) onlyData = true;

            JToken model = GetModelAsJson(onlyData);
            return JsonUtils.JsonToDynamic(model.ToString());
        }
        public Dictionary<string, object> GetModelAsDictionary(bool onlyData = false, bool onlyMainData = false)
        {
            if (_module.CanvasUnavailable) onlyData = true;

            JToken model = GetModelAsJson(onlyData, onlyMainData);
            return JsonUtils.JsonToDictionary(model.ToString());
        }

        public abstract JToken GetModelAsJson(bool onlyData = false, bool onlyMainData = false);

        protected void EnhanceImages(JObject model)
        {
            if (_optionsJson == null)
            {
                var alpaca = _ds.GetAlpaca(_dsContext, true, true, false);

                if (alpaca != null)
                {
                    _schemaJson = alpaca["schema"] as JObject; // cache
                    _optionsJson = alpaca["options"] as JObject; // cache
                }
            }
            JsonUtils.ImagesJson(model, Options, _optionsJson, IsEditMode);
        }

        protected void EnhanceSelect2(JObject model, bool onlyData)
        {
            string colName = string.IsNullOrEmpty(_collection) ? "Items" : _collection;
            bool addDataEnhance = _manifest.AdditionalDataDefined();
            if (addDataEnhance && _additionalData == null)
            {
                GetAdditionalData(onlyData);
            }
            bool collectionEnhance = _templateFiles?.Model != null && _templateFiles.Model.ContainsKey(colName);
            bool enhance = addDataEnhance || collectionEnhance || (_templateFiles != null && _templateFiles.LabelsInTemplate);

            if (enhance && (_optionsJson == null || _schemaJson == null))
            {
                var alpaca = _ds.GetAlpaca(_dsContext, true, true, false);

                if (alpaca != null)
                {
                    _schemaJson = alpaca["schema"] as JObject; // cache
                    _optionsJson = alpaca["options"] as JObject; // cache
                }
            }
            if (enhance)
            {
                var colManifest = collectionEnhance ? _templateFiles.Model[colName] : null;
                var includes = colManifest?.Includes;
                var includelabels = _templateFiles != null && _templateFiles.LabelsInTemplate;
                var ds = DataSourceManager.GetDataSource(_manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(_module);
                JsonUtils.LookupJson(model, _additionalData, _schemaJson, _optionsJson, includelabels, includes,
                    (col, id) =>
                    {
                        // collection enhancement
                        dsContext.Collection = col;
                        var dsItem = ds.Get(dsContext, id);
                        if (dsItem != null && dsItem.Data is JObject)
                        {
                            return dsItem.Data as JObject;
                        }
                        else
                        {
                            JObject res = new JObject();
                            res["Id"] = id;
                            res["Collection"] = col;
                            res["Title"] = "unknow";
                            return res;
                        }
                    },
                    (key) =>
                    {
                        return ds.GetDataAlpaca(dsContext, true, true, false, key);
                    });
            }
            if (_optionsJson != null)
            {
                LookupSelect2InOtherModule(model, _optionsJson, onlyData);
            }
        }

        

        protected void ExtendModel(JObject model, bool onlyData, bool onlyMainData)
        {
            if (_module.CanvasUnavailable) onlyData = true;

            if (_templateFiles != null)
            {
                // include additional data in the Model
                if (_templateFiles.AdditionalDataInTemplate && _manifest.AdditionalDataDefined())
                {
                    model["AdditionalData"] = GetAdditionalData(onlyData);
                }
                // include collections
                if (_templateFiles.Model != null)
                {
                    var additionalCollections = _templateFiles.Model.Where(c => c.Key != _collection);
                    if (additionalCollections.Any())
                    {
                        var collections = model["Collections"] = new JObject();
                        var dsColContext = OpenContentUtils.CreateDataContext(_module);
                        foreach (var item in additionalCollections)
                        {
                            var colManifest = item.Value;
                            dsColContext.Collection = item.Key;
                            Select select = null;
                            if (item.Value.Query != null)
                            {
                                var indexConfig = OpenContentUtils.GetIndexConfig(_module.Settings.TemplateDir, item.Key);
                                QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                                var u = PortalSettings.Current.UserInfo;
                                queryBuilder.Build(item.Value.Query, true, u.UserID, DnnLanguageUtils.GetCurrentCultureCode(), u.Social.Roles.FromDnnRoles());
                                select = queryBuilder.Select;
                            }
                            IDataItems dataItems = _ds.GetAll(dsColContext, select);
                            var colDataJson = new JArray();
                            foreach (var dataItem in dataItems.Items)
                            {
                                var json = dataItem.Data;
                                if (json != null )
                                {
                                    JsonUtils.SimplifyJson(json, GetCurrentCultureCode());
                                }
                                if (json is JObject)
                                {
                                    JObject context = new JObject();
                                    json["Context"] = context;
                                    context["Id"] = dataItem.Id;
                                    EnhanceSelect2(json as JObject, onlyData);
                                    JsonUtils.SimplifyJson(json, GetCurrentCultureCode());
                                }
                                colDataJson.Add(json);
                            }
                            collections[item.Key] = new JObject();
                            collections[item.Key]["Items"] = colDataJson;
                        }
                    }
                }
            }
            // include settings in the Model
            if (!onlyMainData && _templateManifest != null && _templateManifest.SettingsNeeded() && !string.IsNullOrEmpty(_settingsJson))
            {
                try
                {
                    var jsonSettings = JToken.Parse(_settingsJson);
                    if (DnnLanguageUtils.GetPortalLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(jsonSettings, GetCurrentCultureCode());
                    }
                    model["Settings"] = jsonSettings;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error parsing Json of Settings", ex);
                }
            }

            // include static localization in the Model
            if (!onlyMainData)
            {
                var localizationJson = LocalizationUtils.LoadLocalizationJson(_module.Settings.TemplateDir, GetCurrentCultureCode());
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
                context["TabId"] = _module.ViewModule.TabId;
                context["ModuleId"] = _module.ViewModule.ModuleId;
                context["GoogleApiKey"] = App.Services.CreateGlobalSettingsRepository(_portalId).GetGoogleApiKey();
                context["ModuleTitle"] = _module.ViewModule.ModuleTitle;
                var editIsAllowed = !_manifest.DisableEdit && IsEditAllowed(-1);
                context["IsEditable"] = editIsAllowed; //allowed to edit the item or list (meaning allow Add)
                context["IsEditMode"] = IsEditMode;
                context["PortalId"] = _portalId;
                context["MainUrl"] = _module.GetUrl(_detailTabId, GetCurrentCultureCode());
                context["HomeDirectory"] = _module.HomeDirectory;
                context["HTTPAlias"] = _module.HostName;
                context["PortalName"] = _module.PortalName;
            }
        }

        private JObject GetAdditionalData(bool onlyData)
        {
            if (_additionalData == null && _manifest.AdditionalDataDefined())
            {
                _additionalData = new JObject();
                foreach (var item in _manifest.AdditionalDataDefinition)
                {
                    var dataManifest = item.Value;
                    IDataItem dataItem = _ds.GetData(_dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? item.Key);
                    JToken additionalDataJson = new JObject();
                    var json = dataItem?.Data;
                    if (json != null)
                    {
                        //if (DnnLanguageUtils.GetPortalLocales(_portalId).Count > 1)
                        {
                            JsonUtils.SimplifyJson(json, GetCurrentCultureCode());
                        }
                        additionalDataJson = json;

                        //optionally add editurl for AdditionalData. Only for Single items (not arrays)
                        if (!onlyData && json is JObject && IsEditMode)
                        {
                            var context = new JObject();
                            context["EditUrl"] = GetAdditionalDataEditUrl(item.Key);
                            additionalDataJson["Context"] = context;
                        }
                    }
                    if (!App.Services.CreateGlobalSettingsRepository(_portalId).GetLegacyHandlebars())
                        _additionalData[(item.Value.ModelKey ?? item.Key)] = additionalDataJson;
                    else
                        _additionalData[(item.Value.ModelKey ?? item.Key).ToLowerInvariant()] = additionalDataJson;
                }
            }
            return _additionalData;
        }

        /// <summary>
        /// Gets the additional data edit URL.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <returns></returns>
        private string GetAdditionalDataEditUrl(string keyValue)
        {
            return _module.EditAddDataUrl("key", keyValue, _module.ViewModule.ModuleId);
        }

        protected void ExtendSchemaOptions(JObject model, bool onlyData)
        {
            if (_module.CanvasUnavailable) onlyData = true;

            if (_templateFiles != null)
            {
                bool includeSchema = !onlyData && _templateFiles.SchemaInTemplate;
                bool includeOptions = _templateFiles.OptionsInTemplate;
                if (includeSchema || includeOptions)
                {
                    var alpaca = _ds.GetAlpaca(_dsContext, includeSchema, includeOptions, false);
                    // include SCHEMA info in the Model
                    if (alpaca != null && includeSchema)
                    {
                        model["Schema"] = alpaca["schema"];
                        _schemaJson = alpaca["schema"] as JObject; // cache
                    }
                    // include OPTIONS info in the Model
                    if (alpaca != null && includeOptions)
                    {
                        model["Options"] = alpaca["options"];
                        _optionsJson = alpaca["options"] as JObject; // cache
                    }
                }
            }
        }

        protected bool IsEditAllowed(int createdByUser)
        {
            string editRole = _manifest.GetEditRole();
            return (IsEditMode || DnnPermissionsUtils.HasEditRole(_module, editRole, createdByUser)) // edit Role can edit without being in edit mode
                    && DnnPermissionsUtils.HasEditPermissions(_module, editRole, createdByUser);
        }

        protected bool HasEditPermissions(int createdByUser)
        {
            string editRole = _manifest.GetEditRole();
            return DnnPermissionsUtils.HasEditPermissions(_module, editRole, createdByUser);
        }

        protected string GetCurrentCultureCode()
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

        private bool? _isEditMode;

        protected bool IsEditMode
        {
            get
            {
                //Perform tri-state switch check to avoid having to perform a security
                //role lookup on every property access (instead caching the result)
                if (!_isEditMode.HasValue)
                {
                    _isEditMode = _module.DataModule.CheckIfEditable(_module);
                }
                return _isEditMode.Value;
            }
        }

        protected void LookupSelect2InOtherModule(JObject model, JObject options, bool onlyData)
        {

            foreach (var child in model.Children<JProperty>().ToList())
            {
                JObject opt = null;
                if (options?["fields"] != null)
                {
                    opt = options["fields"][child.Name] as JObject;
                }
                if (opt == null) continue;
                bool lookup =
                    opt["type"] != null &&
                    opt["type"].ToString() == "select2" &&
                    opt["dataService"]?["action"] != null &&
                    opt["dataService"]?["action"].ToString() == "Lookup";
 
                    //opt["dataService"]?["data"]?["moduleId"] != null &&
                    //opt["dataService"]?["data"]?["tabId"] != null;

                string dataMember = "";
                string valueField = "Id";
                string moduleId = "";
                string tabId = "";
                if (lookup)
                {
                    dataMember = opt["dataService"]["data"]["dataMember"]?.ToString() ?? "";
                    valueField = opt["dataService"]["data"]["valueField"]?.ToString() ?? "Id";
                    moduleId = opt["dataService"]["data"]["moduleId"]?.ToString() ?? "0";
                    tabId = opt["dataService"]["data"]["tabId"]?.ToString() ?? "0";
                }

                var childProperty = child;

                if (childProperty.Value is JArray)
                {
                    var array = childProperty.Value as JArray;
                    JArray newArray = new JArray();
                    foreach (var value in array)
                    {
                        var obj = value as JObject;
                        if (obj != null)
                        {
                            LookupSelect2InOtherModule(obj, opt["items"] as JObject, onlyData);
                        }
                        else if (lookup)
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    newArray.Add(GenerateObject(val.ToString(), int.Parse(tabId), int.Parse(moduleId), onlyData));
                                }
                                catch (System.Exception)
                                {
                                    Debugger.Break();
                                }
                            }
                        }
                    }
                    if (lookup)
                    {
                        childProperty.Value = newArray;
                    }
                }
                else if (childProperty.Value is JObject)
                {
                    var obj = childProperty.Value as JObject;
                    LookupSelect2InOtherModule(obj, opt, onlyData);
                }
                else if (childProperty.Value is JValue)
                {
                    if (lookup)
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {                            
                            model[childProperty.Name] = GenerateObject(val, int.Parse(tabId), int.Parse(moduleId), onlyData);
                        }
                        catch (System.Exception )
                        {
                            Debugger.Break();
                        }
                    }
                }
            }
        }

        private JToken GenerateObject(string id, int tabId, int moduleId, bool onlyData)
        {
            var module = moduleId> 0 ? OpenContentModuleConfig.Create(moduleId, tabId, PortalSettings.Current) : _module;
            var ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(module);
            IDataItem dataItem = ds.Get(dsContext, id);
            if (dataItem != null)
            {
                var json = dataItem?.Data?.DeepClone() as JObject;
                //if (!string.IsNullOrEmpty(dataMember))
                //{
                //    json = json[dataMember];
                //}
                if (json != null)
                {
                    JsonUtils.SimplifyJson(json, GetCurrentCultureCode());
                    if (!onlyData)
                    {
                        var context = new JObject();
                        json["Context"] = context;
                        context["Id"] = dataItem.Id;
                        context["DetailUrl"] = GenerateDetailUrl(dataItem, json, module.Settings.Manifest, GetCurrentCultureCode(), tabId > 0 ? tabId : _detailTabId);
                    }
                    return json;
                }
            }
            JObject res = new JObject();
            res["Id"] = id;
            res["Title"] = "unknow";
            return res;
        }

        protected string GenerateDetailUrl(IDataItem item, JObject dyn, Manifest.Manifest manifest, string cultureCode, int detailTabId)
        {
            string url = "";
            if (!string.IsNullOrEmpty(manifest.DetailUrl))
            {
                HandlebarsEngine hbEngine = new HandlebarsEngine();
                var dynForHBS = JsonUtils.JsonToDictionary(dyn.ToString());

                url = hbEngine.Execute(manifest.DetailUrl, dynForHBS);
                url = HttpUtility.HtmlDecode(url);
            }
            return _module.GetUrl(_detailTabId, cultureCode, url.CleanupUrl(), "id=" + item.Id);            
        }
    }
}