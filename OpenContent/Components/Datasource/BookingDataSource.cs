using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Documents;
using Satrabel.OpenContent.Components.Form;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class BookingDataSource : OpenContentDataSource
    {

        public override string Name
        {
            get
            {
                return "Satrabel.Booking";
            }
        }
        public override IDataItem Get(DataSourceContext context, string id)
        {
            var dataItem = base.Get(context, id);
            if (context.Collection == "Items") // Events
            {
                AfterGetEvent(context, dataItem);
            }
            return dataItem;
        }
        public override IDataItems GetAll(DataSourceContext context, Select selectQuery)
        {
            var res = base.GetAll(context, selectQuery);
            if (context.Collection == "Items") // Events
            {
                foreach (var item in res.Items)
                {
                    AfterGetEvent(context, item);
                }
            }
            return res;
        }
        public override JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var alpaca = base.GetAlpaca(context, schema, options, view);
            if (context.Collection == "Submissions" && schema)
            {
                var possibleQuatities = new JArray();
                possibleQuatities.Add("0"); //todo
                alpaca["schema"]["properties"]["Quantity"]["enum"] = possibleQuatities;
            }
            return alpaca;
        }
        public override JToken Action(DataSourceContext context, string action, IDataItem item, JToken data)
        {
            if (action == "FormSubmit")
            {
                var ev = item.Data.ToObject<EventDTO>();
                FormSubmit(context, data as JObject, ev);
                AfterGetEvent(context, item);
                return item.Data;
            }
            return null;
        }

        public override void Update(DataSourceContext context, IDataItem item, JToken data)
        {
            if (context.Collection == "Submissions")
            {
                var oldBooking = item.Data.ToObject<BookingDTO>();
                var newBooking = data.ToObject<BookingDTO>();
                BeforeUpdateSubmission(context, oldBooking, newBooking);
            }
            base.Update(context, item, data);
        }

        private void AfterGetEvent(DataSourceContext context, IDataItem dataItem)
        {
            var ev = dataItem.Data.ToObject<EventDTO>();
             var bookingCount = GetBookingCount(context, dataItem.Key);
            var eventCat = Get<EventCategoryDTO>(context, ev.EventCategoryKey);
            var max = eventCat.Max;

            dataItem.Data["BookingMax"] = max-bookingCount;
            dataItem.Data["EventMax"] = max;
            dataItem.Data["BookingCount"] = bookingCount;
            dataItem.Data["EventCategory"] = ev.EventCategory;
        }

        private int GetBookingCount(DataSourceContext context, string eventKey)
        {
            OpenContentController ctrl = new OpenContentController();
            SelectQueryDefinition def = new SelectQueryDefinition();
            var select = new Select();
            select.Filter.AddRule(new FilterRule()
            {
                Field = "Source",
                FieldType = FieldTypeEnum.KEY,
                FieldOperator = OperatorEnum.EQUAL,
                Value = new StringRuleValue("Items/" + eventKey)
            });
            def.Build(select);
            SearchResults docs = LuceneController.Instance.Search(OpenContentInfo.GetScope(GetModuleId(context), "Submissions"), def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
            if (docs.ids.Length > 0)
                return ctrl.GetContents(docs.ids.Select(i => int.Parse(i)).ToArray()).Sum(c => c.JsonAsJToken.ToObject<BookingDTO>().Quantity);
            else
                return 0;
        }

        private EventCategoryDTO GetEventCategory(DataSourceContext context, string key)
        {
            OpenContentController ctrl = new OpenContentController();
            var item = ctrl.GetContent(context.ModuleId, "EventCategory", key);
            return item.JsonAsJToken.ToObject<EventCategoryDTO>();
        }

        private EventDTO GetEvent(DataSourceContext context, string key)
        {
            OpenContentController ctrl = new OpenContentController();
            var item = ctrl.GetContent(context.ModuleId, "EventCategory", key);
            return item.JsonAsJToken.ToObject<EventDTO>();
        }

        private T Get<T>(DataSourceContext context, string key)
        {
            var colName = typeof(T).Name.Replace("DTO", "");
            if (colName == "Event")
            {
                colName = "Items";
            }
            OpenContentController ctrl = new OpenContentController();
            var item = ctrl.GetContent(context.ModuleId, colName, key);
            if (item == null) return default(T);

            return item.JsonAsJToken.ToObject<T>();
        }

        private void BeforeUpdateSubmission(DataSourceContext context, BookingDTO oldBooking, BookingDTO newBooking)
        {
            if (oldBooking.Quantity != newBooking.Quantity)
            {
                int actualBookingCount = GetBookingCount(context, oldBooking.EventKey);
                int quantityChange = newBooking.Quantity - oldBooking.Quantity;
                var ev = Get<EventDTO>(context, oldBooking.EventKey);
                if (ev != null)
                {
                    var eventCat = Get<EventCategoryDTO>(context, ev.EventCategoryKey);
                    if (quantityChange > 0 && (actualBookingCount + quantityChange) > eventCat.Max)
                    {
                        throw new Exception("quantity change not alowed");
                    }
                    if (quantityChange != 0)
                    {

                       // send notifications

                    }
                }
            }
        }
        private void FormSubmit(DataSourceContext context, JObject form, EventDTO ev)
        {
            var colkey = ev.EventCategory.Split('/');
            OpenContentController ctrl = new OpenContentController();
            var evCat = ctrl.GetContent(context.ModuleId, colkey[0], colkey[1]).JsonAsJToken.ToObject<EventCategoryDTO>();

            var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder), "Submissions");
            var content = new OpenContentInfo()
            {
                ModuleId = context.ModuleId,
                Collection = "Submissions",
                Title = "Form",
                Json = form.ToString(),
                CreatedByUserId = context.UserId,
                CreatedOnDate = DateTime.Now,
                LastModifiedByUserId = context.UserId,
                LastModifiedOnDate = DateTime.Now
            };
            ctrl.AddContent(content, true, indexConfig);

            var settings = evCat.FormSettings;
            if (settings != null)
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
                string Message = "Form submitted.";
                var Errors = new List<string>();
                if (settings != null && settings.Notifications != null)
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

                            string send = FormUtils.SendMail(from.ToString(), to.ToString(), (reply == null ? "" : reply.ToString()), notification.EmailSubject, body);
                            if (!string.IsNullOrEmpty(send))
                            {
                                Errors.Add("From:" + from.ToString() + " - To:" + to.ToString() + " - " + send);
                            }
                        }
                        catch (Exception exc)
                        {
                            Errors.Add("Notification " + (settings.Notifications.IndexOf(notification) + 1) + " : " + exc.Message);
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
        }
    }

    public class EventDTO
    {
        public string EventCategory { get; set; }
        public string EventCategoryKey
        {
            get
            {
                return DocumentUtils.GetCollectionKey(EventCategory).Key;
            }
        }
    }

    public class BookingDTO
    {
        public string EventKey
        {
            get
            {
                return DocumentUtils.GetCollectionKey(Source).Key;
            }
        }
        public string Source { get; set; }
        public int Quantity { get; set; }
    }
    public class EventCategoryDTO
    {
        public SettingsDTO FormSettings { get; set; }
        public int Max { get; set; }
    }

}