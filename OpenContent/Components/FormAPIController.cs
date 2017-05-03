using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Form;
using Satrabel.OpenContent.Components.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class FormAPIController : DnnApiController
    {
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Form(string key)
        {
            //string template = (string)ActiveModule.ModuleSettings["template"];

            JObject json = new JObject();
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                if (settings.TemplateAvailable)
                {
                    var formBuilder = new FormBuilder(settings.TemplateDir);
                    json = formBuilder.BuildForm(key);

                    if (UserInfo.UserID > 0 && json["schema"] is JObject)
                    {
                        json["schema"] = FormUtils.InitFields(json["schema"] as JObject, UserInfo);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                LoggingUtils.ProcessApiLoadException(this, exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Submit(SubmitDTO req)
        {
            try
            {
                var data = new JObject();
                data["form"] = req.form;
                string jsonSettings = ActiveModule.ModuleSettings["formsettings"] as string;
                if (!string.IsNullOrEmpty(jsonSettings))
                {
                    data["formSettings"] = JObject.Parse(jsonSettings);
                }
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                //var source = req.form["Source"].ToString();
                var dsItem = ds.Get(dsContext, req.id);
                var res = ds.Action(dsContext, string.IsNullOrEmpty(req.action) ? "FormSubmit" : req.action, dsItem, data);
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

    }

    public class SubmitDTO
    {
        public JObject form { get; set; }
        public string id { get; set; }
        public string action { get; set; }
    }
}