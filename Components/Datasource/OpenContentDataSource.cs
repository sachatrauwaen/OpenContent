using DotNetNuke.Entities.Modules;
using DotNetNuke.Common.Utilities;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource.search;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Satrabel.OpenContent.Components.Lucene.Config;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Logging;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class OpenContentDataSource : IDataSource
    {
        public virtual string Name
        {
            get
            {
                return "OpenContent";
            }
        }

        #region Queries
        public bool Any(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController();
            return ctrl.GetFirstContent(context.ModuleId) != null;
        }
        public JArray GetVersions(DataSourceContext context, IDataItem item)
        {
            var content = (OpenContentInfo)item.Item;
            if (!string.IsNullOrEmpty(content.VersionsJson))
            {
                var verLst = new JArray();
                foreach (var version in content.Versions)
                {
                    var ver = new JObject();
                    ver["text"] = version.CreatedOnDate.ToShortDateString() + " " + version.CreatedOnDate.ToShortTimeString();
                    if (verLst.Count == 0) // first
                    {
                        ver["text"] = ver["text"] + " ( current )";
                    }
                    ver["ticks"] = version.CreatedOnDate.Ticks.ToString();
                    verLst.Add(ver);
                }
                return verLst;
            }
            return null;
        }

        public JToken GetVersion(DataSourceContext context, IDataItem item, DateTime datetime)
        {
            var content = (OpenContentInfo)item.Item;
            if (content != null)
            {
                if (!string.IsNullOrEmpty(content.VersionsJson))
                {
                    var ver = content.Versions.Single(v => v.CreatedOnDate == datetime);
                    if (ver != null)
                    {
                        return ver.Json;
                    }
                }
            }
            return null;
        }

        public IDataItem Get(DataSourceContext context, string id)
        {
            OpenContentController ctrl = new OpenContentController();
            OpenContentInfo content = null;

            if (!string.IsNullOrEmpty(id) && id != "-1")
            {
                LogContext.Log(context.ModuleId, "Get DataItem", "Request", string.Format("{0}.Get() with id {1}", Name, id));
                int idint;
                if (int.TryParse(id, out idint))
                {
                    content = ctrl.GetContent(idint);
                }
            }
            else
            {
                LogContext.Log(context.ModuleId, "Get DataItem", "Request", string.Format("{0}.Get() with id {1}. Returning first item of module.", Name, id));
                content = ctrl.GetFirstContent(context.ModuleId); // single item
            }
            if (content == null)
            {
                Log.Logger.WarnFormat("Item not shown because no content item found. Id [{0}]. Context ModuleId [{1}]", id, context.ModuleId);
                LogContext.Log(context.ModuleId, "Get DataItem", "Result", "not item found with id " + id);
            }
            else if (content.ModuleId == context.ModuleId)
            {
                var dataItem = new DefaultDataItem
                {
                    Id = content.ContentId.ToString(),
                    Data = content.JsonAsJToken,
                    CreatedByUserId = content.CreatedByUserId,
                    Item = content
                };
                LogContext.Log(context.ModuleId, "Get DataItem", "Result", dataItem);
                return dataItem;
            }
            else
            {
                LogContext.Log(context.ModuleId, "Get DataItem", "Result", string.Format("no item returned as incompatible module ids {0}-{1}", content.ModuleId, context.ModuleId));
            }
            return null;
        }

        public IDataItems GetAll(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController();

            var dataList = ctrl.GetContents(context.ModuleId).Select(c => new DefaultDataItem()
            {
                Id = c.ContentId.ToString(),
                Title = c.Title,
                Data = c.JsonAsJToken,
                CreatedByUserId = c.CreatedByUserId,
                Item = c
            });
            return new DefaultDataItems()
            {
                Items = dataList,
                Total = dataList.Count()
            };
        }
        public IDataItems GetAll(DataSourceContext context, Select select)
        {
            OpenContentController ctrl = new OpenContentController();
            if (select == null)
            {
                var dataList = ctrl.GetContents(context.ModuleId).Select(c => new DefaultDataItem()
                {
                    Id = c.ContentId.ToString(),
                    Title = c.Title,
                    Data = c.JsonAsJToken,
                    CreatedByUserId = c.CreatedByUserId,
                    Item = c
                });
                return new DefaultDataItems()
                {
                    Items = dataList,
                    Total = dataList.Count()
                };
            }
            else
            {
                SelectQueryDefinition def = new SelectQueryDefinition();
                def.Build(@select);
                if (LogContext.IsLogActive)
                {
                    var logKey = "Lucene query";
                    LogContext.Log(context.ModuleId, logKey, "Filter", def.Filter.ToString());
                    LogContext.Log(context.ModuleId, logKey, "Query", def.Query.ToString());
                    LogContext.Log(context.ModuleId, logKey, "Sort", def.Sort.ToString());
                    LogContext.Log(context.ModuleId, logKey, "PageIndex", def.PageIndex);
                    LogContext.Log(context.ModuleId, logKey, "PageSize", def.PageSize);
                }

                SearchResults docs = LuceneController.Instance.Search(context.ModuleId.ToString(), def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
                int total = docs.TotalResults;
                //Log.Logger.DebugFormat("OpenContent.JplistApiController.List() Searched for [{0}], found [{1}] items", select.ToJson(), total);
                //System.Diagnostics.Debug.WriteLine(select.ToJson());
                var dataList = new List<IDataItem>();
                foreach (var item in docs.ids)
                {
                    var content = ctrl.GetContent(int.Parse(item));
                    if (content != null)
                    {
                        dataList.Add(new DefaultDataItem
                        {
                            Id = content.ContentId.ToString(),
                            Data = content.JsonAsJToken,
                            CreatedByUserId = content.CreatedByUserId,
                            Item = content
                        });
                    }
                    else
                    {
                        Log.Logger.DebugFormat("OpenContent.JplistApiController.List() ContentItem not found [{0}]", item);
                    }
                }
                return new DefaultDataItems()
                {
                    Items = dataList,
                    Total = total,
                    DebugInfo = def.Filter.ToString() + " - " + def.Query.ToString() + " - " + def.Sort.ToString()
                };
            }
        }

        #region Edit

        public JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm();
        }

        /*
        public IDataItem GetEdit(DataSourceContext context, string id)
        {
            var dataItem = new DefaultDataItem();
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            dataItem.Data = fb.BuildForm();
            OpenContentInfo content = null;
            OpenContentController ctrl = new OpenContentController();
            if (!string.IsNullOrEmpty(id) && id != "-1")
            {
                content = ctrl.GetContent(int.Parse(id));
            }
            if (content != null)
            {
                dataItem.Id = content.ContentId.ToString();
                dataItem.Data["data"] = content.Json.ToJObject("GetContent " + id);
                //AddVersions(dataItem.Data as JObject, content);
                dataItem.CreatedByUserId = content.CreatedByUserId;
                dataItem.Item = content;
            }
            return dataItem;
        }
        public IDataItem GetFirstEdit(DataSourceContext context)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            JObject json = fb.BuildForm();
            OpenContentController ctrl = new OpenContentController();
            var content = ctrl.GetFirstContent(context.ModuleId);
            if (content != null)
            {
                var dataItem = new DefaultDataItem
                {
                    Id = content.ContentId.ToString(),
                    Data = json,
                    CreatedByUserId = content.CreatedByUserId,
                    Item = content
                };
                dataItem.Data["data"] = content.Json.ToJObject("GetFirstEdit");
                //AddVersions(json, content);

                return dataItem;
            }
            return null;
        }
        */
        #endregion

        #endregion

        #region Commands

        public void Add(DataSourceContext context, JToken data)
        {
            OpenContentController ctrl = new OpenContentController();
            var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder));
            var content = new OpenContentInfo()
            {
                ModuleId = context.ModuleId,
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
        public void Update(DataSourceContext context, IDataItem item, JToken data)
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
        public void Delete(DataSourceContext context, IDataItem item)
        {
            OpenContentController ctrl = new OpenContentController();
            var content = (OpenContentInfo)item.Item;
            ctrl.DeleteContent(content, context.Index);
        }

        public virtual void Action(DataSourceContext context, string action, IDataItem item, JToken data)
        {
            throw new NotImplementedException();
        }
 
        #endregion
    }
}