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

#endregion

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class OpenContentAPIController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OpenContentAPIController));
        public string BaseDir
        {
            get
            {
                return PortalSettings.HomeDirectory + "/OpenContent/Templates/";
            }
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Edit()
        {
            return Edit(-1);
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Edit(int id)
        {
            FileUri template = OpenContentUtils.GetTemplate(ActiveModule.ModuleSettings);
            var manifest = OpenContentUtils.GetTemplateManifest(template);
            bool listMode = manifest != null && manifest.IsListTemplate;
            JObject json = new JObject();
            try
            {
                // schema
                var schemaFilename = new FileUri(template.Directory +  "schema.json");
                if (schemaFilename.FileExists)
                {
                    JObject schemaJson = JObject.Parse(File.ReadAllText(schemaFilename.PhysicalFilePath));
                    json["schema"] = schemaJson;
                }
                else
                {
                    //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "schema.json missing for template " + Template);
                }
                // default options
                var optionsFilename = new FileUri(template.Directory + "options.json"); 
                if (optionsFilename.FileExists)
                {
                    string fileContent = File.ReadAllText(optionsFilename.PhysicalFilePath);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        JObject optionsJson = JObject.Parse(fileContent);
                        json["options"] = optionsJson;
                    }
                }
                // language options
                optionsFilename = new FileUri(template.Directory + "options." + PortalSettings.CultureCode + ".json");
                if (optionsFilename.FileExists)
                {
                    string fileContent = File.ReadAllText(optionsFilename.PhysicalFilePath);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        JObject optionsJson = JObject.Parse(fileContent);
                        json["options"] = json["options"].JsonMerge(optionsJson);
                    }
                }
                // view
                var viewFilename = new FileUri(template.Directory + "view.json");
                if (optionsFilename.FileExists)
                {
                    string fileContent = File.ReadAllText(viewFilename.PhysicalFilePath);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        JObject optionsJson = JObject.Parse(fileContent);
                        json["view"] = optionsJson;
                    }
                }
                // template options
                /*
                optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "options." + Path.GetFileNameWithoutExtension(TemplateFilename) + ".json";
                if (File.Exists(optionsFilename))
                {
                    JObject optionsJson = JObject.Parse(File.ReadAllText(optionsFilename));
                    json["options"] = json["options"].JsonMerge(optionsJson);
                }
                 */
                int moduleId = ActiveModule.ModuleID;
                OpenContentController ctrl = new OpenContentController();
                if (listMode)
                {
                    if (id > 0)
                    {
                        var struc = ctrl.GetContent(id, moduleId);
                        if (struc != null)
                        {
                            json["data"] = JObject.Parse(struc.Json);
                            AddVersions(json, struc);
                        }
                    }
                }
                else
                {
                    var struc = ctrl.GetFirstContent(moduleId);
                    if (struc != null)
                    {
                        json["data"] = JObject.Parse(struc.Json);
                        AddVersions(json, struc);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Version(int id, string ticks)
        {
            FileUri template = OpenContentUtils.GetTemplate(ActiveModule.ModuleSettings);
            var manifest = OpenContentUtils.GetTemplateManifest(template);
            bool listMode = manifest != null && manifest.IsListTemplate;
            JObject json = new JObject();
            try
            {
                int moduleId = ActiveModule.ModuleID;
                OpenContentController ctrl = new OpenContentController();
                if (listMode)
                {
                    if (id > 0)
                    {
                        var struc = ctrl.GetContent(id, moduleId);
                        if (struc != null)
                        {
                            if (!string.IsNullOrEmpty(struc.VersionsJson))
                            {
                                var ver = struc.Versions.Single(v => v.CreatedOnDate.Ticks.ToString() == ticks);
                                json = ver.Json;
                            }
                        }
                    }
                }
                else
                {
                    var struc = ctrl.GetFirstContent(moduleId);
                    if (struc != null)
                    {
                        if (!string.IsNullOrEmpty(struc.VersionsJson))
                        {
                            var ver = struc.Versions.Single(v => v.CreatedOnDate.Ticks.ToString() == ticks);
                            json = ver.Json;
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Settings(string Template)
        {
            string data = (string)ActiveModule.ModuleSettings["data"];
            JObject json = new JObject();
            try
            {
                var templateUri = new FileUri(Template);
                string prefix = templateUri.FileNameWithoutExtension + "-";

                // schema
                var schemaFilename = new FileUri(templateUri.Directory + prefix + "schema.json");
                if (schemaFilename.FileExists)
                {
                    JObject schemaJson = JObject.Parse(File.ReadAllText(schemaFilename.PhysicalFilePath));
                    json["schema"] = schemaJson;
                    if (!string.IsNullOrEmpty(data))
                    {
                        json["data"] = JObject.Parse(data);
                    }
                }
                else 
                {
                    //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "schema.json missing for template " + Template);
                }
                // default options
                var optionsFilename = new FileUri(templateUri.Directory +  prefix + "options.json");
                if (optionsFilename.FileExists)
                {
                    JObject optionsJson = JObject.Parse(File.ReadAllText(optionsFilename.PhysicalFilePath));
                    json["options"] = optionsJson;
                }
                // language options
                optionsFilename = new FileUri(templateUri.Directory + prefix + "options." + PortalSettings.CultureCode + ".json");
                if (optionsFilename.FileExists)
                {
                    JObject optionsJson = JObject.Parse(File.ReadAllText(optionsFilename.PhysicalFilePath));
                    json["options"] = json["options"].JsonMerge(optionsJson);
                }
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        json["data"] = JObject.Parse(data);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Settings Json Data : " + data, ex);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Update(JObject json)
        {
            try
            {
                int moduleId = ActiveModule.ModuleID;
                FileUri template = OpenContentUtils.GetTemplate(ActiveModule.ModuleSettings);
                var manifest = OpenContentUtils.GetTemplateManifest(template);
                bool listMode = manifest != null && manifest.IsListTemplate;
                OpenContentController ctrl = new OpenContentController();
                OpenContentInfo content = null;
                if (listMode)
                {
                    int ItemId;
                    if (int.TryParse(json["id"].ToString(), out ItemId))
                    {
                        content = ctrl.GetContent(ItemId, moduleId);
                    }
                }
                else
                {
                    content = ctrl.GetFirstContent(moduleId);
                }

                if (content == null)
                {
                    content = new OpenContentInfo()
                    {
                        ModuleId = moduleId,
                        Title = json["form"]["Title"] == null ? ActiveModule.ModuleTitle : json["form"]["Title"].ToString(),
                        Json = json["form"].ToString(),
                        CreatedByUserId = UserInfo.UserID,
                        CreatedOnDate = DateTime.Now,
                        LastModifiedByUserId = UserInfo.UserID,
                        LastModifiedOnDate = DateTime.Now,
                        Html = "",
                    };
                    ctrl.AddContent(content);
                }
                else
                {
                    content.Title = json["form"]["Title"] == null ? ActiveModule.ModuleTitle : json["form"]["Title"].ToString();
                    content.Json = json["form"].ToString();
                    content.LastModifiedByUserId = UserInfo.UserID;
                    content.LastModifiedOnDate = DateTime.Now;
                    ctrl.UpdateContent(content);
                }
                if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                {
                    string ModuleTitle = json["form"]["ModuleTitle"].ToString();
                    OpenContentUtils.UpdateModuleTitle(ActiveModule, ModuleTitle);
                }
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        public HttpResponseMessage UpdateSettings(JObject json)
        {
            try
            {
                int moduleId = ActiveModule.ModuleID;
                var data = json["data"].ToString();
                var template = json["template"].ToString();

                var mc = new ModuleController();
                mc.UpdateModuleSetting(moduleId, "template", template);
                mc.UpdateModuleSetting(moduleId, "data", data);
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage Lookup(LookupRequestDTO req)
        {
            ModuleController mc = new ModuleController();
            var module = mc.GetModule(req.moduleid, req.tabid, false);
            FileUri template = OpenContentUtils.GetTemplate(module.ModuleSettings);
            var manifest = OpenContentUtils.GetTemplateManifest(template);
            bool listMode = manifest != null && manifest.IsListTemplate;
            //JToken json = new JObject();
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                OpenContentController ctrl = new OpenContentController();
                if (listMode)
                {
                    var items = ctrl.GetContents(req.moduleid);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            res.Add(new LookupResultDTO()
                            {
                                value = item.ContentId.ToString(),
                                text = item.Title
                            });
                        }
                    }
                }
                else
                {
                    var struc = ctrl.GetFirstContent(req.moduleid);
                    if (struc != null)
                    {

                        JToken json = JObject.Parse(struc.Json);
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
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }

    public class LookupRequestDTO
    {
        public int moduleid { get; set; }
        public int tabid { get; set; }
        public string dataMember { get; set; }
        public string valueField { get; set; }
        public string textField { get; set; }
    }
    public class LookupResultDTO
    {
        public string value { get; set; }
        public string text { get; set; }
    }
}

