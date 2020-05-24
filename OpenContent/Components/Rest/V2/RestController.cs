using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Render;
using DotNetNuke.Services.Exceptions;
using System.Web;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Querying;
using System.Collections.Specialized;

namespace Satrabel.OpenContent.Components.Rest.V2
{

    public class RestController : DnnApiController
    {
        //[ValidateAntiForgeryToken]
        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Get(string entity, string id)
        {
            try
            {
                var collection = entity;
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);

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
                    var res = new JObject();
                    res["meta"] = new JObject();
                    if (dsItem != null)
                    {
                        var mf = new ModelFactorySingle(dsItem, module, collection);

                        string raison = "";
                        if (!OpenContentUtils.HaveViewPermissions(dsItem, module.UserRoles.FromDnnRoles(), indexConfig, out raison))
                        {
                            Exceptions.ProcessHttpException(new HttpException(404, "No detail view permissions for id=" + id + " (" + raison + ")"));
                            //throw new UnauthorizedAccessException("No detail view permissions for id " + info.DetailItemId);
                        }

                        mf.Options = reqOptions;
                        var model = mf.GetModelAsJson(false);

                        model["id"] = model["Context"]["Id"];
                        res["meta"]["total"] = dsItem == null ? 0 : 1;
                        //JsonUtils.IdJson(model);
                        if (LogContext.IsLogActive)
                        {
                            var logKey = "Query";
                            LogContext.Log(module.ViewModule.ModuleId, logKey, "model", model);
                            res["meta"]["logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(module.ViewModule.ModuleId));
                        }
                        res[entity] = model;
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
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Get(string entity, int pageIndex, int pageSize, string filter = null, string sort = null)
        {
            try
            {
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

                ModuleInfo activeModule = ActiveModule;

                OpenContentSettings settings = activeModule.OpenContentSettings();
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                JObject reqOptions = null;

                if (module.IsListMode())
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.TemplateDir, collection);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    bool isEditable = module.ViewModule.CheckIfEditable(module);

                    var Query = settings.Query;
                    // manipulate settingsfilter for the userroles key in case of social groups, the only items we want to see are the items from the social group/role
                    //Query = 
                    SocialGroupUtils.AddSocialGroupQueryFilter(settings.Manifest, Query, HttpContext.Current.Request.QueryString);

                    queryBuilder.Build(Query, !isEditable, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles.FromDnnRoles());

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
                        LogContext.Log(activeModule.ModuleID, logKey, "select", queryBuilder.Select);
                        LogContext.Log(activeModule.ModuleID, logKey, "debuginfo", dsItems.DebugInfo);
                        LogContext.Log(activeModule.ModuleID, logKey, "model", model);
                        res["meta"]["logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(activeModule.ModuleID));

                        if (restSelect != null)
                        {
                            //res["meta"]["select"] = JObject.FromObject(restSelect);
                        }
                    }
                    foreach (var item in model["Items"] as JArray)
                    {
                        item["id"] = item["Context"]["Id"];
                        JsonUtils.IdJson(item);
                    }
                    res[entity] = model["Items"];
                    res["meta"]["total"] = dsItems.Total;
                    return Request.CreateResponse(HttpStatusCode.OK, res);
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

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Get(string entity)
        {
            try
            {
                var collection = entity;
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);

                var manifest = module.Settings.Manifest;
                if (manifest.AdditionalDataDefined(entity))
                {
                    var dataManifest = manifest.AdditionalDataDefinition[entity];
                    var res = new JObject();

                    IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                    dsContext.Collection = collection;
                    var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? entity);
                    if (dsItem != null)
                    {
                        var json = dsItem.Data;
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
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Put(string entity, string id, [FromBody]JObject value)
        {
            // update
            try
            {
                var collection = entity;
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);

                string editRole = module.Settings.Template.Manifest.GetEditRole();
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                dsContext.Collection = collection;
                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (id != null)
                    {
                        var itemId = id;
                        dsItem = ds.Get(dsContext, itemId);
                        if (dsItem != null)
                            createdByUserid = dsItem.CreatedByUserId;
                    }
                }
                else
                {
                    dsContext.Single = true;
                    dsItem = ds.Get(dsContext, null);
                    if (dsItem != null)
                        createdByUserid = dsItem.CreatedByUserId;
                }
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
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
                App.Services.CacheAdapter.SyncronizeCache(module);

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
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Triggers an Action
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="memberAction">The member action.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Put(string entity, string id, string memberAction, [FromBody]JObject value)
        {
            // action
            try
            {
                var collection = entity;
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                string editRole = module.Settings.Template.Manifest.GetEditRole();
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                dsContext.Collection = collection;
                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (id != null)
                    {
                        var itemId = id;
                        dsItem = ds.Get(dsContext, itemId);
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
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
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
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Post(string entity, [FromBody]JObject value)
        {
            // Add
            try
            {
                var collection = entity;
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);

                var manifest = module.Settings.Template.Manifest;
                string editRole = manifest.GetEditRole();

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                dsContext.Collection = collection;
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, -1))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                ds.Add(dsContext, value.Properties().First().Value as JObject);

                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Delete(string entity, string id)
        {

            try
            {
                var collection = entity;
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                string editRole = module.Settings.Template.Manifest.GetEditRole();
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                dsContext.Collection = collection;
                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (id != null)
                    {
                        var itemId = id;
                        dsItem = ds.Get(dsContext, itemId);
                        if (dsItem != null)
                            createdByUserid = dsItem.CreatedByUserId;
                    }
                }
                else
                {
                    dsContext.Single = true;
                    dsItem = ds.Get(dsContext, null);
                    if (dsItem != null)
                        createdByUserid = dsItem.CreatedByUserId;
                }
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                if (dsItem != null)
                {
                    ds.Delete(dsContext, dsItem);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        // CORS
        [AllowAnonymous]
        public HttpResponseMessage Options()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }

}