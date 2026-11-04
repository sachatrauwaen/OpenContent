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
        public ModelFactoryMultiple(IEnumerable<IDataItem> dataList, string settingsJson, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module) :
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
        /// Gets the model as dictionary list, used by Url Rewriter.
        /// Uses the full item (no Filter Settings Select projection) so detailUrl
        /// templates (e.g. {{PageInfo.Name}}) still resolve when list models are lean.
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
                    var model = CloneItemData(item);
                    if (model == null)
                    {
                        continue;
                    }
                    JObject context = new JObject();
                    model["Context"] = context;
                    context["Id"] = item.Id;
                    context["CreatedOnDate"] = item.CreatedOnDate.ToString("yyyy-MM-ddTHH:mm:ss");                    
                    JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
                    EnhanceSelect2(model, true, item.Key);
                    yield return JsonUtils.JsonToDictionary(model, SupportLargeLists);
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
                    JObject dyn = PrepareListItemJson(item);
                    if (dyn == null)
                    {
                        continue;
                    }
                    JObject context = new JObject();
                    dyn["Context"] = context;
                    context["Id"] = item.Id;
                    context["CreatedOnDate"] = item.CreatedOnDate.ToString("yyyy-MM-ddTHH:mm:ss");
                    JsonUtils.SimplifyJson(dyn, GetCurrentCultureCode());
                    EnhanceSelect2(dyn, onlyData, _collection);
                    EnhanceUser(dyn, item.CreatedByUserId);
                    EnhanceImages(dyn);
                    if (onlyData)
                    {
                        RemoveNoData(itemsModel);
                    }
                    else
                    {
                        if (_templateManifest != null)
                        {
                            var editStatus = !_manifest.DisableEdit && !_templateManifest.DisableEdit && IsEditAllowed(item.CreatedByUserId);
                            context["IsEditable"] = editStatus;
                        }
                        if (HasEditPermissions(item.CreatedByUserId))
                        {
                            context["EditUrl"] = _module.EditUrl("id", item.Id, _module.ViewModule.ModuleId);
                        }
                        // DetailUrl from full item — Select must not strip fields used by detailUrl.
                        // Only the resulting URL string is kept on the (possibly projected) list item.
                        context["DetailUrl"] = GenerateDetailUrl(item, PrepareDetailUrlItemJson(item) ?? dyn, _manifest, GetCurrentCultureCode(), _detailTabId);
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

        private static JObject CloneItemData(IDataItem item)
        {
            return item?.Data?.DeepClone() as JObject;
        }

        /// <summary>
        /// Clone item data and optionally project to Filter Settings <c>Select</c> fields
        /// for the list model (keeps large-list payloads under MaxLargeListJsonLength).
        /// Clones so projection never mutates the datasource cache.
        /// </summary>
        private JObject PrepareListItemJson(IDataItem item)
        {
            var model = CloneItemData(item);
            if (model == null)
            {
                return null;
            }
            var selectFields = QuerySelectFields;
            if (selectFields != null && selectFields.Count > 0)
            {
                model = JsonUtils.ProjectFields(model, selectFields);
            }
            return model;
        }

        /// <summary>
        /// Full item clone for detail URL generation (never projected by Select).
        /// </summary>
        private JObject PrepareDetailUrlItemJson(IDataItem item)
        {
            var model = CloneItemData(item);
            if (model == null)
            {
                return null;
            }
            JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            return model;
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