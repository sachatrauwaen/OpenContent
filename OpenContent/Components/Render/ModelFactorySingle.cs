using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Render
{
    public class ModelFactorySingle :ModelFactoryBase
    {
        private JToken _dataJson;
        private readonly IDataItem _data;

        public ModelFactorySingle(JToken dataJson, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings) :
            base(settingsJson, physicalTemplateFolder, manifest, templateManifest, templateFiles, module, portalSettings)
        {
            this._dataJson = dataJson;
        }

        public ModelFactorySingle(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings) :
            base(settingsJson, physicalTemplateFolder, manifest, templateManifest, templateFiles, module, portalSettings)
        {
            this._dataJson = data.Data;
            this._data = data;
        }
        public ModelFactorySingle(IDataItem data, OpenContentModuleInfo module, PortalSettings portalSettings, string collection) :
            base(module, portalSettings, collection)
        {
            this._dataJson = data.Data;
            this._data = data;
        }
        /*
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
        */
        public override JToken GetModelAsJson(bool onlyData = false)
        {
            var model = _dataJson as JObject;
            if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
            {
                JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            }
            var enhancedModel = new JObject();
            ExtendSchemaOptions(enhancedModel, onlyData);
            ExtendModel(enhancedModel, onlyData);
            ExtendModelSingle(enhancedModel, onlyData);
            EnhanceSelect2(model, enhancedModel);
            JsonUtils.Merge(model, enhancedModel);
            return model;
        }

        private void ExtendModelSingle(JObject model, bool onlyDataa)
        {
            if (_data != null)
            {
                var context = model["Context"];
                string url = "";
                if (!string.IsNullOrEmpty(_manifest?.DetailUrl))
                {
                    HandlebarsEngine hbEngine = new HandlebarsEngine();
                    dynamic dynForHBS = JsonUtils.JsonToDynamic(model.ToString());
                    url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                    url = HttpUtility.HtmlDecode(url);
                }
                context["DetailUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), UrlHelpers.CleanupUrl(url), "id=" + _data.Id);
                context["Id"] = _data.Id;
                var editIsAllowed = !_manifest.DisableEdit && IsEditAllowed(_data.CreatedByUserId);
                context["EditUrl"] = editIsAllowed ? DnnUrlUtils.EditUrl("id", _data.Id, _module.ViewModule.ModuleID, _portalSettings) : "";
            }
        }
    }
}