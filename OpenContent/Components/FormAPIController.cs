using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Documents;
using Satrabel.OpenContent.Components.Form;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class FormAPIController : DnnApiController
    {
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Form()
        {
            string template = (string)ActiveModule.ModuleSettings["template"];

            JObject json = new JObject();
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                if (settings.TemplateAvailable)
                {
                    var formBuilder = new FormBuilder(settings.TemplateDir);
                    json = formBuilder.BuildForm("form");

                    if (UserInfo.UserID > 0 && json["schema"] is JObject)
                    {
                        FormUtils.InitFields((JObject)json["schema"], UserInfo);
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
                var module = new OpenContentModuleInfo(ActiveModule);
                Manifest.Manifest manifest = module.Settings.Manifest;
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                //var source = req.form["Source"].ToString();
                var dsItem = ds.Get(dsContext, req.id);
                var res = ds.Action(dsContext, "FormSubmit", dsItem, req.form);
                return Request.CreateResponse(HttpStatusCode.OK, res);


                int moduleId = ActiveModule.ModuleID;

                var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.TemplateDir, "Submissions");

                OpenContentController ctrl = new OpenContentController();
                var content = new OpenContentInfo()
                {
                    ModuleId = moduleId,
                    Collection = "Submissions",
                    Title = "Form",
                    Json = req.form.ToString(),
                    CreatedByUserId = UserInfo.UserID,
                    CreatedOnDate = DateTime.Now,
                    LastModifiedByUserId = UserInfo.UserID,
                    LastModifiedOnDate = DateTime.Now
                };
                ctrl.AddContent(content, module.Settings.Manifest.Index, indexConfig);
                string Message = "Form submitted.";
                var Errors = new List<string>();

                string jsonSettings = ActiveModule.ModuleSettings["formsettings"] as string;
                if (!string.IsNullOrEmpty(jsonSettings))
                {
                    SettingsDTO settings = JsonConvert.DeserializeObject<SettingsDTO>(jsonSettings);
                    HandlebarsEngine hbs = new HandlebarsEngine();
                    dynamic data = null;
                    string formData = "";
                    if (req.form != null)
                    {
                        /*
                        if (!string.IsNullOrEmpty(settings.Settings.SiteKey))
                        {
                            Recaptcha recaptcha = new Recaptcha(settings.Settings.SiteKey, settings.Settings.SecretKey);
                            RecaptchaValidationResult validationResult = recaptcha.Validate(form["recaptcha"].ToString());
                            if (!validationResult.Succeeded)
                            {
                                return Request.CreateResponse(HttpStatusCode.Forbidden);
                            }
                            form.Remove("recaptcha");
                        }
                         */
                        data = FormUtils.GenerateFormData(req.form.ToString(), out formData);
                    }

                    if (settings != null && settings.Notifications != null)
                    {
                        foreach (var notification in settings.Notifications)
                        {
                            try
                            {
                                MailAddress from = FormUtils.GenerateMailAddress(notification.From, notification.FromEmail, notification.FromName, notification.FromEmailField, notification.FromNameField, req.form);
                                MailAddress to = FormUtils.GenerateMailAddress(notification.To, notification.ToEmail, notification.ToName, notification.ToEmailField, notification.ToNameField, req.form);
                                MailAddress reply = null;
                                if (!string.IsNullOrEmpty(notification.ReplyTo))
                                {
                                    reply = FormUtils.GenerateMailAddress(notification.ReplyTo, notification.ReplyToEmail, notification.ReplyToName, notification.ReplyToEmailField, notification.ReplyToNameField, req.form);
                                }
                                string body = formData;
                                if (!string.IsNullOrEmpty(notification.EmailBody))
                                {
                                    body = hbs.Execute(notification.EmailBody, data);
                                }

                                string send = FormUtils.SendMail(from.ToString(), to.ToString(), (reply == null ? "" : reply.ToString()), notification.EmailSubject, body);
                                if (!string.IsNullOrEmpty(send))
                                {
                                    Errors.Add("From:" + from.ToString() + " - To:" + to.ToString() + " - " + send);
                                }
                            }
                            catch (Exception exc)
                            {
                                Errors.Add("Notification " + (settings.Notifications.IndexOf(notification) + 1) + " : " + exc.Message + " - " + (UserInfo.IsSuperUser ? exc.StackTrace : ""));
                                Log.Logger.Error(exc);
                            }
                        }
                    }
                    if (settings != null && settings.Settings != null)
                    {
                        if (!string.IsNullOrEmpty(settings.Settings.Message))
                        {
                            Message = hbs.Execute(settings.Settings.Message, data);
                        }
                        else
                        {
                            Message = "Message sended.";
                        }
                        //Tracking = settings.Settings.Tracking;
                        if (!string.IsNullOrEmpty(settings.Settings.Tracking))
                        {
                            //res.RedirectUrl = Globals.NavigateURL(ActiveModule.TabID, "", "result=" + content.ContentId);
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Message = Message,
                    Errors = Errors
                });
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }

    public class SubmitDTO
    {
        public JObject form { get; set; }
        public string id { get; set; }
    }
}