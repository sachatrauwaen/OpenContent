using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Manifest;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class OpenContentDataSource : IDataSource
    {
        public virtual string Name
        {
            get
            {
                return AppConfig.OPENCONTENT;
            }
        }

        #region Queries
        public virtual bool Any(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController();
            return ctrl.GetFirstContent(GetModuleId(context)) != null;
        }
        public virtual JArray GetVersions(DataSourceContext context, IDataItem item)
        {
            var content = (OpenContentInfo)item.Item;
            if (!string.IsNullOrEmpty(content.VersionsJson))
            {
                var verLst = new JArray();
                foreach (var version in content.Versions)
                {
                    var ver = new JObject();
                    ver["text"] = version.LastModifiedOnDate.ToShortDateString() + " " + version.LastModifiedOnDate.ToShortTimeString();
                    if (verLst.Count == 0) // first
                    {
                        ver["text"] = ver["text"] + " ( current )";
                    }
                    ver["ticks"] = version.LastModifiedOnDate.Ticks.ToString();
                    verLst.Add(ver);
                }
                return verLst;
            }
            return null;
        }

        public virtual JToken GetDataVersions(DataSourceContext context, IDataItem item)
        {
            var content = (AdditionalDataInfo)item.Item;
            if (!string.IsNullOrEmpty(content.VersionsJson))
            {
                var verLst = new JArray();
                foreach (var version in content.Versions)
                {
                    var ver = new JObject();
                    ver["text"] = version.LastModifiedOnDate.ToShortDateString() + " " + version.LastModifiedOnDate.ToShortTimeString();
                    if (verLst.Count == 0) // first
                    {
                        ver["text"] = ver["text"] + " ( current )";
                    }
                    ver["ticks"] = version.LastModifiedOnDate.Ticks.ToString();
                    verLst.Add(ver);
                }
                return verLst;
            }
            return null;
        }

        public virtual JToken GetVersion(DataSourceContext context, IDataItem item, DateTime datetime)
        {
            var content = (OpenContentInfo)item.Item;
            if (content != null)
            {
                if (!string.IsNullOrEmpty(content.VersionsJson))
                {
                    var ver = content.Versions.Single(v => v.LastModifiedOnDate == datetime);
                    if (ver != null)
                    {
                        return ver.Json;
                    }
                }
            }
            return null;
        }

        public virtual JToken GetDataVersion(DataSourceContext context, IDataItem item, DateTime datetime)
        {
            var content = (AdditionalDataInfo)item.Item;
            if (content != null)
            {
                if (!string.IsNullOrEmpty(content.VersionsJson))
                {
                    var ver = content.Versions.Single(v => v.LastModifiedOnDate == datetime);
                    if (ver != null)
                    {
                        return ver.Json;
                    }
                }
            }
            return null;
        }

        public virtual IDataItem Get(DataSourceContext context, string id)
        {
            OpenContentController ctrl = new OpenContentController();
            OpenContentInfo content = null;

            if (!string.IsNullOrEmpty(id) && id != "-1")
            {
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Request", string.Format("{0}.Get() with id {1}", Name, id));
                int idint;
                if (int.TryParse(id, out idint))
                {
                    content = ctrl.GetContent(idint);
                }
            }
            else
            {
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Request", string.Format("{0}.Get() with id {1}. Returning first item of module.", Name, id));
                content = ctrl.GetFirstContent(GetModuleId(context)); // single item
            }
            if (content == null)
            {
                Log.Logger.WarnFormat("Item not shown because no content item found. Id [{0}]. Context TabId: [{1}], ModuleId: [{2}]", id, GetTabId(context), GetModuleId(context));
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", "not item found with id " + id);
            }
            else if (content.ModuleId == GetModuleId(context))
            {
                var dataItem = CreateDefaultDataItem(content);
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", dataItem);
                return dataItem;
            }
            else
            {
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", string.Format("no item returned as incompatible module ids {0}-{1}", content.ModuleId, GetModuleId(context)));
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
        public virtual IDataItem GetData(DataSourceContext context, string scope, string key)
        {
            var sourceRelatedData = context.SourceRelatedData();
            if (sourceRelatedData == RelatedDataSourceType.AdditionalData)
            {
                string scopeStorage = AdditionalDataUtils.GetScope(scope, context.PortalId, context.TabId, GetModuleId(context), context.TabModuleId);
                var dc = new AdditionalDataController();
                var json = dc.GetData(scopeStorage, key);
                if (json != null)
                {
                    var dataItem = new DefaultDataItem
                    {
                        Data = json.Json.ToJObject("GetContent " + scope + "/" + key),
                        CreatedByUserId = json.CreatedByUserId,
                        Item = json
                    };
                    LogContext.Log(context.ActiveModuleId, "Get Data", "Result", dataItem);
                    return dataItem;
                }
            }
            else
            {
                IDataItems dataItems = GetAll(context);

                JArray json = new JArray();
                foreach (var dataItem in dataItems.Items)
                {
                    var itemData = dataItem.Data;
                    if (itemData != null)
                    {
                        itemData["Id"] = dataItem.Id; //add the contentItem Id to the json   //ContentId
                        json.Add(itemData);
                    }
                }
                if (json != null)
                {
                    var dataItem = new DefaultDataItem
                    {
                        Data = json.ToJObject("GetContent " + "other module " + "/" + key),
                        Item = json
                    };
                    LogContext.Log(context.ActiveModuleId, "Get Data", "Result", dataItem);
                    return dataItem;
                }
            }
            return null;
        }

        public virtual IDataItems GetAll(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController();

            var dataList = ctrl.GetContents(GetModuleId(context))
                .OrderBy(i => i.CreatedOnDate)
                .Select(content => CreateDefaultDataItem(content));

            return new DefaultDataItems()
            {
                Items = dataList,
                Total = dataList.Count()
            };
        }

        public virtual IDataItems GetAll(DataSourceContext context, Select selectQuery)
        {
            if (selectQuery == null)
            {
                return GetAll(context);
            }
            else
            {
                OpenContentController ctrl = new OpenContentController();
                SelectQueryDefinition def = new SelectQueryDefinition();
                def.Build(selectQuery);
                if (LogContext.IsLogActive)
                {
                    var logKey = "Lucene query";
                    LogContext.Log(context.ActiveModuleId, logKey, "Filter", def.Filter.ToString());
                    LogContext.Log(context.ActiveModuleId, logKey, "Query", def.Query.ToString());
                    LogContext.Log(context.ActiveModuleId, logKey, "Sort", def.Sort.ToString());
                    LogContext.Log(context.ActiveModuleId, logKey, "PageIndex", def.PageIndex);
                    LogContext.Log(context.ActiveModuleId, logKey, "PageSize", def.PageSize);
                }

                SearchResults docs = LuceneController.Instance.Search(GetModuleId(context).ToString(), def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
                int total = docs.TotalResults;
                var dataList = new List<IDataItem>();
                foreach (string item in docs.ids)
                {
                    var content = ctrl.GetContent(int.Parse(item));
                    if (content != null)
                    {
                        dataList.Add(CreateDefaultDataItem(content));
                    }
                    else
                    {
                        Log.Logger.DebugFormat("OpenContentDataSource.GetAll() ContentItem not found [{0}]", item);
                    }
                }
                return new DefaultDataItems()
                {
                    Items = dataList,
                    Total = total,
                    DebugInfo = def.Filter + " - " + def.Query + " - " + def.Sort
                };
            }
        }

        #region Query Alpaca info for Edit

        public virtual JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm("", context.CurrentCultureCode);
        }

        // Additional Data
        public virtual JObject GetDataAlpaca(DataSourceContext context, bool schema, bool options, bool view, string key)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm(key, context.CurrentCultureCode);
        }

        #endregion

        #endregion

        #region Commands

        public virtual void Add(DataSourceContext context, JToken data)
        {
            OpenContentController ctrl = new OpenContentController();
            var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder));
            var content = new OpenContentInfo()
            {
                ModuleId = GetModuleId(context),
                Title = data["Title"] == null ? "" : data["Title"].ToString(),
                Json = data.ToString(),
                JsonAsJToken = data,
                CreatedByUserId = context.UserId,
                CreatedOnDate = DateTime.Now,
                LastModifiedByUserId = context.UserId,
                LastModifiedOnDate = DateTime.Now,
                Html = "",
            };
            ctrl.AddContent(content, context.Index, indexConfig);
        }
        public virtual void Update(DataSourceContext context, IDataItem item, JToken data)
        {
            OpenContentController ctrl = new OpenContentController();
            var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder));
            var content = (OpenContentInfo)item.Item;
            content.Title = data["Title"] == null ? "" : data["Title"].ToString();
            content.Json = data.ToString();
            content.JsonAsJToken = data;
            content.LastModifiedByUserId = context.UserId;
            content.LastModifiedOnDate = DateTime.Now;
            ctrl.UpdateContent(content, context.Index, indexConfig);
        }
        public virtual void Delete(DataSourceContext context, IDataItem item)
        {
            OpenContentController ctrl = new OpenContentController();
            var content = (OpenContentInfo)item.Item;
            ctrl.DeleteContent(content, context.Index);
        }

        /// <summary>
        /// Perform a particular action.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="action">The action.</param>
        /// <param name="item">The item to perform the action on.</param>
        /// <param name="data">The additional data/parameters needed to perform the Action.</param>
        /// <returns>Optionally return a JToken with a result value</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual JToken Action(DataSourceContext context, string action, IDataItem item, JToken data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds Related data (a.k.a Additional Data).
        /// Related data is data that is supportive to the Core data of datasource. Eg Categories, Enums, etc
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        public virtual void AddData(DataSourceContext context, string scope, string key, JToken data)
        {
            string scopeStorage = AdditionalDataUtils.GetScope(scope, context.PortalId, context.TabId, GetModuleId(context), context.TabModuleId);
            AdditionalDataController ctrl = new AdditionalDataController();
            var additionalData = new AdditionalDataInfo()
            {
                Scope = scopeStorage,
                DataKey = key,
                Json = data.ToString(),
                CreatedByUserId = context.UserId,
                CreatedOnDate = DateTime.Now,
                LastModifiedByUserId = context.UserId,
                LastModifiedOnDate = DateTime.Now,
            };
            ctrl.AddData(additionalData);
        }

        /// <summary>
        /// Updates the Related data (a.k.a Additional Data).
        /// Related data is data that is supportive to the Core data of datasource. Eg Categories, Enums, etc
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="item">The item.</param>
        /// <param name="data">The data.</param>
        public virtual void UpdateData(DataSourceContext context, IDataItem item, JToken data)
        {
            AdditionalDataController ctrl = new AdditionalDataController();
            var additionalData = (AdditionalDataInfo)item.Item;
            additionalData.Json = data.ToString();
            additionalData.LastModifiedByUserId = context.UserId;
            additionalData.LastModifiedOnDate = DateTime.Now;
            ctrl.UpdateData(additionalData);
        }

        #endregion

        #region Private Methods

        private static int GetModuleId(DataSourceContext context)
        {
            return context.Config?["ModuleId"]?.Value<int>() ?? context.ModuleId;
        }
        private static int GetTabId(DataSourceContext context)
        {
            return context.Config?["TabId"]?.Value<int>() ?? context.TabId;
        }
        private static DefaultDataItem CreateDefaultDataItem(OpenContentInfo content)
        {
            return new DefaultDataItem
            {
                Id = content.ContentId.ToString(),
                Title = content.Title,
                Data = content.JsonAsJToken,
                CreatedByUserId = content.CreatedByUserId,
                LastModifiedByUserId = content.LastModifiedByUserId,
                LastModifiedOnDate = content.LastModifiedOnDate,
                CreatedOnDate = content.CreatedOnDate,
                Item = content
            };
        }
        #endregion
    }
}