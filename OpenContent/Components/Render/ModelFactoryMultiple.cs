using System;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Portals;

namespace Satrabel.OpenContent.Components.Render
{
    public class ModelFactoryMultiple : ModelFactoryBase
    {
        private readonly IEnumerable<IDataItem> _dataList = null;

        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, OpenContentModuleConfig module) :
            base(module)
        {
            this._dataList = dataList;
        }
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, OpenContentModuleConfig module, string collection) :
            base(module, collection)
        {
            this._dataList = dataList;
        }
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, string settingsJson,  Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module) :
            base(settingsJson, manifest, templateManifest, templateFiles, module)
        {
            this._dataList = dataList;
        }
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, string settingsJson, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module, int portalId, string cultureCode) :
            base(settingsJson, manifest, templateManifest, templateFiles, module, portalId, cultureCode)
        {
            this._dataList = dataList;
        }

        [Obsolete("This method is obsolete since aug 2017; use another constructor instead")]
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo moduleinfo, PortalSettings portalSettings) :
            base(settingsJson, manifest, templateManifest, templateFiles, OpenContentModuleConfig.Create(moduleinfo.ModuleId, moduleinfo.TabId, portalSettings))
        {
            this._dataList = dataList;
        }

        /// <summary>
        /// Gets the model as dictionary list, used by Url Rewriter
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dictionary<string, object>> GetModelAsDictionaryList()
        {
            var completeModel = new JObject();
            ExtendModel(completeModel, true, false);
            if (_dataList != null)
            {
                foreach (var item in _dataList)
                {
                    var model = item.Data.DeepClone() as JObject;
                    JObject context = new JObject();
                    model["Context"] = context;
                    context["Id"] = item.Id;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
                    }
                    EnhanceSelect2(model, true);
                    yield return JsonUtils.JsonToDictionary(model.ToString());
                }
            }
        }
        public override JToken GetModelAsJson(bool onlyData = false, bool onlyMainData = false)
        {
            if (_module.CanvasUnavailable) onlyData = true;
            JObject model = new JObject();
            var itemsModel = model;

            ExtendModel(model, onlyData, onlyMainData);
            ExtendSchemaOptions(itemsModel, onlyData || onlyMainData);
            ExtendItemsModel(itemsModel, onlyData || onlyMainData);

            if (!onlyData && !onlyMainData)
            {
                itemsModel["Context"]["RssUrl"] = _module.HostName + "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + _module.ViewModule.ModuleId + "&tabId=" + _detailTabId;
            }
            var items = new JArray(); ;
            itemsModel["Items"] = items;
            if (_dataList != null && _dataList.Any())
            {
                var mainUrl = _module.GetUrl(_detailTabId, GetCurrentCultureCode());
                foreach (var item in _dataList)
                {
                    JObject dyn = item.Data as JObject;
                    JObject context = new JObject();
                    dyn["Context"] = context;
                    context["Id"] = item.Id;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(dyn, GetCurrentCultureCode());
                    }
                    EnhanceSelect2(dyn, onlyData);
                    EnhanceUser(dyn, item.CreatedByUserId);
                    EnhanceImages(dyn, itemsModel);
                    if (onlyData)
                    {
                        RemoveNoData(itemsModel);
                    }
                    else 
                    {
                        string url = "";
                        if (!string.IsNullOrEmpty(_manifest.DetailUrl))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            var dynForHBS = JsonUtils.JsonToDictionary(dyn.ToString());
                            url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                            url = HttpUtility.HtmlDecode(url);
                        }
                        var editStatus = !_manifest.DisableEdit && IsEditAllowed(item.CreatedByUserId);
                        context["IsEditable"] = editStatus;
                        if (HasEditPermissions(item.CreatedByUserId))
                        {
                            context["EditUrl"] = _module.EditUrl("id", item.Id, _module.ViewModule.ModuleId);
                        }
                        context["DetailUrl"] = _module.GetUrl(_detailTabId, url.CleanupUrl(), "id=" + item.Id);
                        context["MainUrl"] = mainUrl;
                    }
                    items.Add(dyn);
                }
            }
            return model;
        }

        private void ExtendItemsModel(JObject model, bool onlyData)
        {
            if (_module.CanvasUnavailable) onlyData = true;

            if (!onlyData)
            {
                // include CONTEXT in the Model
                JObject context;
                if (model["Context"] is JObject)
                {
                    context = model["Context"] as JObject;
                }
                else
                {
                    context = new JObject();
                    model["Context"] = context;
                }
                context["AddUrl"] = _module.EditUrl(_module.ViewModule.ModuleId);
            }
        }

        private void EnhanceUser(JObject model, int createdByUserId)
        {
            string colName = string.IsNullOrEmpty(_collection) ? "Items" : _collection;
            if (_templateManifest != null && !string.IsNullOrEmpty(colName) && _templateFiles?.Model != null && _templateFiles.Model.ContainsKey(colName))
            {
                var colManifest = _templateFiles.Model[colName];
                if (colManifest != null)
                {
                    // enhance Context with dnn user
                    if (colManifest.CreateByUser && model["Context"] != null)
                    {
                        var dnnUser = UserController.GetUserById(_portalId, createdByUserId);
                        if (dnnUser != null)
                        {
                            var user = new JObject();
                            user["DisplayName"] = dnnUser.DisplayName;
                            user["FirstName"] = dnnUser.FirstName;
                            user["LastName"] = dnnUser.LastName;
                            user["Email"] = dnnUser.Email;
                            model["Context"]["CreatedByUser"] = user;
                        }
                    }
                }
            }
        }
        private void EnhanceImages(JObject model, JObject itemsModel)
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

        private static void RemoveNoData(JObject model)
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
    }
}