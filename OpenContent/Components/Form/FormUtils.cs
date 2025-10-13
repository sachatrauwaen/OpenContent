using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Mail;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Satrabel.OpenContent.Components.Form
{
    public static class FormUtils
    {
        public static MailAddress GenerateMailAddress(string typeOfAddress, string email, string name, string formEmailField, string formNameField, JObject form)
        {
            MailAddress adr = null;
            var portalSettings = PortalSettings.Current;

            if (typeOfAddress == "host")
            {
                adr = GenerateMailAddress(Host.HostEmail, Host.HostTitle);
            }
            else if (typeOfAddress == "admin")
            {
                var user = UserController.GetUserById(portalSettings.PortalId, portalSettings.AdministratorId);
                adr = GenerateMailAddress(user.Email, user.DisplayName);
            }
            else if (typeOfAddress == "form")
            {
                if (string.IsNullOrEmpty(formNameField))
                    formNameField = "name";
                if (string.IsNullOrEmpty(formEmailField))
                    formEmailField = "email";

                string formEmail = GetProperty(form, formEmailField);
                string formName = GetProperty(form, formNameField);
                adr = GenerateMailAddress(formEmail, formName);
            }
            else if (typeOfAddress == "custom")
            {
                adr = GenerateMailAddress(email, name);
            }
            else if (typeOfAddress == "current")
            {
                var userInfo = portalSettings.UserInfo;
                if (userInfo == null)
                    throw new Exception($"Can't send email to current user, as there is no current user. Parameters were TypeOfAddress: [{typeOfAddress}], Email: [{email}], Name: [{name}], FormEmailField: [{formEmailField}], FormNameField: [{formNameField}], FormNameField: [{form}]");

                adr = GenerateMailAddress(userInfo.Email, userInfo.DisplayName);
                if (adr == null)
                    throw new Exception($"Can't send email to current user, as email address of current user is unknown. Parameters were TypeOfAddress: [{typeOfAddress}], Email: [{email}], Name: [{name}], FormEmailField: [{formEmailField}], FormNameField: [{formNameField}], FormNameField: [{form}]");
            }

            if (adr == null)
            {
                throw new Exception($"Can't determine email address. Parameters were TypeOfAddress: [{typeOfAddress}], Email: [{email}], Name: [{name}], FormEmailField: [{formEmailField}], FormNameField: [{formNameField}], FormNameField: [{form}]");
            }

            return adr;
        }

        private static MailAddress GenerateMailAddress(string email, string title)
        {
            email = email.Trim(); //normalize email

            return IsValidEmail(email) ? new MailAddress(email, title) : null;
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

        /// <summary>
        /// Determines whether email is valid.
        /// </summary>
        /// <remarks>
        /// https://technet.microsoft.com/nl-be/library/01escwtf(v=vs.110).aspx
        /// </remarks>
        private static bool IsValidEmail(string strIn)
        {
            if (string.IsNullOrEmpty(strIn)) return false;

            bool invalid = false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper);
            }
            catch (Exception)
            {
                invalid = true;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                RegexOptions.IgnoreCase);
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();
            string domainName = match.Groups[2].Value;
            domainName = idn.GetAscii(domainName);
            return match.Groups[1].Value + domainName;
        }

        public static string SendMail(string mailFrom, string mailTo, string replyTo, string subject, string body)
        {
            return SendMail(mailFrom, mailTo, replyTo, "", "", subject, body);
        }
        public static string SendMail(string mailFrom, string mailTo, string replyTo, string cc, string bcc, string subject, string body, List<Attachment> attachments = null)
        {

            DotNetNuke.Services.Mail.MailPriority priority = DotNetNuke.Services.Mail.MailPriority.Normal;
            MailFormat bodyFormat = MailFormat.Html;
            Encoding bodyEncoding = Encoding.UTF8;

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
                    if (item.Name == "Files")
                    {
                        var files = item.Value as JArray;
                        foreach (var file in files)
                        {
                            formDataS.Append("<tr>").Append("<td>").Append("File").Append("</td>").Append("<td>").Append(" : ").Append("</td>").Append("<td><a href=\"").Append(HttpUtility.HtmlEncode(file["url"])).Append("\">").Append(file["name"]).Append("</a></td>").Append("</tr>");
                        }
                    }
                    else
                    {
                        formDataS.Append("<tr>").Append("<td>").Append(item.Name).Append("</td>").Append("<td>").Append(" : ").Append("</td>").Append("<td>").Append(HttpUtility.HtmlEncode(item.Value)).Append("</td>").Append("</tr>");
                    }
                    //formDataS.Append("<tr>").Append("<td>").Append(item.Name).Append("</td>").Append("<td>").Append(" : ").Append("</td>").Append("<td>").Append(item.Value).Append("</td>").Append("</tr>");
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

        public static JObject FormSubmit(JObject formInfo, JObject item = null)
        {
            if (formInfo["formSettings"] is JObject)
            {
                var settingsJson = (formInfo["formSettings"] as JObject).DeepClone();
                JsonUtils.SimplifyJson(settingsJson, DnnLanguageUtils.GetCurrentCultureCode());
                var settings = settingsJson.ToObject<SettingsDTO>();                
                var form = formInfo["form"] as JObject;
                return FormSubmit(form, settings, item);
            }
            else
            {
                var res = new JObject();
                res["message"] = "Form submited";
                return res;
            }
        }
        public static JObject FormSubmit(JObject form, SettingsDTO settings, JObject item = null)
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
                    if (item != null)
                    {
                        data.Item = JsonUtils.JsonToDynamic(item.ToString());
                    }
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
                            ProcessTemplates(hbs, data, notification);
                            MailAddress from = FormUtils.GenerateMailAddress(notification.From, notification.FromEmail, notification.FromName, notification.FromEmailField, notification.FromNameField, form);
                            MailAddress to = FormUtils.GenerateMailAddress(notification.To, notification.ToEmail, notification.ToName, notification.ToEmailField, notification.ToNameField, form);
                            MailAddress reply = null;
                            if (!string.IsNullOrEmpty(notification.ReplyTo))
                            {
                                reply = FormUtils.GenerateMailAddress(notification.ReplyTo, notification.ReplyToEmail, notification.ReplyToName, notification.ReplyToEmailField, notification.ReplyToNameField, form);
                            }
                            string body = formData;
                            string rawBody= string.Empty;
                            if (notification.EmailBodyType == "raw")
                            {
                                rawBody = notification.EmailRaw;
                            }
                            else
                            {
                                rawBody = notification.EmailBody;
                            }
                            if (!string.IsNullOrEmpty(rawBody))
                            {
                                try
                                {
                                    body = hbs.Execute(rawBody, data);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Email Body : " + ex.Message, ex);
                                }
                            }
                            string subject = notification.EmailSubject;
                            if (!string.IsNullOrEmpty(notification.EmailSubject))
                            {
                                try
                                {
                                    subject = hbs.Execute(notification.EmailSubject, data);
                                    subject = HttpUtility.HtmlDecode(subject);
                                    
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Email Subject : " + ex.Message, ex);
                                }
                            }
                            var attachements = new List<Attachment>();
                            if (form["Files"] is JArray)
                            {
                                foreach (var fileItem in form["Files"] as JArray)
                                {
                                    var file = FileManager.Instance.GetFile((int)fileItem["id"]);
                                    attachements.Add(new Attachment(FileManager.Instance.GetFileContent(file), fileItem["name"].ToString()));
                                }
                            }
                            string send = FormUtils.SendMail(from.ToString(), to.ToString(), reply?.ToString() ?? "", notification.CcEmails, notification.BccEmails, subject, body, attachements);
                            if (!string.IsNullOrEmpty(send))
                            {
                                errors.Add("From:" + from.ToString() + " - To:" + to.ToString() + " - " + send);
                            }
                        }
                        catch (Exception exc)
                        {
                            errors.Add("Error in Email Notification " + (settings.Notifications.IndexOf(notification) + 1) + " : " + exc.Message);
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
                        message = "Message sent.";
                    }
                    //Tracking = settings.Settings.Tracking;
                    if (!string.IsNullOrEmpty(settings.Settings.Tracking))
                    {
                        //res.RedirectUrl = Globals.NavigateURL(ActiveModule.TabID, "", "result=" + content.ContentId);
                    }
                }
                var res = new JObject();
                res["message"] = message;
                res["errors"] = new JArray(errors);
                return res;
            }
            return null;
        }

        private static void ProcessTemplates(HandlebarsEngine hbs, dynamic data, NotificationDTO notification)
        {
            if (notification.From == "custom")
            {
                if (!string.IsNullOrEmpty(notification.FromEmail) && notification.FromEmail.Contains("{{"))
                {
                    notification.FromEmail = hbs.Execute(notification.FromEmail, data);
                }
                if (!string.IsNullOrEmpty(notification.FromName) && notification.FromName.Contains("{{"))
                {
                    notification.FromName = hbs.Execute(notification.FromName, data);
                }
            }
            if (notification.To == "custom")
            {
                if (!string.IsNullOrEmpty(notification.ToEmail) && notification.ToEmail.Contains("{{"))
                {
                    notification.ToEmail = hbs.Execute(notification.ToEmail, data);
                }
                if (!string.IsNullOrEmpty(notification.ToName) && notification.ToName.Contains("{{"))
                {
                    notification.ToName = hbs.Execute(notification.ToName, data);
                }
            }
            if (notification.ReplyTo == "custom")
            {
                if (!string.IsNullOrEmpty(notification.ReplyToEmail) && notification.ReplyToEmail.Contains("{{"))
                {
                    notification.ReplyToEmail = hbs.Execute(notification.ReplyToEmail, data);
                }
                if (!string.IsNullOrEmpty(notification.ReplyToName) && notification.ReplyToName.Contains("{{"))
                {
                    notification.ReplyToName = hbs.Execute(notification.ReplyToName, data);
                }
            }
            if (!string.IsNullOrEmpty(notification.EmailSubject) && notification.EmailSubject.Contains("{{"))
            {
                notification.EmailSubject = hbs.Execute(notification.EmailSubject, data);
            }
        }

        public static string ToAbsoluteUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Insert(0, "~");
            if (!relativeUrl.StartsWith("~/"))
                relativeUrl = relativeUrl.Insert(0, "~/");

            var url = HttpContext.Current.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : String.Empty;

            return String.Format("{0}://{1}{2}{3}",
                url.Scheme, url.Host, port, VirtualPathUtility.ToAbsolute(relativeUrl));
        }
    }
}