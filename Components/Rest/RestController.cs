using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.JPList;
using Satrabel.OpenContent.Components.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Datasource.search;

namespace Satrabel.OpenContent.Components.Rest
{


    [AllowAnonymous]
    public class RestController : DnnApiController
    {

        //[ValidateAntiForgeryToken]
        //[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Anonymous)]
        //[HttpGet]
        //[HttpOptions]
        [SupportedModules("OpenContent")]
        public HttpResponseMessage Get(string entity, string id)
        {
            try
            {


                //int ModuleId = int.Parse(Request.Headers.GetValues("ModuleId").First());
                //int TabId = int.Parse(Request.Headers.GetValues("TabId").First());
                ModuleController mc = new ModuleController();
                ModuleInfo activeModule = ActiveModule; //mc.GetModule(ModuleId, TabId, false);

                OpenContentSettings settings = activeModule.OpenContentSettings();
                ModuleInfo module = activeModule;
                if (settings.ModuleId > 0)
                {
                    //ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                var templateManifest = settings.Template;
                JObject reqOptions = null;
                //if (!string.IsNullOrEmpty(req.options))
                //{
                //    reqOptions = JObject.Parse(req.options);
                //}
                //string editRole = manifest == null ? "" : manifest.EditRole;
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                if (listMode)
                {

                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    if (!string.IsNullOrEmpty(settings.Query))
                    {
                        var query = JObject.Parse(settings.Query);
                        queryBuilder.Build(query, PortalSettings.UserMode != PortalSettings.Mode.Edit, UserInfo.UserID);
                    }
                    else
                    {
                        queryBuilder.BuildFilter(PortalSettings.UserMode != PortalSettings.Mode.Edit);
                    }
                    //if (restSelect != null)
                    //{
                    //    RestQueryBuilder.MergeJpListQuery(indexConfig, queryBuilder.Select, restSelect);
                    //}
                    queryBuilder.Select.Query.AddRule(new FilterRule()
                    {
                        Field = "$id",
                        Value = new StringRuleValue(id)
                    });
                    IDataItems dsItems;
                    if (queryBuilder.DefaultNoResults && queryBuilder.Select.IsQueryEmpty)
                    {
                        dsItems = new DefaultDataItems()
                        {
                            Items = new List<DefaultDataItem>(),
                            Total = 0
                        };
                    }
                    else
                    {
                        var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                        var dsContext = new DataSourceContext()
                        {
                            ModuleId = module.ModuleID,
                            UserId = UserInfo.UserID,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            Config = manifest.DataSourceConfig,
                            Options = reqOptions
                        };
                        dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    }
                    int mainTabId = settings.DetailTabId > 0 ? settings.DetailTabId : settings.TabId;
                    ModelFactory mf = new ModelFactory(dsItems.Items, activeModule, PortalSettings, mainTabId);
                    mf.Options = reqOptions;
                    var model = mf.GetModelAsJson(false);
                    var res = new JObject();
                    res["meta"] = new JObject();
                    res["meta"]["total"] = dsItems.Total;
                    //model["luceneQuery"] = dsItems.DebugInfo;
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Query";
                        LogContext.Log(activeModule.ModuleID, logKey, "select", queryBuilder.Select);
                        LogContext.Log(activeModule.ModuleID, logKey, "result", dsItems);
                        LogContext.Log(activeModule.ModuleID, logKey, "model", model);
                        res["meta"]["logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(activeModule.ModuleID));
                    }
                    foreach (var item in model["Items"] as JArray)
                    {
                        item["id"] = item["Context"]["Id"];
                        JsonUtils.IdJson(item);
                        //if (item["Gallery"] is JArray)
                        //{
                        //    foreach (var i in item["Gallery"] as JArray)
                        //    {
                        //        i["id"] = Guid.NewGuid().ToString();
                        //    }
                        //}
                    }
                    res[entity] = model["Items"];
                    //res["meta"]["id"] = id;
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        [SupportedModules("OpenContent")]
        public HttpResponseMessage Get(string entity, int pageIndex, int pageSize, string filter, string sort)
        {
            try
            {
                RestSelect restSelect = new RestSelect()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                if (!string.IsNullOrEmpty(filter))
                {
                    restSelect.Query = JsonConvert.DeserializeObject<RestGroup>(filter);
                }
                if (!string.IsNullOrEmpty(sort))
                {
                    restSelect.Sort = JsonConvert.DeserializeObject<List<RestSort>>(sort);
                }

                //int ModuleId = int.Parse(Request.Headers.GetValues("ModuleId").First());
                //int TabId = int.Parse(Request.Headers.GetValues("TabId").First());
                ModuleController mc = new ModuleController();
                ModuleInfo activeModule = ActiveModule; //mc.GetModule(ModuleId, TabId, false);

                OpenContentSettings settings = activeModule.OpenContentSettings();
                ModuleInfo module = activeModule;
                if (settings.ModuleId > 0)
                {
                    //ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                var templateManifest = settings.Template;
                JObject reqOptions = null;
                //if (!string.IsNullOrEmpty(req.options))
                //{
                //    reqOptions = JObject.Parse(req.options);
                //}
                //string editRole = manifest == null ? "" : manifest.EditRole;
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                if (listMode)
                {

                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    if (!string.IsNullOrEmpty(settings.Query))
                    {
                        var query = JObject.Parse(settings.Query);
                        queryBuilder.Build(query, PortalSettings.UserMode != PortalSettings.Mode.Edit, UserInfo.UserID);
                    }
                    else
                    {
                        queryBuilder.BuildFilter(PortalSettings.UserMode != PortalSettings.Mode.Edit);
                    }
                    if (restSelect != null)
                    {
                        RestQueryBuilder.MergeQuery(indexConfig, queryBuilder.Select, restSelect);
                    }
                    IDataItems dsItems;
                    if (queryBuilder.DefaultNoResults && queryBuilder.Select.IsQueryEmpty)
                    {
                        dsItems = new DefaultDataItems()
                        {
                            Items = new List<DefaultDataItem>(),
                            Total = 0
                        };
                    }
                    else
                    {
                        var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                        var dsContext = new DataSourceContext()
                        {
                            ModuleId = module.ModuleID,
                            UserId = UserInfo.UserID,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            Config = manifest.DataSourceConfig,
                            Options = reqOptions
                        };
                        dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    }
                    int mainTabId = settings.DetailTabId > 0 ? settings.DetailTabId : settings.TabId;
                    ModelFactory mf = new ModelFactory(dsItems.Items, activeModule, PortalSettings, mainTabId);
                    mf.Options = reqOptions;
                    var model = mf.GetModelAsJson(false);
                    var res = new JObject();
                    res["meta"] = new JObject();
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Query";
                        LogContext.Log(activeModule.ModuleID, logKey, "select", queryBuilder.Select);
                        LogContext.Log(activeModule.ModuleID, logKey, "result", dsItems);
                        LogContext.Log(activeModule.ModuleID, logKey, "model", model);
                        res["meta"]["logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(activeModule.ModuleID));
                    }
                    foreach (var item in model["Items"] as JArray)
                    {
                        item["id"] = item["Context"]["Id"];
                        JsonUtils.IdJson(item);
                    }
                    res[entity] = model["Items"];
                    res["meta"]["total"] = dsItems.Total;
                    if (restSelect != null)
                    {
                        res["meta"]["select"] = JObject.FromObject(restSelect);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        public HttpResponseMessage Get(string entity)
        {
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Manifest;
                TemplateManifest templateManifest = settings.Template;
                if (manifest.AdditionalData.ContainsKey(entity))
                {

                    var dataManifest = manifest.AdditionalData[entity];
                    string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, module.ModuleID, ActiveModule.TabModuleID);

                    var templateFolder = string.IsNullOrEmpty(dataManifest.TemplateFolder) ? settings.TemplateDir : settings.TemplateDir.ParentFolder.Append(dataManifest.TemplateFolder);
                    //var fb = new FormBuilder(templateFolder);
                    //JObject json = fb.BuildForm(entity);
                    var res = new JObject();

                    int createdByUserid = -1;
                    var dc = new AdditionalDataController();
                    var data = dc.GetData(scope, dataManifest.StorageKey ?? entity);
                    if (data != null)
                    {
                        var json = data.Json.ToJObject("GetContent " + scope + "/" + entity);
                        createdByUserid = data.CreatedByUserId;
                        JsonUtils.IdJson(json);
                        res[entity] = json;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return Get(entity, 0, 100, null, null);
                }

            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        public HttpResponseMessage Put(string entity, string id, [FromBody]JObject value)
        {
            // update
            try
            {
                bool index = false;
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                TemplateManifest templateManifest = settings.Template;
                index = settings.Template.Manifest.Index;
                string editRole = manifest == null ? "" : manifest.EditRole;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Index = index,
                    UserId = UserInfo.UserID,
                    PortalId = module.PortalID,
                    Config = manifest.DataSourceConfig
                };
                string itemId = null;
                IDataItem dsItem = null;
                if (listMode)
                {
                    if (id != null)
                    {
                        itemId = id;
                        dsItem = ds.Get(dsContext, itemId);
                        //content = ctrl.GetContent(itemId);
                        if (dsItem != null)
                            createdByUserid = dsItem.CreatedByUserId;
                    }
                }
                else
                {
                    dsContext.Single = true;
                    dsItem = ds.Get(dsContext, null);
                    //dsItem = ctrl.GetFirstContent(module.ModuleID);
                    if (dsItem != null)
                        createdByUserid = dsItem.CreatedByUserId;
                }
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    //return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                if (dsItem == null)
                {
                    ds.Add(dsContext, value.Properties().First().Value as JObject);
                }
                else
                {
                    ds.Update(dsContext, dsItem, value.Properties().First().Value as JObject);
                }
                //if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                //{
                //    string moduleTitle = json["form"]["ModuleTitle"].ToString();
                //    OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                //}
                //else if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.Object)
                //{
                //    if (json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()] != null)
                //    {
                //        string moduleTitle = json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                //        OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                //    }
                //}
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        public HttpResponseMessage Put(string entity, string id, string memberAction, [FromBody]JObject value)
        {
            // action
            try
            {
                bool index = false;
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                TemplateManifest templateManifest = settings.Template;
                index = settings.Template.Manifest.Index;
                string editRole = manifest == null ? "" : manifest.EditRole;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Index = index,
                    UserId = UserInfo.UserID,
                    PortalId = module.PortalID,
                    Config = manifest.DataSourceConfig
                };
                string itemId = null;
                IDataItem dsItem = null;
                if (listMode)
                {
                    if (id != null)
                    {
                        itemId = id;
                        dsItem = ds.Get(dsContext, itemId);
                        //content = ctrl.GetContent(itemId);
                        if (dsItem != null)
                            createdByUserid = dsItem.CreatedByUserId;
                    }
                }
                else
                {
                    dsContext.Single = true;
                    dsItem = ds.Get(dsContext, null);
                    //dsItem = ctrl.GetFirstContent(module.ModuleID);
                    if (dsItem != null)
                        createdByUserid = dsItem.CreatedByUserId;
                }
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    //return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                JToken res = null;
                if (dsItem != null)
                {
                    res = ds.Action(dsContext, memberAction, dsItem, value);
                }

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        public HttpResponseMessage Post(string entity, [FromBody]JObject value)
        {
            // Add
            try
            {
                bool index = false;
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                TemplateManifest templateManifest = settings.Template;
                index = settings.Template.Manifest.Index;
                string editRole = manifest == null ? "" : manifest.EditRole;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Index = index,
                    UserId = UserInfo.UserID,
                    PortalId = module.PortalID,
                    Config = manifest.DataSourceConfig
                };

                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    //return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                ds.Add(dsContext, value.Properties().First().Value as JObject);
                //if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                //{
                //    string moduleTitle = json["form"]["ModuleTitle"].ToString();
                //    OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                //}
                //else if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.Object)
                //{
                //    if (json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()] != null)
                //    {
                //        string moduleTitle = json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                //        OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                //    }
                //}
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        public HttpResponseMessage Delete(string entity, string id)
        {

            try
            {
                bool index = false;
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                TemplateManifest templateManifest = settings.Template;
                index = settings.Template.Manifest.Index;
                string editRole = manifest == null ? "" : manifest.EditRole;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Index = index,
                    UserId = UserInfo.UserID,
                    PortalId = module.PortalID,
                    Config = manifest.DataSourceConfig
                };
                string itemId = null;
                IDataItem dsItem = null;
                if (listMode)
                {
                    if (id != null)
                    {
                        itemId = id;
                        dsItem = ds.Get(dsContext, itemId);
                        //content = ctrl.GetContent(itemId);
                        if (dsItem != null)
                            createdByUserid = dsItem.CreatedByUserId;
                    }
                }
                else
                {
                    dsContext.Single = true;
                    dsItem = ds.Get(dsContext, null);
                    //dsItem = ctrl.GetFirstContent(module.ModuleID);
                    if (dsItem != null)
                        createdByUserid = dsItem.CreatedByUserId;
                }
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    //return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                if (dsItem == null)
                {

                }
                else
                {
                    ds.Delete(dsContext, dsItem);
                }
                //if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                //{
                //    string moduleTitle = json["form"]["ModuleTitle"].ToString();
                //    OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                //}
                //else if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.Object)
                //{
                //    if (json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()] != null)
                //    {
                //        string moduleTitle = json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                //        OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                //    }
                //}
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        public HttpResponseMessage Options()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }


}