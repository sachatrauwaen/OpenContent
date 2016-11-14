using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;


namespace Satrabel.OpenContent.Components.Form
{
    public class FormUtils
    {
        public static MailAddress GenerateMailAddress(string TypeOfAddress, string Email, string Name, string FormEmailField, string FormNameField, JObject form)
        {
            MailAddress adr = null;
            var portalSettings = PortalSettings.Current;
            var userInfo = portalSettings.UserInfo;

            if (TypeOfAddress == "host")
            {
                adr = new MailAddress(Host.HostEmail, Host.HostTitle);
            }
            else if (TypeOfAddress == "admin")
            {
                var user = UserController.GetUserById(portalSettings.PortalId, portalSettings.AdministratorId);
                adr = new MailAddress(user.Email, user.DisplayName);
            }
            else if (TypeOfAddress == "form")
            {
                if (string.IsNullOrEmpty(FormNameField))
                    FormNameField = "name";
                if (string.IsNullOrEmpty(FormEmailField))
                    FormEmailField = "email";

                string FormEmail = GetProperty(form, FormEmailField);
                string FormName = GetProperty(form, FormNameField);
                adr = new MailAddress(FormEmail, FormName);
            }
            else if (TypeOfAddress == "custom")
            {
                adr = new MailAddress(Email, Name);
            }
            else if (TypeOfAddress == "current")
            {
                if (userInfo == null)
                    throw new Exception(string.Format("Can't send email to current user, as there is no current user. Parameters were TypeOfAddress: [{0}], Email: [{1}], Name: [{2}], FormEmailField: [{3}], FormNameField: [{4}], FormNameField: [{5}]", TypeOfAddress, Email, Name, FormEmailField, FormNameField, form));
                if (string.IsNullOrEmpty(userInfo.Email))
                    throw new Exception(string.Format("Can't send email to current user, as email address of current user is unknown. Parameters were TypeOfAddress: [{0}], Email: [{1}], Name: [{2}], FormEmailField: [{3}], FormNameField: [{4}], FormNameField: [{5}]", TypeOfAddress, Email, Name, FormEmailField, FormNameField, form));

                adr = new MailAddress(userInfo.Email, userInfo.DisplayName);
            }

            if (adr == null)
            {
                throw new Exception(string.Format("Can't determine email address. Parameters were TypeOfAddress: [{0}], Email: [{1}], Name: [{2}], FormEmailField: [{3}], FormNameField: [{4}], FormNameField: [{5}]", TypeOfAddress, Email, Name, FormEmailField, FormNameField, form));
            }

            return adr;
        }
        private static string GetProperty(JObject obj, string PropertyName)
        {
            string PropertyValue = "";
            var Property = obj.Children<JProperty>().SingleOrDefault(p => p.Name.ToLower() == PropertyName.ToLower());
            if (Property != null)
            {
                PropertyValue = Property.Value.ToString();
            }
            return PropertyValue;
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
            bool smtpEnableSSL = Host.EnableSMTPSSL;

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
                            smtpEnableSSL);

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
        public static void InitFields(JObject schemaJson, UserInfo userInfo)
        {
            if (schemaJson["properties"] != null && schemaJson["properties"]["Username"] != null)
            {
                schemaJson["properties"]["Username"]["default"] = userInfo.Username;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["FirstName"] != null)
            {
                schemaJson["properties"]["FirstName"]["default"] = userInfo.FirstName;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["LastName"] != null)
            {
                schemaJson["properties"]["LastName"]["default"] = userInfo.LastName;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["Email"] != null)
            {
                schemaJson["properties"]["Email"]["default"] = userInfo.Email;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["DisplayName"] != null)
            {
                schemaJson["properties"]["DisplayName"]["default"] = userInfo.DisplayName;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["Telephone"] != null)
            {
                schemaJson["properties"]["Telephone"]["default"] = userInfo.Profile.Telephone;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["Street"] != null)
            {
                schemaJson["properties"]["Street"]["default"] = userInfo.Profile.Street;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["City"] != null)
            {
                schemaJson["properties"]["City"]["default"] = userInfo.Profile.City;
            }
            if (schemaJson["properties"] != null && schemaJson["properties"]["Country"] != null)
            {
                schemaJson["properties"]["Country"]["default"] = userInfo.Profile.Country;
            }
        }
    }
}