using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Form;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class OpenContentDataSource : IDataSource, IDataIndex
    {
        public virtual string Name => App.Config.Opencontent;

        #region Queries
        public virtual bool Any(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController(context.PortalId);
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
                    DateTime VersionDate = GetVersionDate(version);
                    var ver = new JObject();
                    ver["text"] = VersionDate.ToShortDateString() + " " + VersionDate.ToShortTimeString();
                    if (verLst.Count == 0) // first
                    {
                        ver["text"] = ver["text"] + " ( current )";
                    }
                    ver["ticks"] = VersionDate.Ticks.ToString();
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
                    DateTime versionDate = GetVersionDate(version);
                    var ver = new JObject();
                    ver["text"] = versionDate.ToShortDateString() + " " + versionDate.ToShortTimeString();
                    if (verLst.Count == 0) // first
                    {
                        ver["text"] = ver["text"] + " ( current )";
                    }
                    ver["ticks"] = versionDate.Ticks.ToString();
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
                    var ver = content.Versions.Single(v => GetVersionDate(v) == datetime);
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
                    var ver = content.Versions.Single(v => GetVersionDate(v) == datetime);
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
            OpenContentController ctrl = new OpenContentController(context.PortalId);
            OpenContentInfo content = null;

            if (!string.IsNullOrEmpty(id) && id != "-1")
            {
                /*
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get DataItem", "Request", string.Format("{0}.Get() with id {1}", Name, id));
                }
                */
                content = ctrl.GetContent(GetModuleId(context), context.Collection, id);
            }
            else
            {
                /*
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get DataItem", "Request", string.Format("{0}.Get() with id {1}. Returning first item of module.", Name, id));
                }
                */
                content = ctrl.GetFirstContent(GetModuleId(context)); // single item
            }
            if (content == null)
            {
                App.Services.Logger.Warn($"Item not shown because no content item found. Id [{id}]. Context TabId: [{GetTabId(context)}], ModuleId: [{GetModuleId(context)}]");
                LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", "not item found with id " + id);
            }
            else if (content.ModuleId == GetModuleId(context) && content.Collection == context.Collection)
            {
                var dataItem = CreateDefaultDataItem(content);
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", dataItem.Data);
                }
                return dataItem;
            }
            else
            {
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get DataItem", "Result", $"no item returned as incompatible module ids {content.ModuleId}-{GetModuleId(context)}");
                }
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
            string scopeStorage = AdditionalDataUtils.GetScope(scope, context.PortalId, context.TabId, GetModuleId(context), context.TabModuleId);
            var dc = new AdditionalDataController();
            var json = dc.GetData(scopeStorage, key);
            if (json != null)
            {
                var dataItem = new DefaultDataItem("")
                {
                    Data = json.Json.ToJObject("GetContent " + scope + "/" + key),
                    CreatedByUserId = json.CreatedByUserId,
                    Item = json
                };
                if (LogContext.IsLogActive)
                {
                    LogContext.Log(context.ActiveModuleId, "Get Data", key, dataItem.Data);
                }
                return dataItem;
            }
            return null;
        }

        public virtual IDataItems GetAll(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController(context.PortalId);

            var dataList = ctrl.GetContents(GetModuleId(context), context.Collection)
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

            SelectQueryDefinition def = BuildQuery(context, selectQuery);
            OpenContentController ctrl = new OpenContentController(context.PortalId);
            SearchResults docs = LuceneController.Instance.Search(OpenContentInfo.GetScope(GetModuleId(context), context.Collection), def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);

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
                    App.Services.Logger.Debug($"OpenContentDataSource.GetAll() ContentItem not found [{item}]");
                }
            }
            return new DefaultDataItems()
            {
                Items = dataList,
                Total = total,
                DebugInfo = def.Filter + " - " + def.Query + " - " + def.Sort
            };
        }

        private static SelectQueryDefinition BuildQuery(DataSourceContext context, Select selectQuery)
        {
            SelectQueryDefinition def = new SelectQueryDefinition();
            def.Build(selectQuery, context.CurrentCultureCode);
            if (LogContext.IsLogActive)
            {
                var logKey = "Lucene query";
                LogContext.Log(context.ActiveModuleId, logKey, "Filter", def.Filter?.ToString());
                LogContext.Log(context.ActiveModuleId, logKey, "Query", def.Query?.ToString());
                LogContext.Log(context.ActiveModuleId, logKey, "Sort", def.Sort?.ToString());
                LogContext.Log(context.ActiveModuleId, logKey, "PageIndex", def.PageIndex);
                LogContext.Log(context.ActiveModuleId, logKey, "PageSize", def.PageSize);
            }

            return def;
        }

        #region Query Alpaca info for Edit

        public virtual JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm(context.Collection, context.CurrentCultureCode, schema, options, view);
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
            OpenContentController ctrl = new OpenContentController(context.PortalId);
            var content = new OpenContentInfo()
            {
                ModuleId = GetModuleId(context),
                Collection = context.Collection,
                Title = data["Title"]?.ToString() ?? "",
                Json = data.ToString(),
                CreatedByUserId = context.UserId,
                CreatedOnDate = DateTime.Now,
                LastModifiedByUserId = context.UserId,
                LastModifiedOnDate = DateTime.Now
            };
            ctrl.AddContent(content);
            context.Id = content.Id;

            //Index the content item
            if (context.Index)
            {
                var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder), context.Collection);
                LuceneController.Instance.Add(content, indexConfig);
                LuceneController.Instance.Commit();
            }
            ClearUrlRewriterCache(context);
            Notify(context, data, "add");
        }
        public virtual void Update(DataSourceContext context, IDataItem item, JToken data)
        {
            OpenContentController ctrl = new OpenContentController(context.PortalId);
            var content = (OpenContentInfo)item.Item;
            content.Title = data["Title"]?.ToString() ?? "";
            content.Json = data.ToString();
            content.LastModifiedByUserId = context.UserId;
            content.LastModifiedOnDate = DateTime.Now;
            ctrl.UpdateContent(content);
            if (context.Index)
            {
                var module = OpenContentModuleConfig.Create(ModuleController.Instance.GetModule(context.ModuleId, -1, false), new PortalSettings(context.PortalId));
                var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder), context.Collection);
                content.HydrateDefaultFields(indexConfig, module.Settings?.Manifest?.UsePublishTime ?? false);
                LuceneController.Instance.Update(content, indexConfig);
                LuceneController.Instance.Commit();
            }
            ClearUrlRewriterCache(context);
            Notify(context, data, "update");
        }

        private static void Notify(DataSourceContext context, JToken data, string action)
        {
            if (context.Options?["Notifications"] is JArray)
            {
                var notifData = new JObject();
                notifData["form"] = data.DeepClone();
                notifData["form"]["action"] = action;
                notifData["formSettings"] = new JObject();
                notifData["formSettings"] = context.Options;
                FormUtils.FormSubmit(notifData);
            }
        }

        public virtual void Delete(DataSourceContext context, IDataItem item)
        {
            OpenContentController ctrl = new OpenContentController(context.PortalId);
            var content = (OpenContentInfo)item.Item;
            ctrl.DeleteContent(content);
            if (context.Index)
            {
                LuceneController.Instance.Delete(content);
                LuceneController.Instance.Commit();
            }
            ClearUrlRewriterCache(context);
            Notify(context, content.JsonAsJToken, "delete");
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
            if (action == "FormSubmit")
            {
                if (data["form"]["approvalEnabled"] != null && data["form"]["approvalEnabled"].Value<bool>() == true)
                {
                    data["form"]["approved"] = false;
                }
                OpenContentController ctrl = new OpenContentController(context.PortalId);
                //var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder), "Submissions");
                var content = new OpenContentInfo()
                {
                    ModuleId = GetModuleId(context),
                    Collection = "Submissions",
                    Title = item?.Data["Title"] == null ? "Form" : item.Data["Title"].ToString(),
                    Json = data["form"].ToString(),
                    CreatedByUserId = context.UserId,
                    CreatedOnDate = DateTime.Now,
                    LastModifiedByUserId = context.UserId,
                    LastModifiedOnDate = DateTime.Now
                };
                ctrl.AddContent(content);

                //Index the content item

                if (context.Index)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder), "Submissions");
                    LuceneController.Instance.Add(content, indexConfig);
                    LuceneController.Instance.Commit();
                }
                return FormUtils.FormSubmit(data as JObject, item?.Data?.DeepClone() as JObject);
            }
            return null;
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
            var ctrl = new AdditionalDataController();
            var additionalData = (AdditionalDataInfo)item.Item;
            additionalData.Json = data.ToString();
            additionalData.LastModifiedByUserId = context.UserId;
            additionalData.LastModifiedOnDate = DateTime.Now;
            ctrl.UpdateData(additionalData);
            ClearUrlRewriterCache(context);
        }

        //public void Reindex(DataSourceContext context)
        //{
        //    string scope = OpenContentInfo.GetScope(context.ModuleId, context.Collection);
        //    var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder), context.Collection); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files
        //    OpenContentController occ = new OpenContentController();
        //    LuceneController.Instance.ReIndexData(occ.GetContents(context.ModuleId, context.Collection), indexConfig, scope);
        //}

        public IEnumerable<IIndexableItem> GetIndexableData(DataSourceContext context)
        {
            OpenContentController occ = new OpenContentController(context.PortalId);
            return occ.GetContents(context.ModuleId, context.Collection);
        }

        #endregion

        #region Private Methods

        protected static int GetModuleId(DataSourceContext context)
        {
            return context.Config.GetValue("ModuleId", context.ModuleId);
        }

        private static DateTime GetVersionDate(OpenContentVersion version)
        {
            return version.LastModifiedOnDate == null ? version.CreatedOnDate : version.LastModifiedOnDate;
        }

        private static int GetTabId(DataSourceContext context)
        {
            return context.Config.GetValue("TabId", context.TabId);
        }

        private static DefaultDataItem CreateDefaultDataItem(OpenContentInfo content)
        {
            return new DefaultDataItem(content.Id)
            {
                Collection = content.Collection,
                Title = content.Title,
                Data = content.JsonAsJToken,
                CreatedByUserId = content.CreatedByUserId,
                LastModifiedByUserId = content.LastModifiedByUserId,
                LastModifiedOnDate = content.LastModifiedOnDate,
                CreatedOnDate = content.CreatedOnDate,
                Item = content
            };
        }

        private static void ClearUrlRewriterCache(DataSourceContext context)
        {
            UrlRewriter.UrlRulesCaching.Remove(context.PortalId, context.ModuleId);
        }

        #endregion
    }
}