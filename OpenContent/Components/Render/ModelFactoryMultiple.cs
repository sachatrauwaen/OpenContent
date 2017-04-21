using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Render
{
    public class ModelFactoryMultiple : ModelFactoryBase
    {
        private readonly IEnumerable<IDataItem> _dataList = null;

        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, OpenContentModuleInfo module, PortalSettings portalSettings) :
            base(module, portalSettings)
        {
            this._dataList = dataList;
        }
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, OpenContentModuleInfo module, PortalSettings portalSettings, string collection) :
            base(module, portalSettings, collection)
        {
            this._dataList = dataList;
        }
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings) :
            base(settingsJson, physicalTemplateFolder, manifest, templateManifest, templateFiles, module, portalSettings)
        {
            this._dataList = dataList;
        }
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, int portalId, string cultureCode) :
            base(settingsJson, physicalTemplateFolder, manifest, templateManifest, templateFiles, module, portalId, cultureCode)
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
            ExtendModel(completeModel, true);
            if (_dataList != null)
            {
                foreach (var item in _dataList)
                {
                    var model = item.Data as JObject;
                    JObject context = new JObject();
                    model["Context"] = context;
                    context["Id"] = item.Id;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
                    }
                    EnhanceSelect2(model);
                    yield return JsonUtils.JsonToDictionary(model.ToString());
                }
            }
        }
        public override JToken GetModelAsJson(bool onlyData = false, bool onlyMainData = false)
        {
            if (_portalSettings == null) onlyData = true;
            JObject model = new JObject();
            var itemsModel = model;

            ExtendModel(model, onlyData || onlyMainData);
            ExtendSchemaOptions(itemsModel, onlyData || onlyMainData);
            ExtendItemsModel(itemsModel, onlyData || onlyMainData);

            if (!onlyData && !onlyMainData)
            {
                itemsModel["Context"]["RssUrl"] = _portalSettings.PortalAlias.HTTPAlias +
                       "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + _module.ViewModule.ModuleID + "&tabId=" + _detailTabId;
            }
            JArray items = new JArray(); ;
            itemsModel["Items"] = items;
            //string editRole = Manifest.GetEditRole();
            if (_dataList != null && _dataList.Any())
            {
                var mainUrl = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), "");
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
                    EnhanceSelect2(dyn);
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
                            context["EditUrl"] = DnnUrlUtils.EditUrl("id", item.Id, _module.ViewModule.ModuleID, _portalSettings);
                        }
                        context["DetailUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), UrlHelpers.CleanupUrl(url), "id=" + item.Id);
                        context["MainUrl"] = mainUrl;
                    }
                    items.Add(dyn);
                }
            }
            return model;
        }

        private void ExtendItemsModel(JObject model, bool onlyData)
        {
            if (_portalSettings == null) onlyData = true;

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
                context["AddUrl"] = DnnUrlUtils.EditUrl(_module.ViewModule.ModuleID, _portalSettings);
            }
        }

        private void EnhanceUser(JObject model, int createdByUserId)
        {
            string colName = string.IsNullOrEmpty(_collection) ? "Items" : _collection;
            if (_templateManifest != null && !string.IsNullOrEmpty(colName) && _templateFiles.Model != null && _templateFiles.Model.ContainsKey(colName))
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