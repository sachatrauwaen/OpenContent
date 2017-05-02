using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;


namespace Satrabel.OpenContent.Components.Form
{
    public class FormUtils
    {
        public static MailAddress GenerateMailAddress(string typeOfAddress, string email, string name, string formEmailField, string formNameField, JObject form)
        {
            MailAddress adr = null;
            var portalSettings = PortalSettings.Current;
            var userInfo = portalSettings.UserInfo;

            if (typeOfAddress == "host")
            {
                adr = new MailAddress(Host.HostEmail, Host.HostTitle);
            }
            else if (typeOfAddress == "admin")
            {
                var user = UserController.GetUserById(portalSettings.PortalId, portalSettings.AdministratorId);
                adr = new MailAddress(user.Email, user.DisplayName);
            }
            else if (typeOfAddress == "form")
            {
                if (string.IsNullOrEmpty(formNameField))
                    formNameField = "name";
                if (string.IsNullOrEmpty(formEmailField))
                    formEmailField = "email";

                string formEmail = GetProperty(form, formEmailField);
                string formName = GetProperty(form, formNameField);
                adr = new MailAddress(formEmail, formName);
            }
            else if (typeOfAddress == "custom")
            {
                adr = new MailAddress(email, name);
            }
            else if (typeOfAddress == "current")
            {
                if (userInfo == null)
                    throw new Exception($"Can't send email to current user, as there is no current user. Parameters were TypeOfAddress: [{typeOfAddress}], Email: [{email}], Name: [{name}], FormEmailField: [{formEmailField}], FormNameField: [{formNameField}], FormNameField: [{form}]");
                if (string.IsNullOrEmpty(userInfo.Email))
                    throw new Exception($"Can't send email to current user, as email address of current user is unknown. Parameters were TypeOfAddress: [{typeOfAddress}], Email: [{email}], Name: [{name}], FormEmailField: [{formEmailField}], FormNameField: [{formNameField}], FormNameField: [{form}]");

                adr = new MailAddress(userInfo.Email, userInfo.DisplayName);
            }

            if (adr == null)
            {
                throw new Exception($"Can't determine email address. Parameters were TypeOfAddress: [{typeOfAddress}], Email: [{email}], Name: [{name}], FormEmailField: [{formEmailField}], FormNameField: [{formNameField}], FormNameField: [{form}]");
            }

            return adr;
        }
        private static string GetProperty(JObject obj, string propertyName)
        {
            string propertyValue = "";
            var property = obj.Children<JProperty>().SingleOrDefault(p => p.Name.ToLower() == propertyName.ToLower());
            if (property != null)
            {
                propertyValue = property.Value.ToString();
            }
            return propertyValue;
        }
        public static string SendMail(string mailFrom, string mailTo, string replyTo, string subject, string body)
        {

            //string mailFrom
            //string mailTo, 
            string cc = "";
            string bcc = "";
            //string replyTo, 
            DotNetNuke.Services.Mail.MailPriority priority = DotNetNuke.Services.Mail.MailPriority.Normal;
            //string subject, 
            MailFormat bodyFormat = MailFormat.Html;
            Encoding bodyEncoding = Encoding.UTF8;
            //string body, 
            List<Attachment> attachments = new List<Attachment>();
            string smtpServer = Host.SMTPServer;
            string smtpAuthentication = Host.SMTPAuthentication;
            string smtpUsername = Host.SMTPUsername;
            string smtpPassword = Host.SMTPPassword;
            bool smtpEnableSsl = Host.EnableSMTPSSL;

            string res = Mail.SendMail(mailFrom,
                            mailTo,
                            cc,
                            bcc,
                            replyTo,
                            priority,
                            subject,
                            bodyFormat,
                            bodyEncoding,
                            body,
                            attachments,
                            smtpServer,
                            smtpAuthentication,
                            smtpUsername,
                            smtpPassword,
                            smtpEnableSsl);

            //Mail.SendEmail(replyTo, mailFrom, mailTo, subject, body);
            return res;
        }

        public static dynamic GenerateFormData(string form, out string formData)
        {
            dynamic data = null;
            formData = "";
            StringBuilder formDataS = new StringBuilder();
            if (form != null)
            {
                formDataS.Append("<table boder=\"1\">");
                foreach (var item in JObject.Parse(form).Properties())
                {
                    formDataS.Append("<tr>").Append("<td>").Append(item.Name).Append("</td>").Append("<td>").Append(" : ").Append("</td>").Append("<td>").Append(item.Value).Append("</td>").Append("</tr>");
                }
                formDataS.Append("</table>");
                data = JsonUtils.JsonToDynamic(form);
                data.FormData = formDataS.ToString();
                formData = formDataS.ToString();
            }
            return data;
        }

