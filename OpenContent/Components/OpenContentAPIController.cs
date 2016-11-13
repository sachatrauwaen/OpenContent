#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
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
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using DotNetNuke.Services.Localization;

#endregion

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class OpenContentAPIController : DnnApiController
    {
        public PortalFolderUri BaseDir
        {
            get
            {
                return new PortalFolderUri(PortalSettings.PortalId, PortalSettings.HomeDirectory + "/OpenContent/Templates/");
            }
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Edit()
        {
            return Edit(null);
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Edit(string id)
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
                string editRole = manifest.GetEditRole();
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    ActiveModuleId = ActiveModule.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig
                };
                IDataItem dsItem = null;
                if (listMode)
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
                //var content = GetContent(module.ModuleID, listMode, int.Parse(id));
                //if (content != null)
                if (dsItem != null)
                {
                    //json["data"] = content.Json.ToJObject("GetContent " + id);
                    //json = dsItem.Data as JObject;
                    json["data"] = dsItem.Data;
                    if (json["schema"]["properties"]["ModuleTitle"] is JObject)
                    {
                        //json["data"]["ModuleTitle"] = ActiveModule.ModuleTitle;
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
                    //AddVersions(json, content);
                    //createdByUserid = content.CreatedByUserId;
                    createdByUserid = dsItem.CreatedByUserId;
                }

                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Manifest;
                TemplateManifest templateManifest = settings.Template;
                var dataManifest = manifest.GetAdditionalData(key);
                var templateFolder = string.IsNullOrEmpty(dataManifest.TemplateFolder) ? settings.TemplateDir : settings.TemplateDir.ParentFolder.Append(dataManifest.TemplateFolder);
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    PortalId = PortalSettings.PortalId,
                    TabId = ActiveModule.TabID,
                    ModuleId = module.ModuleID,
                    TabModuleId = ActiveModule.TabModuleID,
                    UserId = UserInfo.UserID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig,
                    //Options = reqOptions
                };
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
                    createdByUserid = dsItem.CreatedByUserId;
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                string key = json["key"].ToString();
                var dataManifest = manifest.GetAdditionalData(key);
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    PortalId = PortalSettings.PortalId,
                    TabId = ActiveModule.TabID,
                    ModuleId = module.ModuleID,
                    TabModuleId = ActiveModule.TabModuleID,
                    UserId = UserInfo.UserID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig,
                    //Options = reqOptions
                };
                var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key);
                if (dsItem == null)
                {
                    ds.AddData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key, json["form"]);
                }
                else
                {
                    ds.UpdateData(dsContext, dsItem, json["form"]);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        private static void AddVersions(JObject json, AdditionalDataInfo data)
        {
            if (!string.IsNullOrEmpty(data.VersionsJson))
            {
                var verLst = new JArray();
                foreach (var item in data.Versions)
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
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Version(string id, string ticks)
        {
            OpenContentSettings settings = ActiveModule.OpenContentSettings();
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                ModuleController mc = new ModuleController();
                module = mc.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = settings.Template.Manifest;
            string editRole = manifest.GetEditRole();
            JToken json = new JObject();
            try
            {
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    ActiveModuleId = ActiveModule.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig
                };
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
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                JObject json = fb.BuildForm(key);

                var dataJson = data.ToJObject("Raw settings json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Update(JObject json)
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
                string editRole = manifest.GetEditRole();

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    ActiveModuleId = ActiveModule.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Index = index,
                    UserId = UserInfo.UserID,
                    PortalId = module.PortalID,
                    Config = manifest.DataSourceConfig
                };
                IDataItem dsItem = null;
                if (listMode)
                {
                    if (json["id"] != null)
                    {
                        var itemId = json["id"].ToString();
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
                if (dsItem == null)
                {
                    ds.Add(dsContext, json["form"] as JObject);
                }
                else
                {
                    ds.Update(dsContext, dsItem, json["form"] as JObject);
                }
                if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                {
                    string moduleTitle = json["form"]["ModuleTitle"].ToString();
                    OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                }
                else if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.Object)
                {
                    if (json["form"]["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()] != null)
                    {
                        string moduleTitle = json["form"]["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()].ToString();
                        OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                index = manifest.Index;
                string editRole = manifest.GetEditRole();
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    ActiveModuleId = ActiveModule.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Index = index,
                    UserId = UserInfo.UserID,
                    PortalId = module.PortalID,
                    Config = manifest.DataSourceConfig
                };
                IDataItem content = null;
                if (listMode)
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
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, ActiveModule, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                if (content != null)
                {
                    ds.Delete(dsContext, content);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                    //string template = (string)ActiveModule.ModuleSettings["template"];
                    //if (!string.IsNullOrEmpty(template)) mc.UpdateModuleSetting(moduleId, "template", template);
                    if (!string.IsNullOrEmpty(data)) mc.UpdateModuleSetting(moduleId, "data", data);
                }
                else if (json["form"] != null)
                {
                    var form = json["form"].ToString();
                    var key = json["key"].ToString();
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(form)) mc.UpdateModuleSetting(moduleId, key, form);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                    var data = json["data"].ToString();
                    var datafile = new FileUri(settings.TemplateDir.UrlFolder + prefix + "builder.json");
                    var schemafile = new FileUri(settings.TemplateDir.UrlFolder + prefix + "schema.json");
                    var optionsfile = new FileUri(settings.TemplateDir.UrlFolder + prefix + "options.json");
                    try
                    {
                        File.WriteAllText(datafile.PhysicalFilePath, data);
                        File.WriteAllText(schemafile.PhysicalFilePath, schema);
                        File.WriteAllText(optionsfile.PhysicalFilePath, options);
                    }
                    catch (Exception ex)
                    {
                        string mess = string.Format("Error while saving file [{0}]", datafile.FilePath);
                        Log.Logger.Error(mess, ex);
                        throw new Exception(mess, ex);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Lookups the data for Additional Data.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage LookupData(LookupDataRequestDTO req)
        {
            OpenContentSettings settings = ActiveModule.OpenContentSettings();
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                ModuleController mc = new ModuleController();
                module = mc.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = settings.Template.Manifest;
            string key = req.dataKey;
            var dataManifest = manifest.GetAdditionalData(key);
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    PortalId = PortalSettings.PortalId,
                    TabId = ActiveModule.TabID,
                    ModuleId = module.ModuleID,
                    TabModuleId = ActiveModule.TabModuleID,
                    UserId = UserInfo.UserID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig,
                    //Options = reqOptions
                };
                var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key);
                if (dsItem != null)
                {
                    JToken json = dsItem.Data;
                    if (!string.IsNullOrEmpty(req.dataMember))
                    {
                        json = json[req.dataMember];
                    }
                    if (json is JArray)
                    {
                        if (LocaleController.Instance.GetLocales(PortalSettings.PortalId).Count > 1)
                        {
                            JsonUtils.SimplifyJson(json, DnnLanguageUtils.GetCurrentCultureCode());
                        }
                        AddLookupItems(req.valueField, req.textField, req.childrenField, res, json as JArray);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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
                    AddLookupItems(valueField, textField, childrenField, res, childJson, prefix + "..");
                }
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage Lookup(LookupRequestDTO req)
        {
            ModuleController mc = new ModuleController();
            var module = mc.GetModule(req.moduleid, req.tabid, false);
            if (module == null) throw new Exception(string.Format("Can not find ModuleInfo (tabid:{0}, moduleid:{1})", req.tabid, req.moduleid));

            var settings = module.OpenContentSettings();
            Manifest.Manifest manifest = settings.Manifest;
            TemplateManifest templateManifest = settings.Template;
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    ActiveModuleId = ActiveModule.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig
                };

                if (listMode)
                {
                    var items = ds.GetAll(dsContext, null).Items;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            res.Add(new LookupResultDTO()
                            {
                                value = item.Id, //todo user valuefield
                                text = item.Title //todo user textfield
                            });
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
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /*
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage List(ListDTO req)
        {
            OpenContentSettings settings = ActiveModule.OpenContentSettings();
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                ModuleController mc = new ModuleController();
                module = mc.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = settings.Template.Manifest;
            TemplateManifest templateManifest = settings.Template;
            string editRole = manifest.GetEditRole();
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            JArray json = new JArray();
            try
            {
                if (listMode)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);

                    var docs = LuceneController.Instance.Search(module.ModuleID.ToString(), "Title", req.query, "", "", 10, 0, indexConfig);
                    foreach (var item in docs.ids)
                    {
                        var content = GetContent(module.ModuleID, listMode, int.Parse(item));
                        if (content != null)
                        {
                            json.Add(content.Json.ToJObject("GetContent " + item));
                        }
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        */
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage EditSettings(string key)
        {
            string data = (string)ActiveModule.ModuleSettings[key];
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                var fb = new FormBuilder(settings.TemplateDir);
                JObject json = fb.BuildForm(key);
                var dataJson = data.ToJObject("Raw settings json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
                var dataJson = JsonUtils.LoadJsonFromFile(settings.TemplateDir.UrlFolder + prefix + "builder.json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
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

