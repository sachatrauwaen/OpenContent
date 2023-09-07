using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Rest;
using Newtonsoft.Json;
using System.Collections.Generic;
using Satrabel.OpenContent.Components.Querying;
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Services.Exceptions;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class ExternalApiController : DnnApiController
    {

        [AllowAnonymous]
        public HttpResponseMessage GetAllItems(string apiKey, int tabId, int moduleId)
        {
            try
            {
                string _apikey = App.Services.CreateGlobalSettingsRepository(PortalSettings.PortalId).GetRestApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                if (apiKey != _apikey)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var viewModule = DnnUtils.GetDnnModule(tabId, moduleId);

                var collection = "Items";

                OpenContentSettings settings = viewModule.OpenContentSettings();
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(viewModule, PortalSettings);
                //if (!module.HasAllUsersViewPermissions())
                //{
                //    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                //}
                JObject reqOptions = null;

                if (module.IsListMode())
                {
                    IDataItems dsItems;

                    IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID, false, reqOptions);
                    dsContext.Collection = collection;
                    dsItems = ds.GetAll(dsContext, null);

                    var mf = new ModelFactoryMultiple(dsItems.Items, module, collection);
                    mf.Options = reqOptions;
                    var model = mf.GetModelAsJson(false);
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Query";
                        LogContext.Log(viewModule.ModuleID, logKey, "debuginfo", dsItems.DebugInfo);
                        LogContext.Log(viewModule.ModuleID, logKey, "model", model);

                    }
                    foreach (var item in model["Items"] as JArray)
                    {
                        item["id"] = item["Context"]["Id"];
                        //JsonUtils.IdJson(item);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, model["Items"]);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [AllowAnonymous]
        public HttpResponseMessage GetItem(string apiKey, int tabId, int moduleId, string id)
        {
            try
            {
                string _apikey = App.Services.CreateGlobalSettingsRepository(PortalSettings.PortalId).GetRestApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                if (apiKey != _apikey)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var collection = "Items";

                var viewModule = DnnUtils.GetDnnModule(tabId, moduleId);
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(viewModule, PortalSettings);

                JObject reqOptions = null;
                //if (!string.IsNullOrEmpty(req.options))
                //{
                //    reqOptions = JObject.Parse(req.options);
                //}
                //string editRole = manifest.GetEditRole();
                if (module.IsListMode())
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.TemplateDir, collection);
                    //bool isEditable = ActiveModule.CheckIfEditable(PortalSettings);
                    IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID, false, reqOptions);
                    dsContext.Collection = collection;
                    var dsItem = ds.Get(dsContext, id);
                    if (dsItem != null)
                    {
                        var mf = new ModelFactorySingle(dsItem, module, collection);

                        //string raison = "";
                        //if (!OpenContentUtils.IsViewAllowed(dsItem, module.UserRoles.FromDnnRoles(), indexConfig, out raison))
                        //{
                        //    Exceptions.ProcessHttpException(new HttpException(404, "No detail view permissions for id=" + id + " (" + raison + ")"));
                        //    //throw new UnauthorizedAccessException("No detail view permissions for id " + info.DetailItemId);
                        //}

                        mf.Options = reqOptions;
                        var model = mf.GetModelAsJson(false);

                        model["id"] = model["Context"]["Id"];
                        if (LogContext.IsLogActive)
                        {
                            var logKey = "Query";
                            LogContext.Log(module.ViewModule.ModuleId, logKey, "model", model);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, model);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "no item found");
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }


        [AllowAnonymous]
        public HttpResponseMessage Get(string apiKey, int tabId, int moduleId, string entity, int pageIndex, int pageSize, string filter = null, string sort = null)
        {
            try
            {
                string _apikey = App.Services.CreateGlobalSettingsRepository(PortalSettings.PortalId).GetRestApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                if (apiKey != _apikey)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var viewModule = DnnUtils.GetDnnModule(tabId, moduleId);

                var collection = entity;
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

                OpenContentSettings settings = viewModule.OpenContentSettings();
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(viewModule, PortalSettings);
                //if (!module.HasAllUsersViewPermissions())
                //{
                //    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                //}
                JObject reqOptions = null;

                if (module.IsListMode())
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.TemplateDir, collection);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    bool isEditable = module.ViewModule.CheckIfEditable(module);
                    queryBuilder.Build(settings.Query, !isEditable, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles.FromDnnRoles());

                    RestQueryBuilder.MergeQuery(indexConfig, queryBuilder.Select, restSelect, DnnLanguageUtils.GetCurrentCultureCode());
                    IDataItems dsItems;
                    if (queryBuilder.DefaultNoResults && queryBuilder.Select.IsEmptyQuery)
                    {
                        dsItems = new DefaultDataItems()
                        {
                            Items = new List<DefaultDataItem>(),
                            Total = 0
                        };
                    }
                    else
                    {
                        IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                        var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID, false, reqOptions);
                        dsContext.Collection = collection;
                        dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    }
                    var mf = new ModelFactoryMultiple(dsItems.Items, module, collection);
                    mf.Options = reqOptions;
                    var model = mf.GetModelAsJson(false);
                    var res = new JObject();
                    res["meta"] = new JObject();
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Query";
                        LogContext.Log(viewModule.ModuleID, logKey, "select", queryBuilder.Select);
                        LogContext.Log(viewModule.ModuleID, logKey, "debuginfo", dsItems.DebugInfo);
                        LogContext.Log(viewModule.ModuleID, logKey, "model", model);
                        res["meta"]["logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(viewModule.ModuleID));

                        if (restSelect != null)
                        {
                            //res["meta"]["select"] = JObject.FromObject(restSelect);
                        }
                    }
                    foreach (var item in model["Items"] as JArray)
                    {
                        item["id"] = item["Context"]["Id"];
                        //JsonUtils.IdJson(item);
                    }
                    res[entity] = model["Items"];
                    res["meta"]["total"] = dsItems.Total;
                    //return Request.CreateResponse(HttpStatusCode.OK, res);

                    return Request.CreateResponse(HttpStatusCode.OK, model["Items"]);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [AllowAnonymous]
        public HttpResponseMessage GetData(string apiKey, int tabId, int moduleId, string scope, string key)
        {
            try
            {
                string _apikey = App.Services.CreateGlobalSettingsRepository(PortalSettings.PortalId).GetRestApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                if (apiKey != _apikey)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var viewModule = DnnUtils.GetDnnModule(tabId, moduleId);

                string scopeStorage = AdditionalDataUtils.GetScope(scope, PortalSettings.PortalId, tabId, moduleId, viewModule.TabModuleID);
                var dc = new AdditionalDataController();
                var json = dc.GetData(scopeStorage, key);
                if (json != null)
                {
                    //var dataItem = new DefaultDataItem("")
                    //{
                    //    Data = json.Json.ToJObject("GetContent " + scope + "/" + key),
                    //    CreatedByUserId = json.CreatedByUserId,
                    //    Item = json
                    //};
                    //if (LogContext.IsLogActive)
                    //{
                    //    LogContext.Log(context.ActiveModuleId, "Get Data", key, dataItem.Data);
                    //}
                    //return dataItem;
                    //OpenContentSettings settings = viewModule.OpenContentSettings();
                    OpenContentModuleConfig module = OpenContentModuleConfig.Create(viewModule, PortalSettings);

                    //if (!module.HasAllUsersViewPermissions())
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                    //}
                    return Request.CreateResponse(HttpStatusCode.OK, json.Json.ToJObject("GetContent " + scope + "/" + key));
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }


        /*
                [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
                [ValidateAntiForgeryToken]
                [HttpPost]
                public HttpResponseMessage Add(UpdateRequest req)
                {
                    try
                    {
                        var module = OpenContentModuleConfig.Create(req.ModuleId, req.TabId, PortalSettings);
                        string editRole = module.Settings.Template.Manifest.GetEditRole();

                        var dataSource = new OpenContentDataSource();

                        if (module.IsListMode())
                        {
                            if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, -1))
                            {
                                App.Services.Logger.Warn($"Failed the HasEditPermissions() check");
                                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Failed the HasEditPermissions() check");
                            }
                            var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                            dsContext.Collection = req.Collection;

                            JToken data = req.json;
                            data["Title"] = ActiveModule.ModuleTitle;
                            dataSource.Add(dsContext, data);
                            App.Services.CacheAdapter.SyncronizeCache(module);
                            return Request.CreateResponse(HttpStatusCode.OK, "");
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "It's not a list mode module");
                        }
                    }
                    catch (Exception exc)
                    {
                        App.Services.Logger.Error(exc);
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
                    }
                }
                public class UpdateRequest
                {
                    public int ModuleId { get; set; }
                    public string Collection { get; set; }
                    public int TabId { get; set; }
                    public JObject json { get; set; }
                }
        */
    }
}