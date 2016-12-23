using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.Search;
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
                if (dataItem != null)
                    AfterGetEvent(context, dataItem);
            }
            else if (context.Collection == "Submissions") // Events
            {
                if (dataItem != null)
                    AfterGetSubmission(context, dataItem);
            }
            return dataItem;
        }

        private void AfterGetSubmission(DataSourceContext context, IDataItem dataItem)
        {
            var booking = dataItem.Data.ToObject<BookingDTO>();
            var bookingCount = GetBookingCount(context, booking.EventId);
            var ev = Get<EventDTO>(context, booking.EventId);
            if (ev != null)
            {
                var eventCat = Get<EventCategoryDTO>(context, ev.EventCategory);
                var max = eventCat.Max;
                dataItem.Data["BookingMax"] = max - bookingCount;
                dataItem.Data["EventMax"] = max;
                dataItem.Data["BookingCount"] = bookingCount;
                dataItem.Data["EventCategory"] = ev.EventCategory;
            }
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
                FormSubmit(context, data["form"] as JObject, ev);
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
            var bookingCount = GetBookingCount(context, dataItem.Id);
            var eventCat = Get<EventCategoryDTO>(context, ev.EventCategory);
            var max = eventCat.Max;

            dataItem.Data["BookingMax"] = max - bookingCount;
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
                Value = new StringRuleValue(eventKey)
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
                int actualBookingCount = GetBookingCount(context, oldBooking.EventId);
                int quantityChange = newBooking.Quantity - oldBooking.Quantity;
                var ev = Get<EventDTO>(context, oldBooking.EventId);
                if (ev != null)
                {
                    var eventCat = Get<EventCategoryDTO>(context, ev.EventCategory);
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
            //var colkey = ev.EventCategory.Split('/');
            OpenContentController ctrl = new OpenContentController();
            var evCat = ctrl.GetContent(context.ModuleId, "EventCategory", ev.EventCategory).JsonAsJToken.ToObject<EventCategoryDTO>();

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

            FormUtils.FormSubmit(form, evCat.FormSettings);
        }
    }

    public class EventDTO
    {
        public string EventCategory { get; set; }

    }

    public class BookingDTO
    {
        public string EventId
        {
            get
            {
                return Source;
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