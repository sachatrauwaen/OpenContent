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
using System.Web;

namespace Satrabel.OpenContent.Components.Render
{
    public class ModelFactorySingle : ModelFactoryBase
    {
        private readonly JToken _dataJson;
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
        public bool Detail { get; set; }

        public override JToken GetModelAsJson(bool onlyData = false, bool onlyMainData = false)
        {
            var model = _dataJson as JObject;
            var enhancedModel = new JObject();
            ExtendSchemaOptions(enhancedModel, onlyData || onlyMainData);
            ExtendModel(enhancedModel, onlyData, onlyMainData);
            ExtendModelSingle(enhancedModel);
            EnhanceSelect2(model, onlyData);
            JsonUtils.Merge(model, enhancedModel);
            JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            return model;
        }

        private void ExtendModelSingle(JObject model)
        {
            if (_data != null)
            {
                var context = model["Context"];
                if (Detail)
                {
                    context["DetailUrl"] = GenerateDetailUrl(_data, model, _manifest, _detailTabId);
                    context["Id"] = _data.Id;
                    var editIsAllowed = !_manifest.DisableEdit && IsEditAllowed(_data.CreatedByUserId);
                    context["EditUrl"] = editIsAllowed ? DnnUrlUtils.EditUrl("id", _data.Id, _module.ViewModule.ModuleID, _portalSettings) : "";
                }
            }
        }
    }
}