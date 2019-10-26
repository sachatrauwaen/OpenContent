#region Copyright

// 
// Copyright (c) 2015-2017
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using System.IO;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Entities.Modules;
using System.Collections.Generic;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;

#endregion

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class OpenContentAPIController : DnnApiController
    {
        #region Queries

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Edit()
        {
            return Edit(null);
        }

        /// <summary>
        /// Edits the specified identifier.
        /// </summary>
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Edit(string id)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module);
                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (!string.IsNullOrEmpty(id)) // not a new item
                    {
                        dsItem = ds.Get(dsContext, id);
                    }
                }
                else
                {
                    dsContext.Single = true;
                    dsItem = ds.Get(dsContext, null);
                }
                int createdByUserid = -1;
                var json = ds.GetAlpaca(dsContext, true, true, true);
                if (ds is IDataActions)
                {
                    var actions = ((IDataActions)ds).GetActions(dsContext, dsItem);
                    if (json["options"] == null) json["options"] = new JObject();
                    if (json["options"]["form"] == null) json["options"]["form"] = new JObject();
                    if (json["options"]["form"]["buttons"] == null) json["options"]["form"]["buttons"] = new JObject();
                    var buttons = json["options"]["form"]["buttons"] as JObject;
                    var newButtons = new JObject();
                    foreach (var act in actions)
                    {
                        var but = buttons[act.Name];
                        if (but == null)
                        {
                            but = new JObject();
                        }
                        but["after"] = act.AfterExecute;
                        newButtons[act.Name] = but;

                    }
                    json["options"]["form"]["buttons"] = newButtons;
                }
                if (dsItem != null)
                {
                    json["data"] = dsItem.Data;
                    if (json["schema"]["properties"]["ModuleTitle"] is JObject)
                    {
                        if (json["data"]["ModuleTitle"] != null && json["data"]["ModuleTitle"].Type == JTokenType.String)
                        {
                            json["data"]["ModuleTitle"] = ActiveModule.ModuleTitle;
                        }
                        else if (json["data"]["ModuleTitle"] != null && json["data"]["ModuleTitle"].Type == JTokenType.Object)
                        {
                            json["data"]["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()] = ActiveModule.ModuleTitle;
                        }
                    }
                    var versions = ds.GetVersions(dsContext, dsItem);
                    if (versions != null)
                    {
                        json["versions"] = versions;
                    }
                    createdByUserid = dsItem.CreatedByUserId;
                }

                var context = new JObject();
                var currentLocale = DnnLanguageUtils.GetCurrentLocale(PortalSettings.PortalId);
                context["culture"] = currentLocale.Code;  //todo why not use  DnnLanguageUtils.GetCurrentCultureCode() ???
                context["defaultCulture"] = LocaleController.Instance.GetDefaultLocale(PortalSettings.PortalId).Code;
                context["numberDecimalSeparator"] = currentLocale.Culture.NumberFormat.NumberDecimalSeparator;
                context["rootUrl"] = System.Web.VirtualPathUtility.ToAbsolute(string.Concat(System.Web.HttpRuntime.AppDomainAppVirtualPath, "/"));
                context["alpacaCulture"] = AlpacaEngine.AlpacaCulture(currentLocale.Code);
                context["bootstrap"] = App.Services.CreateGlobalSettingsRepository(PortalSettings.PortalId).GetEditLayout() != AlpacaLayoutEnum.DNN;
                context["horizontal"] = App.Services.CreateGlobalSettingsRepository(PortalSettings.PortalId).GetEditLayout() == AlpacaLayoutEnum.BootstrapHorizontal;
                json["context"] = context;

                //todo: can't we do some of these checks at the beginning of this method to fail faster?
                if (!DnnPermissionsUtils.HasEditPermissions(module, module.Settings.Manifest.GetEditRole(), createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage EditData(string key)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                var dataManifest = module.Settings.Manifest.GetAdditionalData(key);

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key);
                var json = ds.GetDataAlpaca(dsContext, true, true, true, key);
                if (dsItem != null)
                {
                    json["data"] = dsItem.Data;
                    var versions = ds.GetDataVersions(dsContext, dsItem);
                    if (versions != null)
                    {
                        json["versions"] = versions;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage UpdateData(JObject json)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                string key = json["key"].ToString();
                var dataManifest = module.Settings.Template.Manifest.GetAdditionalData(key);

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key);
                if (dsItem == null)
                {
                    ds.AddData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key, json["form"]);
                }
                else
                {
                    ds.UpdateData(dsContext, dsItem, json["form"]);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    isValid = true
                });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Version(string id, string ticks)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            JToken json = new JObject();
            try
            {
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                var dsItem = ds.Get(dsContext, id);
                if (dsItem != null)
                {
                    var version = ds.GetVersion(dsContext, dsItem, new DateTime(long.Parse(ticks)));
                    if (version != null)
                    {
                        json = version;
                        createdByUserid = dsItem.CreatedByUserId;
                    }
                }

                string editRole = module.Settings.Template.Manifest.GetEditRole();
                //todo: can't we do some of these checks at the beginning of this method to fail faster?
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Settings()
        {
            string data = (string)ActiveModule.ModuleSettings["data"];
            string template = (string)ActiveModule.ModuleSettings["template"];
            try
            {
                var templateUri = new FileUri(template);
                string key = templateUri.FileNameWithoutExtension;
                var fb = new FormBuilder(templateUri);
                JObject json = fb.BuildForm(key, DnnLanguageUtils.GetCurrentCultureCode());

                var dataJson = data.ToJObject("Raw settings json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage EditSettings(string key)
        {
            return EditSettings(key, true);
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage EditSettings(string key, bool templateFolder)
        {
            string data = (string)ActiveModule.ModuleSettings[key];
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                var fb = new FormBuilder(templateFolder ? settings.TemplateDir : new FolderUri("~/DesktopModules/OpenContent"));
                JObject json = fb.BuildForm(key, DnnLanguageUtils.GetCurrentCultureCode());
                var dataJson = data.ToJObject("Raw settings json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage EditQuerySettings()
        {
            string data = (string)ActiveModule.ModuleSettings["query"];
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                var fb = new FormBuilder(settings.TemplateDir);
                JObject json = fb.BuildQuerySettings(settings.Template.Collection);
                var dataJson = data.ToJObject("query settings json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Lookups the data for Additional Data.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage LookupData(LookupDataRequestDTO req)
        {
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);


                string key = req.dataKey;
                var additionalDataManifest = module.Settings.Template.Manifest.GetAdditionalData(key);

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                var dataItems = ds.GetData(dsContext, additionalDataManifest.ScopeType, additionalDataManifest.StorageKey ?? key);
                if (dataItems != null)
                {
                    JToken json = dataItems.Data;
                    if (!string.IsNullOrEmpty(req.dataMember))
                    {
                        json = json[req.dataMember];
                    }
                    if (json is JArray)
                    {
                        json = json.DeepClone();
                        JsonUtils.SimplifyJson(json, DnnLanguageUtils.GetCurrentCultureCode());
                        AddLookupItems(req.valueField, req.textField, req.childrenField, res, json as JArray);

                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage Lookup(LookupRequestDTO req)
        {
            int moduleid = req.moduleid > 0 ? req.moduleid : ActiveModule.ModuleID;
            int tabid = req.tabid > 0 ? req.tabid : ActiveModule.TabID;
            var module = OpenContentModuleConfig.Create(moduleid, tabid, PortalSettings);
            if (module == null) throw new Exception($"Can not find ModuleInfo (tabid:{req.tabid}, moduleid:{req.moduleid})");
            
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                if (module.IsListMode())
                {
                    var items = ds.GetAll(dsContext, null).Items;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var json = item.Data;

                            if (!string.IsNullOrEmpty(req.dataMember) && json[req.dataMember] != null)
                            {
                                json = json[req.dataMember];
                            }
                            json = json.DeepClone();
                            JsonUtils.SimplifyJson(json, DnnLanguageUtils.GetCurrentCultureCode());

                            var array = json as JArray;
                            if (array != null)
                            {
                                res.AddRange(array.Select(childItem =>
                                    new LookupResultDTO
                                    {
                                        value = string.IsNullOrEmpty(req.valueField) || childItem[req.valueField] == null ? "" : childItem[req.valueField].ToString(),
                                        text = string.IsNullOrEmpty(req.textField) || childItem[req.textField] == null ? "" : childItem[req.textField].ToString()
                                    }
                                    )
                                );
                            }
                            else
                            {
                                res.Add(new LookupResultDTO
                                {
                                    value = string.IsNullOrEmpty(req.valueField) || json[req.valueField] == null ? item.Id : json[req.valueField].ToString(),
                                    text = string.IsNullOrEmpty(req.textField) || json[req.textField] == null ? item.Title : json[req.textField].ToString()
                                });
                            }
                        }
                    }
                }
                else
                {
                    dsContext.Single = true;
                    var struc = ds.Get(dsContext, null);
                    if (struc != null)
                    {
                        JToken json = struc.Data;
                        if (!string.IsNullOrEmpty(req.dataMember))
                        {
                            json = json[req.dataMember];
                            json = json.DeepClone();
                            JsonUtils.SimplifyJson(json, DnnLanguageUtils.GetCurrentCultureCode());

                            if (json is JArray)
                            {
                                foreach (JToken item in (JArray)json)
                                {
                                    res.Add(new LookupResultDTO()
                                    {
                                        value = item[req.valueField] == null ? "" : item[req.valueField].ToString(),
                                        text = item[req.textField] == null ? "" : item[req.textField].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage LookupCollection(LookupCollectionRequestDTO req)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var res = new List<LookupResultDTO>();

            try
            {
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                dsContext.Collection = req.collection;

                if (module.IsListMode())
                {
                    var items = ds.GetAll(dsContext, null).Items;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var json = item.Data as JObject;
                            json = json.DeepClone() as JObject;
                            JsonUtils.SimplifyJson(json, DnnLanguageUtils.GetCurrentCultureCode());
                            if (json?[req.textField] != null)
                            {
                                res.Add(new LookupResultDTO()
                                {
                                    value = item.Id,
                                    text = json[req.textField].ToString()
                                });
                            }
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void AddLookupItems(string valueField, string textField, string childrenField, List<LookupResultDTO> res, JArray json, string prefix = "")
        {
            foreach (JToken item in json)
            {
                res.Add(new LookupResultDTO()
                {
                    value = item[valueField] == null ? "" : item[valueField].ToString(),
                    text = item[textField] == null ? "" : prefix + item[textField].ToString()
                });

                if (!string.IsNullOrEmpty(childrenField) && item[childrenField] is JArray)
                {
                    var childJson = item[childrenField] as JArray;
                    AddLookupItems(valueField, textField, childrenField, res, childJson, $"{prefix}..");
                }
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage LoadBuilder(string key)
        {
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                string prefix = string.IsNullOrEmpty(key) ? "" : key + "-";
                JObject json = new JObject();
                var dataJson = JsonUtils.LoadJsonFromCacheOrDisk(new FileUri(settings.TemplateDir, $"{prefix}builder.json"));
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Commands

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Update(JObject json)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                string editRole = module.Settings.Template.Manifest.GetEditRole();
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (json["id"] != null)
                    {
                        var itemId = json["id"].ToString();
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

                //todo: can't we do some of these checks at the beginning of this method to fail faster?
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                AddNotifyInfo(dsContext);
                try
                {
                    if (dsItem == null)
                    {
                        ds.Add(dsContext, json["form"] as JObject);
                    }
                    else
                    {
                        ds.Update(dsContext, dsItem, json["form"] as JObject);
                    }
                    App.Services.CacheAdapter.SyncronizeCache(module);
                }
                catch (DataNotValidException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        isValid = false,
                        validMessage = ex.Message
                    });
                }

                if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                {
                    string moduleTitle = json["form"]["ModuleTitle"].ToString();
                    ActiveModule.UpdateModuleTitle(moduleTitle);
                }
                else if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.Object)
                {
                    if (json["form"]["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()] != null)
                    {
                        string moduleTitle = json["form"]["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()].ToString();
                        ActiveModule.UpdateModuleTitle(moduleTitle);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    isValid = true
                });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }


        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage ReOrder(List<string> ids)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (ids != null)
                    {
                        int i = 1;
                        foreach (var id in ids)
                        {
                            dsItem = ds.Get(dsContext, id);
                            var json = dsItem.Data;
                            json["SortIndex"] = i;
                            ds.Update(dsContext, dsItem, json);
                            i++;
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    isValid = true
                });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private void AddNotifyInfo(DataSourceContext dsContext)
        {
            string jsonSettings = ActiveModule.ModuleSettings["notifications"] as string;
            if (!string.IsNullOrEmpty(jsonSettings))
            {
                dsContext.Options = new JObject();
                dsContext.Options = JObject.Parse(jsonSettings);
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Action(SubmitDTO req)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                string editRole = module.Settings.Template.Manifest.GetEditRole();
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                IDataItem dsItem = null;
                if (module.IsListMode())
                {
                    if (req.id != null)
                    {
                        var itemId = req.id;
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

                //todo: can't we do some of these checks at the beginning of this method to fail faster?
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                try
                {
                    var res = ds.Action(dsContext, req.action, dsItem, req.form);
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        isValid = true,
                        result = res
                    });
                }
                catch (DataNotValidException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        isValid = false,
                        validMessage = ex.Message
                    });
                }
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Delete(JObject json)
        {
            try
            {
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                string editRole = module.Settings.Template.Manifest.GetEditRole();
                int createdByUserid = -1;

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                IDataItem content = null;
                if (module.IsListMode())
                {
                    content = ds.Get(dsContext, json["id"].ToString());
                    if (content != null)
                    {
                        createdByUserid = content.CreatedByUserId;
                    }
                }
                else
                {
                    dsContext.Single = true;
                    content = ds.Get(dsContext, null);
                    if (content != null)
                    {
                        createdByUserid = content.CreatedByUserId;
                    }
                }

                //todo: can't we do some of these checks at the beginning of this method to fail faster?
                if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                AddNotifyInfo(dsContext);
                if (content != null)
                {
                    ds.Delete(dsContext, content);
                }
                App.Services.CacheAdapter.SyncronizeCache(module);
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage UpdateSettings(JObject json)
        {
            try
            {
                var mc = new ModuleController();
                int moduleId = ActiveModule.ModuleID;
                if (json["data"] != null)
                {
                    var data = json["data"].ToString();
                    if (!string.IsNullOrEmpty(data)) mc.UpdateModuleSetting(moduleId, "data", data);
                }
                else if (json["form"] != null)
                {
                    var form = json["form"].ToString();
                    var key = json["key"].ToString();
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(form))
                        mc.UpdateModuleSetting(moduleId, key, form);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    isValid = true
                });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage UpdateQuerySettings(JObject json)
        {
            try
            {
                var mc = new ModuleController();
                int moduleId = ActiveModule.ModuleID;
                if (json["form"] != null)
                {
                    var form = json["form"].ToString();
                    var key = "query";
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(form))
                        mc.UpdateModuleSetting(moduleId, key, form);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    isValid = true
                });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage UpdateBuilder(JObject json)
        {
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();

                if (json["data"] != null && json["schema"] != null && json["options"] != null)
                {
                    var key = json["key"].ToString();
                    string prefix = string.IsNullOrEmpty(key) ? "" : key + "-";
                    var schema = json["schema"].ToString();
                    var options = json["options"].ToString();
                    var view = json["view"].ToString();
                    var index = json["index"].ToString();
                    var data = json["data"].ToString();
                    var datafile = new FileUri(settings.TemplateDir, $"{prefix}builder.json");
                    var schemafile = new FileUri(settings.TemplateDir, $"{prefix}schema.json");
                    var optionsfile = new FileUri(settings.TemplateDir, $"{prefix}options.json");
                    var viewfile = new FileUri(settings.TemplateDir, $"{prefix}view.json");
                    var indexfile = new FileUri(settings.TemplateDir, $"{prefix}index.json");
                    try
                    {
                        File.WriteAllText(datafile.PhysicalFilePath, data);
                        File.WriteAllText(schemafile.PhysicalFilePath, schema);
                        File.WriteAllText(optionsfile.PhysicalFilePath, options);
                        File.WriteAllText(viewfile.PhysicalFilePath, view);
                        if (string.IsNullOrEmpty(index))
                        {
                            if (indexfile.FileExists)
                            {
                                File.Delete(indexfile.PhysicalFilePath);
                            }
                        }
                        else
                        {
                            File.WriteAllText(indexfile.PhysicalFilePath, index);
                        }
                    }
                    catch (Exception ex)
                    {
                        string mess = $"Error while saving file [{datafile.FilePath}]";
                        App.Services.Logger.Error(mess, ex);
                        throw new Exception(mess, ex);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    isValid = true
                });
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion
    }

    public class LookupRequestDTO
    {
        public int moduleid { get; set; }
        public int tabid { get; set; }
        public string dataMember { get; set; }
        /// <summary>
        /// Gets or sets the value field.
        /// </summary>
        /// <value>
        /// The Id field. 
        /// </value>
        public string valueField { get; set; }
        /// <summary>
        /// Gets or sets the text field.
        /// </summary>
        /// <value>
        /// The Display text.
        /// </value>
        public string textField { get; set; }
    }
    public class LookupDataRequestDTO
    {
        /// <summary>
        /// Gets or sets the data key.
        /// </summary>
        /// <value>
        /// Which additional data object to search. 
        /// </value>
        public string dataKey { get; set; }
        /// <summary>
        /// Gets or sets the data member.
        /// </summary>
        /// <value>
        /// Optional The data member of the data object to search. 
        /// </value>
        public string dataMember { get; set; }
        /// <summary>
        /// Gets or sets the value field.
        /// </summary>
        /// <value>
        /// The value field.
        /// </value>
        public string valueField { get; set; }
        /// <summary>
        /// Gets or sets the text field.
        /// </summary>
        /// <value>
        /// The text field.
        /// </value>
        public string textField { get; set; }
        public string childrenField { get; set; }
    }
    public class LookupCollectionRequestDTO
    {
        public string textField { get; set; }

        public string collection { get; set; }
    }
    public class LookupResultDTO
    {
        public string value { get; set; }
        public string text { get; set; }
    }

    public class ListDTO
    {
        public string query { get; set; }
    }
}

