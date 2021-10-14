using DotNetNuke.Services.Mail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Form;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class RestApiDataSource : OpenContentDataSource
    {

        public override string Name
        {
            get
            {
                return "Satrabel.RestApi";
            }
        }
        public override IDataItems GetAll(DataSourceContext context)
        {
            JArray items = new JArray();

            var url = context.Config["listUrl"].ToString();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult(); ;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync();
                var content = responseBody.GetAwaiter().GetResult();
                items = JArray.Parse(content);
            }
            var dataList = items
                .Select(content => CreateDefaultDataItem(content));

            return new DefaultDataItems()
            {
                Items = dataList,
                Total = dataList.Count()
            };
        }

        public override IDataItems GetAll(DataSourceContext context, Select selectQuery)
        {
            if (selectQuery == null)
            {
                return GetAll(context);
            }
            else
            {
                string query = "";
                foreach (var f in selectQuery.Filter.FilterRules)
                {
                    if (f.Value != null)
                    {
                        if (string.IsNullOrEmpty(query))
                            query += "?";
                        else
                            query += "&";

                        query += f.Field + "=" + f.Value.AsString;
                    }
                }

                if (string.IsNullOrEmpty(query))
                    query += "?";
                else
                    query += "&";
                query += "PageIndex=" + selectQuery.PageIndex;

                if (string.IsNullOrEmpty(query))
                    query += "?";
                else
                    query += "&";
                query += "PageSize=" + selectQuery.PageSize;

                JArray items = new JArray();

                var url = context.Config["listUrl"].ToString()+query;

                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(url).GetAwaiter().GetResult(); ;
                    response.EnsureSuccessStatusCode();
                    var responseBody = response.Content.ReadAsStringAsync();
                    var content = responseBody.GetAwaiter().GetResult();
                    items = JArray.Parse(content);
                }
                var dataList = items
                    .Select(content => CreateDefaultDataItem(content));

                return new DefaultDataItems()
                {
                    Items = dataList,
                    Total = dataList.Count()
                };


                //var ruleCategory = selectQuery.Filter.FilterRules.FirstOrDefault(f => f.Field == "Category");
                //if (ruleCategory != null)
                //{
                //    string category = ruleCategory.Value.AsString;

                //}
            }
        }

        public override IDataItem Get(DataSourceContext context, string id)
        {
            JObject item = new JObject();
            var url = context.Config["detailUrl"].ToString();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(string.Format(url, id)).GetAwaiter().GetResult(); ;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync();
                var content = responseBody.GetAwaiter().GetResult();
                item = JObject.Parse(content);
            }

            if (item == null)
            {
                App.Services.Logger.Warn($"Item not shown because no content item found. Id [{id}]. url : [{url}], Id: [{id}]");
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", "not item found with id " + id);
            }
            else
            {
                var dataItem = CreateDefaultDataItem(item);
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", dataItem.Data);
                }
                return dataItem;
            }
            return null;
        }

        /// <summary>
        /// Gets additional/related data of a datasource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scope">The Scope. (portal, tab, module, tabmodule)</param>
        /// <param name="key">The unique key in the scope</param>
        /// <returns></returns>
        public override IDataItem GetData(DataSourceContext context, string scope, string key)
        {
            if (context.Config[key + "Url"] == null)
            {
                return base.GetData(context, scope, key);
            }

            JToken item = new JArray();
            var url = context.Config[key+"Url"].ToString();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(string.Format(url, key)).GetAwaiter().GetResult(); ;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync();
                var content = responseBody.GetAwaiter().GetResult();
                item = JToken.Parse(content);
            }
            if (item != null)
            {
                var dataItem = new DefaultDataItem()
                {
                    Data = item,
                    CreatedByUserId = 1, 
                    Item = null
                };
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get Data", key, dataItem.Data);
                }
                return dataItem;
            }
            return null;
        }
        private static DefaultDataItem CreateDefaultDataItem(JToken content)
        {
            return new DefaultDataItem
            {
                Id = content["id"].ToString(),
                Key = content["id"].ToString(),
                Collection = "Items",
                Title = content["title"]?.ToString(),
                Data = content,
                CreatedByUserId = 1,
                LastModifiedByUserId = 1,
                LastModifiedOnDate = DateTime.Now,
                CreatedOnDate = DateTime.Now,
                Item = null
            };
        }

    }



}