        public static JObject InitFields(JObject sourceJson, UserInfo userInfo)
        {
            var schemaJson = sourceJson.DeepClone() as JObject;  //make sure we do not modify the cached object

            if (schemaJson["properties"]?["Username"] != null)
            {
                schemaJson["properties"]["Username"]["default"] = userInfo.Username;
            }
            if (schemaJson["properties"]?["FirstName"] != null)
            {
                schemaJson["properties"]["FirstName"]["default"] = userInfo.FirstName;
            }
            if (schemaJson["properties"]?["LastName"] != null)
            {
                schemaJson["properties"]["LastName"]["default"] = userInfo.LastName;
            }
            if (schemaJson["properties"]?["Email"] != null)
            {
                schemaJson["properties"]["Email"]["default"] = userInfo.Email;
            }
            if (schemaJson["properties"]?["DisplayName"] != null)
            {
                schemaJson["properties"]["DisplayName"]["default"] = userInfo.DisplayName;
            }
            if (schemaJson["properties"]?["Telephone"] != null)
            {
                schemaJson["properties"]["Telephone"]["default"] = userInfo.Profile.Telephone;
            }
            if (schemaJson["properties"]?["Street"] != null)
            {
                schemaJson["properties"]["Street"]["default"] = userInfo.Profile.Street;
            }
            if (schemaJson["properties"]?["City"] != null)
            {
                schemaJson["properties"]["City"]["default"] = userInfo.Profile.City;
            }
            if (schemaJson["properties"]?["Country"] != null)
            {
                schemaJson["properties"]["Country"]["default"] = userInfo.Profile.Country;
            }
            return schemaJson;
        }

        public static JObject FormSubmit(JObject formInfo)
        {
            if (formInfo["formSettings"] is JObject)
            {
                var settings = (formInfo["formSettings"] as JObject).ToObject<SettingsDTO>();
                var form = formInfo["form"] as JObject;
                return FormSubmit(form, settings);
            }
            else
            {
                var res = new JObject();
                res["message"] = "Form submited";
                return res;
            }
        }
        public static JObject FormSubmit(JObject form, SettingsDTO settings)
        {
            if (form != null)
            {
                HandlebarsEngine hbs = new HandlebarsEngine();
                dynamic data = null;
                string formData = "";
                if (form != null)
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
                    data = FormUtils.GenerateFormData(form.ToString(), out formData);
                }
                // Send emails
                string message = "Form submitted.";
                var errors = new List<string>();
                if (settings?.Notifications != null)
                {
                    foreach (var notification in settings.Notifications)
                    {
                        try
                        {
                            MailAddress from = FormUtils.GenerateMailAddress(notification.From, notification.FromEmail, notification.FromName, notification.FromEmailField, notification.FromNameField, form);
                            MailAddress to = FormUtils.GenerateMailAddress(notification.To, notification.ToEmail, notification.ToName, notification.ToEmailField, notification.ToNameField, form);
                            MailAddress reply = null;
                            if (!string.IsNullOrEmpty(notification.ReplyTo))
                            {
                                reply = FormUtils.GenerateMailAddress(notification.ReplyTo, notification.ReplyToEmail, notification.ReplyToName, notification.ReplyToEmailField, notification.ReplyToNameField, form);
                            }
                            string body = formData;
                            if (!string.IsNullOrEmpty(notification.EmailBody))
                            {
                                body = hbs.Execute(notification.EmailBody, data);
                            }

                            string send = FormUtils.SendMail(from.ToString(), to.ToString(), reply?.ToString() ?? "", notification.EmailSubject, body);
                            if (!string.IsNullOrEmpty(send))
                            {
                                errors.Add("From:" + from.ToString() + " - To:" + to.ToString() + " - " + send);
                            }
                        }
                        catch (Exception exc)
                        {
                            errors.Add("Notification " + (settings.Notifications.IndexOf(notification) + 1) + " : " + exc.Message);
                            App.Services.Logger.Error(exc);
                        }
                    }
                }
                if (settings?.Settings != null)
                {
                    if (!string.IsNullOrEmpty(settings.Settings.Message))
                    {
                        message = hbs.Execute(settings.Settings.Message, data);
                    }
                    else
                    {
                        message = "Message sended.";
                    }
                    //Tracking = settings.Settings.Tracking;
                    if (!string.IsNullOrEmpty(settings.Settings.Tracking))
                    {
                        //res.RedirectUrl = Globals.NavigateURL(ActiveModule.TabID, "", "result=" + content.ContentId);
                    }
                }
                var res = new JObject();
                res["message"] = message;
                return res;
            }
            return null;
        }
    }
}