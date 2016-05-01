#region Copyright

// 
// Copyright (c) 2015
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
using System.Web.Hosting;
using System.IO;
using DotNetNuke.Instrumentation;
using Satrabel.OpenContent.Components;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Common;
using DotNetNuke.Services.FileSystem;
using System.Collections.Generic;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;

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
            OpenContentSettings settings = ActiveModule.OpenContentSettings();
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                ModuleController mc = new ModuleController();
                module = mc.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = settings.Manifest;
            TemplateManifest templateManifest = settings.Template;
            string editRole = manifest == null ? "" : manifest.EditRole;
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            try
            {
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
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
                            json["data"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()] = ActiveModule.ModuleTitle;
                        }
                    }
                    var versions = ds.GetVersions(dsContext, dsItem);
                    if (versions != null)
                    {
                        json["versions"]= versions;
                    }
                    //AddVersions(json, content);
                    //createdByUserid = content.CreatedByUserId;
                    createdByUserid = dsItem.CreatedByUserId;
                }

                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, createdByUserid))
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
            OpenContentSettings settings = ActiveModule.OpenContentSettings();
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                ModuleController mc = new ModuleController();
                module = mc.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = settings.Manifest;
            TemplateManifest templateManifest = settings.Template;
            var dataManifest = manifest.AdditionalData[key];
            string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, module.ModuleID, ActiveModule.TabModuleID);
            try
            {
                var templateFolder = string.IsNullOrEmpty(dataManifest.TemplateFolder) ? settings.TemplateDir : settings.TemplateDir.ParentFolder.Append(dataManifest.TemplateFolder);
                var fb = new FormBuilder(templateFolder);
                JObject json = fb.BuildForm(key);
                int createdByUserid = -1;
                var dc = new AdditionalDataController();
                var data = dc.GetData(scope, dataManifest.StorageKey ?? key);
                if (data != null)
                {
                    json["data"] = data.Json.ToJObject("GetContent " + scope + "/" + key);
                    AddVersions(json, data);
                    createdByUserid = data.CreatedByUserId;
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
                TemplateManifest templateManifest = settings.Template;
                string key = json["key"].ToString();
                var dataManifest = manifest.AdditionalData[key];
                string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, module.ModuleID, ActiveModule.TabModuleID);
                AdditionalDataController ctrl = new AdditionalDataController();
                AdditionalDataInfo data = ctrl.GetData(scope, dataManifest.StorageKey ?? key);
                if (data == null)
                {
                    data = new AdditionalDataInfo()
                    {
                        Scope = scope,
                        DataKey = dataManifest.StorageKey ?? key,
                        Json = json["form"].ToString(),
                        CreatedByUserId = UserInfo.UserID,
                        CreatedOnDate = DateTime.Now,
                        LastModifiedByUserId = UserInfo.UserID,
                        LastModifiedOnDate = DateTime.Now,
                    };
                    ctrl.AddData(data);
                }
                else
                {
                    data.Json = json["form"].ToString();
                    data.LastModifiedByUserId = UserInfo.UserID;
                    data.LastModifiedOnDate = DateTime.Now;
                    ctrl.UpdateData(data);
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
            var templateManifest = settings.Template;
            string editRole = manifest == null ? "" : manifest.EditRole;
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            JToken json = new JObject();
            try
            {
                int CreatedByUserid = -1;
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
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
                        CreatedByUserid = dsItem.CreatedByUserId;
                    }
                }
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, CreatedByUserid))
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
            string Template = (string)ActiveModule.ModuleSettings["template"];
            try
            {
                var templateUri = new FileUri(Template);
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
                    if (json["id"] != null)
                    {
                        itemId = json["id"].ToString();
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
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, createdByUserid))
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
                    string moduleTitle = json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                    OpenContentUtils.UpdateModuleTitle(ActiveModule, moduleTitle);
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
                string editRole = manifest == null ? "" : manifest.EditRole;
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int CreatedByUserid = -1;
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
                IDataItem content = null;
                if (listMode)
                {
                        content = ds.Get(dsContext, json["id"].ToString());
                        if (content != null)
                        {
                            CreatedByUserid = content.CreatedByUserId;
                        }
                }
                else
                {
                    dsContext.Single = true;
                    content = ds.Get(dsContext, null);
                    if (content != null)
                    {
                        CreatedByUserid = content.CreatedByUserId;
                    }
                }
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, CreatedByUserid))
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
            TemplateManifest templateManifest = settings.Template;
            string key = req.dataKey;
            var dataManifest = manifest.AdditionalData[key];
            string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, module.ModuleID, ActiveModule.TabModuleID);
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                AdditionalDataController ctrl = new AdditionalDataController();
                AdditionalDataInfo data = ctrl.GetData(scope, dataManifest.StorageKey ?? key);
                if (data != null)
                {

                    JToken json = data.Json.ToJObject("Get data of  " + req.dataKey);
                    if (!string.IsNullOrEmpty(req.dataMember))
                    {
                        json = json[req.dataMember];
                    }
                    if (json is JArray)
                    {
                        AddLookupItems(req.valueField, req.textField, req.childrenField, res, json as JArray);
                    }
                    /*
                    else if (json is JObject)
                    {
                        foreach (var item in json.Children<JProperty>())
                        {
                            res.Add(new LookupResultDTO()
                            {
                                value = dataManifest.ModelKey ?? key +"/"+item.Name,
                                text = item.Value[req.textField] == null ? "" : item.Value[req.textField].ToString()
                            });
                        }
                    }
                     */
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
                    text = item[textField] == null ? "" : prefix+item[textField].ToString()
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
            var settings = module.OpenContentSettings();
            Manifest.Manifest manifest = settings.Manifest;
            TemplateManifest templateManifest = settings.Template;
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            //JToken json = new JObject();
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig
                };
                //var dsItem = ds.GetEdit(dsContext, id);
                
                if (listMode)
                {
                    var items = ds.GetAll(dsContext, null).Items;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            res.Add(new LookupResultDTO()
                            {
                                value = item.Id,
                                text = item.Title
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
            string editRole = manifest == null ? "" : manifest.EditRole;
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

