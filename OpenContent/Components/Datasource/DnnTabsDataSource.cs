using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using System;
using System.Collections.Generic;
using System.Linq;
using Satrabel.OpenContent.Components.Querying.Search;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DnnTabsDataSource : DefaultDataSource
    {
        public override string Name
        {
            get
            {
                return "Satrabel.DnnTabs";
            }
        }
        public override IDataItem Get(DataSourceContext context, string id)
        {
            return GetAll(context, null).Items.SingleOrDefault(i => i.Id == id);
        }
        public override IDataItems GetAll(DataSourceContext context, Select selectQuery)
        {
            int AdminTabId = PortalSettings.Current.AdminTabId;
            var tabs = TabController.GetTabsBySortOrder(context.PortalId).Where(t => t.ParentId != AdminTabId && !t.IsSuperTab);
            tabs = tabs.Where(t => Navigation.CanShowTab(t, false, true, false));
            int total = tabs.Count();
            if (selectQuery != null)
            {
                var tabName = selectQuery.Query.FilterRules.FirstOrDefault(f => f.Field == "TabName");
                if (tabName != null)
                {
                    tabs = tabs.Where(t => t.TabName.ToLower().Contains(tabName.Value.AsString.ToLower()));
                }
                tabs = tabs.Skip(selectQuery.PageIndex * selectQuery.PageSize).Take(selectQuery.PageSize).ToList();
            }
            var dataList = new List<IDataItem>();
            foreach (var tab in tabs)
            {
                var item = new DefaultDataItem()
                {
                    Id = tab.TabID.ToString(),
                    Title = tab.TabName,
                    Data = JObject.FromObject(new
                    {
                        tab.TabName,
                        tab.TabID,
                        tab.Title,
                        tab.Description,
                    }),
                    CreatedByUserId = tab.CreatedByUserID,
                    LastModifiedByUserId = tab.LastModifiedByUserID,
                    LastModifiedOnDate = tab.LastModifiedOnDate,
                    CreatedOnDate = tab.CreatedOnDate,
                    Item = tab
                };
                item.Data["Settings"] = new JObject();
                item.Data["Head"] = new JObject();
                foreach (string key in tab.TabSettings.Keys)
                {
                    if (key.StartsWith("og:"))
                    {
                        item.Data["Head"][key] = tab.TabSettings[key].ToString();
                    }
                    else
                    {
                        item.Data["Settings"][key] = tab.TabSettings[key].ToString();
                    }
                }
                dataList.Add(item);
            }
            return new DefaultDataItems()
            {
                Items = dataList,
                Total = total,
                //DebugInfo = 
            };
        }
        public override JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm("",context.CurrentCultureCode);
        }

        public override void Add(DataSourceContext context, Newtonsoft.Json.Linq.JToken data)
        {
            throw new NotImplementedException();
        }

        public override void Update(DataSourceContext context, IDataItem item, Newtonsoft.Json.Linq.JToken data)
        {
            var schema = GetAlpaca(context, true, false, false)["schema"] as JObject;
            TabController tc = new TabController();
            var tab = (TabInfo)item.Item;
            if (HasProperty(schema, "", "TabName"))
            {
                tab.TabName = data["TabName"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "Title"))
            {
                tab.Title = data["Title"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "Description"))
            {
                tab.Description = data["Description"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "Settings"))
            {
                var settings = data["Settings"] as JObject;
                var settingsSchema = schema["properties"]["Settings"]["properties"] as JObject;
                foreach (var prop in settingsSchema.Properties())
                {
                    if (settings[prop.Name] != null)
                    {
                        tc.UpdateTabSetting(tab.TabID, prop.Name, settings[prop.Name].ToString());
                    }
                    else
                    {
                        tc.DeleteTabSetting(tab.TabID, prop.Name);
                    }
                }
            }
            if (HasProperty(schema, "", "Head"))
            {
                string head = "";
                var settings = data["Head"] as JObject;
                var settingsSchema = schema["properties"]["Head"]["properties"] as JObject;
                foreach (var prop in settingsSchema.Properties())
                {
                    if (settings[prop.Name] != null)
                    {
                        tc.UpdateTabSetting(tab.TabID, prop.Name, settings[prop.Name].ToString());
                    }
                    else
                    {
                        tc.DeleteTabSetting(tab.TabID, prop.Name);
                    }
                    head += $"<meta property=\"{prop.Name}\" content=\"{prop.Value.ToString()}\" />";
                }
                tab.PageHeadText = head;
            }
            tc.UpdateTab(tab);
        }

        public override void Delete(DataSourceContext context, IDataItem item)
        {
            throw new NotImplementedException();
        }
        private bool HasProperty(JObject schema, string subobject, string property)
        {
            if (!string.IsNullOrEmpty(subobject))
            {
                schema = schema[subobject] as JObject;
            }
            if (schema == null || !(schema["properties"] is JObject) ) return false;

            return ((JObject)schema["properties"]).Properties().Any(p => p.Name == property);
        }
    }
}