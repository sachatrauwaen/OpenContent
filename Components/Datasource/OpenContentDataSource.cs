using DotNetNuke.Entities.Modules;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class OpenContentDataSource : IDataSource
    {
        public string Name
        {
            get
            {
                return "OpenContent";
            }
        }
        public IDataItem GetEdit(DataSourceContext context, string id)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            JObject json = fb.BuildForm();
            var content = GetContent(context.ModuleId, id);
            if (content != null)
            {
                var dataItem = new DefaultDataItem();
                dataItem.Id = content.ContentId.ToString();
                dataItem.Data = json;
                dataItem.Data["data"] = content.Json.ToJObject("GetContent " + id);
                AddVersions(json, content);
                dataItem.CreatedByUserId = content.CreatedByUserId;
                dataItem.Item = content;
                return dataItem;
            }
            return null;
        }
        public IDataItem Get(DataSourceContext context, string id)
        {
            var content = GetContent(context.ModuleId, id);
            if (content != null && content.ModuleId == context.ModuleId)
            {
                var dataItem = new DefaultDataItem();
                dataItem.Id = content.ContentId.ToString();
                dataItem.Data = content.Json.ToJObject("GetContent " + id);
                dataItem.CreatedByUserId = content.CreatedByUserId;
                dataItem.Item = content;
                return dataItem;
            }
            return null;
        }
        public IDataItem GetFirst(DataSourceContext context)
        {
            return Get(context, null);
        }
        public IEnumerable<IDataItem> GetAll(DataSourceContext context)
        {
            OpenContentController ctrl = new OpenContentController();
            return ctrl.GetContents(context.ModuleId).Select(c => new DefaultDataItem() { 
                Id = c.ContentId.ToString(),
                Data = c.Json.ToJObject("GetContent " + c.ContentId),
                CreatedByUserId = c.CreatedByUserId,
                Item = c
            });
        }
        private OpenContentInfo GetContent(int moduleId, string id)
        {
            OpenContentController ctrl = new OpenContentController();
            if (!string.IsNullOrEmpty(id) && id != "-1")
            {
                return ctrl.GetContent(int.Parse(id));
            }
            else
            {
                return ctrl.GetFirstContent(moduleId);
            }
        }
        private static void AddVersions(JObject json, OpenContentInfo struc)
        {
            if (!string.IsNullOrEmpty(struc.VersionsJson))
            {
                var verLst = new JArray();
                foreach (var item in struc.Versions)
                {
                    var ver = new JObject();
                    ver["text"] = item.CreatedOnDate.ToShortDateString() + " " + item.CreatedOnDate.ToShortTimeString();
                    if (verLst.Count == 0) // first
                    {
                        ver["text"] = ver["text"] + " ( current )";
                    }
                    ver["ticks"] = item.CreatedOnDate.Ticks.ToString();
                    verLst.Add(ver);
                }
                json["versions"] = verLst;

                //json["versions"] = JArray.Parse(struc.VersionsJson);
            }
        }


        public void AddContent(DataSourceContext context, JObject data)
        {
            OpenContentController ctrl = new OpenContentController();
            var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder));
            var content = new OpenContentInfo()
            {
                ModuleId = context.ModuleId,
                Title = data["Title"] == null ? "" : data["Title"].ToString(),
                Json = data.ToString(),
                CreatedByUserId = context.UserId,
                CreatedOnDate = DateTime.Now,
                LastModifiedByUserId = context.UserId,
                LastModifiedOnDate = DateTime.Now,
                Html = "",
            };
            ctrl.AddContent(content, context.Index, indexConfig);
        }
        public void UpdateContent(DataSourceContext context, IDataItem item, JObject data)
        {
            OpenContentController ctrl = new OpenContentController();
            var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(context.TemplateFolder));
            var content = (OpenContentInfo)item.Item;
            content.Title = data["Title"] == null ? "" : data["Title"].ToString();
            content.Json = data.ToString();
            content.LastModifiedByUserId = context.UserId;
            content.LastModifiedOnDate = DateTime.Now;
            ctrl.UpdateContent(content, context.Index, indexConfig);
        }
    }
}