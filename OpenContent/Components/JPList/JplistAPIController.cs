﻿using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Querying;
using Satrabel.OpenContent.Components.Render;

namespace Satrabel.OpenContent.Components.JPList
{
    [SupportedModules("OpenContent")]
    public class JplistAPIController : DnnApiController
    {
        //[ValidateAntiForgeryToken] to work with output caching
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage List(RequestDTO req)
        {
            try
            {
                OpenContentModuleConfig module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                JObject reqOptions = null;
                if (!string.IsNullOrEmpty(req.options))
                {
                    reqOptions = JObject.Parse(req.options);
                }
                if (module.IsListMode())
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template);
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    bool isEditable = module.ViewModule.CheckIfEditable(module);
                    queryBuilder.Build(module.Settings.Query, !isEditable, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles.FromDnnRoles());

                    JplistQueryBuilder.MergeJpListQuery(indexConfig, queryBuilder.Select, req.StatusLst, DnnLanguageUtils.GetCurrentCultureCode());
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
                        dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    }
                    var mf = new ModelFactoryMultiple(dsItems.Items, module);
                    mf.Options = reqOptions;
                    var model = mf.GetModelAsJson(false, req.onlyItems);

                    //model["luceneQuery"] = dsItems.DebugInfo;
                    if (LogContext.IsLogActive)
                    {
                        LogContext.Log(ActiveModule.ModuleID, "RequestContext", "IsEditable", isEditable);
                        LogContext.Log(ActiveModule.ModuleID, "RequestContext", "UserRoles", PortalSettings.UserInfo.Social.Roles.Select(r => r.RoleName));
                        LogContext.Log(ActiveModule.ModuleID, "RequestContext", "CurrentUserId", PortalSettings.UserId);
                        var logKey = "Query";
                        LogContext.Log(ActiveModule.ModuleID, logKey, "select", queryBuilder.Select);
                        LogContext.Log(ActiveModule.ModuleID, logKey, "debuginfo", dsItems.DebugInfo);
                        LogContext.Log(ActiveModule.ModuleID, logKey, "model", model);
                        model["Logs"] = JToken.FromObject(LogContext.Current.ModuleLogs(ActiveModule.ModuleID));
                    }
                    var res = new ResultDTO()
                    {
                        data = model,
                        count = dsItems.Total
                    };
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

    }
}
