using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
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
using Satrabel.OpenContent.Components.Datasource.Search;

namespace Satrabel.OpenContent.Components.Rest
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
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);
                var manifest =module.Settings.Template.Manifest;
                var templateManifest = module.Settings.Template;
                JObject reqOptions = null;
                //if (!string.IsNullOrEmpty(req.options))
                //{
                //    reqOptions = JObject.Parse(req.options);
                //}
                //string editRole = manifest.GetEditRole();
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                if (listMode)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template.Key.TemplateDir);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    bool isEditable = ActiveModule.CheckIfEditable(PortalSettings);//portalSettings.UserMode != PortalSettings.Mode.Edit;
                    queryBuilder.Build(module.Settings.Query, !isEditable, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles);
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
                        IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                        var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID,false, reqOptions);
                        dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    }
                    ModelFactory mf = new ModelFactory(dsItems.Items, module, PortalSettings);
                    mf.Options = reqOptions;
                    var model = mf.GetModelAsJson(false);
                    var res = new JObject();
                    res["meta"] = new JObject();
                    res["meta"]["total"] = dsItems.Total;
                    //model["luceneQuery"] = dsItems.DebugInfo;
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Query";
                        LogContext.Log(module.ViewModule.ModuleID, logKey, "select", queryBuilder.Select);
                        LogContext.Log(module.ViewModule.ModuleID, logKey, "debuginfo", dsItems.DebugInfo);
                        LogContext.Log(module.ViewModule.ModuleID, logKey, "model", model);
                        res["meta"]["logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(module.ViewModule.ModuleID));
                    }
                    if (model["Items"] is JArray)
                    {
                        foreach (var item in (JArray)model["Items"])
                        {
                            item["id"] = item["Context"]["Id"];
                            JsonUtils.IdJson(item);
                        }
                    }
                    res[entity] = model["Items"];
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Get(string entity, int pageIndex, int pageSize, string filter = null, string sort = null)
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

                ModuleController mc = new ModuleController();
                ModuleInfo activeModule = ActiveModule; //mc.GetModule(ModuleId, TabId, false);

                OpenContentSettings settings = activeModule.OpenContentSettings();
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);
                var templateManifest = settings.Template;
                JObject reqOptions = null;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                if (listMode)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    bool isEditable = ActiveModule.CheckIfEditable(PortalSettings);//portalSettings.UserMode != PortalSettings.Mode.Edit;
                    queryBuilder.Build(settings.Query, !isEditable, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles);

                    RestQueryBuilder.MergeQuery(indexConfig, queryBuilder.Select, restSelect, DnnLanguageUtils.GetCurrentCultureCode());
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
                        IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                        var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID, false, reqOptions);

                        dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    }
                    ModelFactory mf = new ModelFactory(dsItems.Items, module, PortalSettings);
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
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Get(string entity)
        {
            try
            {
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);

                var manifest = module.Settings.Manifest;
                TemplateManifest templateManifest = module.Settings.Template;
                if (manifest.AdditionalDataDefined(entity))
                {
                    var dataManifest = manifest.AdditionalDataDefinition[entity];
                    //string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, ActiveModule.TabID, module.ModuleID, ActiveModule.TabModuleID);

                    //var templateFolder = string.IsNullOrEmpty(dataManifest.TemplateFolder) ? settings.TemplateDir : settings.TemplateDir.ParentFolder.Append(dataManifest.TemplateFolder);
                    //var fb = new FormBuilder(templateFolder);
                    //JObject json = fb.BuildForm(entity);
                    var res = new JObject();

                    int createdByUserid = -1;

                    IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                    var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? entity);
                    if (dsItem != null)
                    {
                        var json = dsItem.Data;
                        createdByUserid = dsItem.CreatedByUserId;
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

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Put(string entity, string id, [FromBody]JObject value)
        {
            // update
            try
            {
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);

                var manifest = module.Settings.Template.Manifest;
                TemplateManifest templateManifest = module.Settings.Template;
                string editRole = manifest.GetEditRole();

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                IDataItem dsItem = null;
                if (listMode)
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
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
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
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);

                var manifest = module.Settings.Template.Manifest;
                TemplateManifest templateManifest = module.Settings.Template;
                string editRole = manifest.GetEditRole();

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                IDataItem dsItem = null;
                if (listMode)
                {
                    if (id != null)
                    {
                        var itemId = id;
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
                Log.Logger.Error(exc);
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
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);

                var manifest = settings.Template.Manifest;
                TemplateManifest templateManifest = settings.Template;
                string editRole = manifest.GetEditRole();

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                ds.Add(dsContext, value.Properties().First().Value as JObject);

                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [SupportedModules("OpenContent")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Delete(string entity, string id)
        {

            try
            {
                OpenContentModuleInfo module = new OpenContentModuleInfo(ActiveModule);
                var manifest = module.Settings.Template.Manifest;
                TemplateManifest templateManifest = module.Settings.Template;
                string editRole = manifest.GetEditRole();

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                IDataItem dsItem = null;
                if (listMode)
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
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                if (dsItem != null)
                {
                    ds.Delete(dsContext, dsItem);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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