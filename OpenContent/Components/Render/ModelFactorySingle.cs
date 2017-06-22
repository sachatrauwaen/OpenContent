using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
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

        public ModelFactorySingle(JToken dataJson, string settingsJson, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module) :
            base(settingsJson, manifest, templateManifest, templateFiles, module)
        {
            this._dataJson = dataJson;
        }

        public ModelFactorySingle(IDataItem data, string settingsJson, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleConfig module) :
            base(settingsJson, manifest, templateManifest, templateFiles, module)
        {
            this._dataJson = data.Data;
            this._data = data;
        }
        public ModelFactorySingle(IDataItem data, OpenContentModuleConfig module, string collection) :
            base(module, collection)
        {
            this._dataJson = data.Data;
            this._data = data;
        }
        public bool Detail { get; set; }

        public override JToken GetModelAsJson(bool onlyData = false, bool onlyMainData = false)
        {
            var model = _dataJson as JObject;
            if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
            {
                JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            }
            var enhancedModel = new JObject();
            ExtendSchemaOptions(enhancedModel, onlyData || onlyMainData);
            ExtendModel(enhancedModel, onlyData || onlyMainData);
            ExtendModelSingle(enhancedModel);
            EnhanceSelect2(model);
            JsonUtils.Merge(model, enhancedModel);
            return model;
        }

        private void ExtendModelSingle(JObject model)
        {
            if (_data != null)
            {
                var context = model["Context"];
                if (Detail)
                {
                    string url = "";
                    if (!string.IsNullOrEmpty(_manifest?.DetailUrl))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        var dynForHBS = JsonUtils.JsonToDictionary(model.ToString());
                        url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                        url = HttpUtility.HtmlDecode(url);
                    }
                    context["DetailUrl"] = _module.GetUrl(_detailTabId, url.CleanupUrl(), "id=" + _data.Id);
                    context["Id"] = _data.Id;
                    var editIsAllowed = !_manifest.DisableEdit && IsEditAllowed(_data.CreatedByUserId);
                    context["EditUrl"] = editIsAllowed ? _module.EditUrl("id", _data.Id, _module.ViewModule.ModuleId) : "";
                }
            }
        }
    }
}