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

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Edit()
        {
            return Edit(-1);
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Edit(int id)
        {
            OpenContentSettings settings = new OpenContentSettings(ActiveModule.ModuleSettings);
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                module = ModuleController.Instance.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = OpenContentUtils.GetManifest(settings.Template.Directory);
            TemplateManifest templateManifest = null;
            if (manifest != null)
            {
                templateManifest = manifest.GetTemplateManifest(settings.Template);
            }
            string editRole = manifest == null ? "" : manifest.EditRole;
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            JObject json = new JObject();
            try
            {
                // schema
                var schemaFilename = new FileUri(settings.Template.Directory + "schema.json");
                if (schemaFilename.FileExists)
                {
                    JObject schemaJson = schemaFilename.ToJObject();
                    json["schema"] = schemaJson;
                }
                else
                {
                    //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "schema.json missing for template " + Template);
                }
                // default options
                var optionsFilename = new FileUri(settings.Template.Directory + "options.json");
                var optionsJson = optionsFilename.ToJObject();
                if (optionsJson != null)
                    json["options"] = optionsJson;

                // language options
                optionsFilename = new FileUri(settings.Template.Directory + "options." + PortalSettings.CultureCode + ".json");
                optionsJson = optionsFilename.ToJObject();
                if (optionsJson != null)
                    json["options"] = json["options"].JsonMerge(optionsJson);

                // view
                var viewFilename = new FileUri(settings.Template.Directory + "view.json");
                optionsJson = viewFilename.ToJObject();
                if (optionsJson != null)
                    json["view"] = optionsJson;

                int CreatedByUserid = -1;
                var content = GetContent(module.ModuleID, listMode, id);
                if (content != null)
                {
                    json["data"] = content.Json.ToJObject("GetContent " + id);
                    AddVersions(json, content);
                    CreatedByUserid = content.CreatedByUserId;
                }

                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, CreatedByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private OpenContentInfo GetContent(int moduleId, bool listMode, int id)
        {
            OpenContentController ctrl = new OpenContentController();
            if (listMode)
            {
                if (id > 0)
                {
                    return ctrl.GetContent(id, moduleId);
                }
            }
            else
            {
                return ctrl.GetFirstContent(moduleId);

            }
            return null;
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Version(int id, string ticks)
        {
            //FileUri template = OpenContentUtils.GetTemplate(ActiveModule.ModuleSettings);
            OpenContentSettings settings = new OpenContentSettings(ActiveModule.ModuleSettings);
            ModuleInfo module = ActiveModule;
            if (settings.ModuleId > 0)
            {
                module = ModuleController.Instance.GetModule(settings.ModuleId, settings.TabId, false);
            }
            var manifest = OpenContentUtils.GetManifest(settings.Template.Directory);
            TemplateManifest templateManifest = null;
            if (manifest != null)
            {
                templateManifest = manifest.GetTemplateManifest(settings.Template);
            }
            string editRole = manifest == null ? "" : manifest.EditRole;
            bool listMode = templateManifest != null && templateManifest.IsListTemplate;
            JObject json = new JObject();
            try
            {
                int CreatedByUserid = -1;
                var content = GetContent(module.ModuleID, listMode, id);
                if (content != null)
                {
                    if (!string.IsNullOrEmpty(content.VersionsJson))
                    {
                        var ver = content.Versions.Single(v => v.CreatedOnDate.Ticks.ToString() == ticks);
                        json = ver.Json;

                    }
                    CreatedByUserid = content.CreatedByUserId;
                }
                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, CreatedByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
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
                JObject schemaJson = schemaFilename.ToJObject();
                if (schemaJson != null)
                    json["schema"] = schemaJson;

                // default options
                var optionsFilename = new FileUri(templateUri.Directory + prefix + "options.json");
                JObject optionsJson = optionsFilename.ToJObject();
                if (optionsJson != null)
                    json["options"] = optionsJson;

                // language options
                optionsFilename = new FileUri(templateUri.Directory + prefix + "options." + PortalSettings.CultureCode + ".json");
                optionsJson = optionsFilename.ToJObject();
                if (optionsJson != null)
                    json["options"] = json["options"].JsonMerge(optionsJson);


                JObject dataJson = data.ToJObject("Raw settings json");
                if (dataJson != null)
                    json["data"] = dataJson;

                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
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
                OpenContentSettings settings = new OpenContentSettings(ActiveModule.ModuleSettings);
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    module = ModuleController.Instance.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = OpenContentUtils.GetManifest(settings.Template.Directory);
                TemplateManifest templateManifest = null;
                if (manifest != null)
                {
                    templateManifest = manifest.GetTemplateManifest(settings.Template);
                }
                string editRole = manifest == null ? "" : manifest.EditRole;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                OpenContentController ctrl = new OpenContentController();
                OpenContentInfo content = null;
                if (listMode)
                {
                    int itemId;
                    if (json["id"] != null && int.TryParse(json["id"].ToString(), out itemId))
                    {
                        content = ctrl.GetContent(itemId, module.ModuleID);
                        if (content != null)
                            createdByUserid = content.CreatedByUserId;
                    }
                }
                else
                {
                    content = ctrl.GetFirstContent(module.ModuleID);
                    if (content != null)
                        createdByUserid = content.CreatedByUserId;
                }

                if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, createdByUserid))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                if (content == null)
                {
                    content = new OpenContentInfo()
                    {
                        ModuleId = module.ModuleID,
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

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Delete(JObject json)
        {
            try
            {
                OpenContentSettings settings = new OpenContentSettings(ActiveModule.ModuleSettings);
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    module = ModuleController.Instance.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = OpenContentUtils.GetManifest(settings.Template.Directory);
                TemplateManifest templateManifest = null;
                if (manifest != null)
                {
                    templateManifest = manifest.GetTemplateManifest(settings.Template);
                }
                string editRole = manifest == null ? "" : manifest.EditRole;
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int CreatedByUserid = -1;
                OpenContentController ctrl = new OpenContentController();
                OpenContentInfo content = null;
                if (listMode)
                {
                    int ItemId;
                    if (int.TryParse(json["id"].ToString(), out ItemId))
                    {
                        content = ctrl.GetContent(ItemId, module.ModuleID);
                        if (content != null)
                        {
                            CreatedByUserid = content.CreatedByUserId;
                        }
                    }
                }
                else
                {
                    content = ctrl.GetFirstContent(module.ModuleID);
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
                    ctrl.DeleteContent(content);
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

                        JToken json = struc.Json.ToJObject("GetFirstContent data of moduleId " + req.moduleid);
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

