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
            string Template = (string)ActiveModule.ModuleSettings["template"];
            var manifest = OpenContentUtils.GetTemplateManifest(Template);
            bool ListMode = manifest != null && manifest.IsListTemplate;
            JObject json = new JObject();
            try
            {
                string TemplateFilename = HostingEnvironment.MapPath("~/" + Template);
                // schema
                string schemaFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "schema.json";
                JObject schemaJson = JObject.Parse(File.ReadAllText(schemaFilename));
                json["schema"] = schemaJson;
                // default options
                string optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        JObject optionsJson = JObject.Parse(fileContent);
                        json["options"] = optionsJson;
                    }
                }
                // language options
                optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "options." + PortalSettings.CultureCode + ".json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        JObject optionsJson = JObject.Parse(fileContent);
                        json["options"] = json["options"].JsonMerge(optionsJson);
                    }
                }
                // view
                string viewFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + "view.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(viewFilename);
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
                int ModuleId = ActiveModule.ModuleID;
                OpenContentController ctrl = new OpenContentController();
                if (ListMode)
                {
                    if (id > 0)
                    {
                        var struc = ctrl.GetContent(id, ModuleId);
                        if (struc != null)
                        {
                            json["data"] = JObject.Parse(struc.Json);
                        }
                    }
                }
                else
                {
                    var struc = ctrl.GetFirstContent(ModuleId);
                    if (struc != null)
                    {
                        json["data"] = JObject.Parse(struc.Json);
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
            string Data = (string)ActiveModule.ModuleSettings["data"];
            JObject json = new JObject();
            try
            {
                string TemplateFilename = HostingEnvironment.MapPath("~/" + Template);
                string prefix = Path.GetFileNameWithoutExtension(TemplateFilename) + "-";
                // schema
                string schemaFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + prefix + "schema.json";
                /*
                if (!File.Exists(schemaFilename))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "schema.json missing for template " + Template);
                }
                 */
                if (File.Exists(schemaFilename))
                {
                    JObject schemaJson = JObject.Parse(File.ReadAllText(schemaFilename));
                    json["schema"] = schemaJson;
                    if (!string.IsNullOrEmpty(Data))
                    {
                        json["data"] = JObject.Parse(Data);
                    }
                }
                // default options
                string optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + prefix + "options.json";
                if (File.Exists(optionsFilename))
                {
                    JObject optionsJson = JObject.Parse(File.ReadAllText(optionsFilename));
                    json["options"] = optionsJson;
                }
                // language options
                optionsFilename = Path.GetDirectoryName(TemplateFilename) + "\\" + prefix + "options." + PortalSettings.CultureCode + ".json";
                if (File.Exists(optionsFilename))
                {
                    JObject optionsJson = JObject.Parse(File.ReadAllText(optionsFilename));
                    json["options"] = json["options"].JsonMerge(optionsJson);
                }
                if (!string.IsNullOrEmpty(Data))
                {
                    try
                    {
                        json["data"] = JObject.Parse(Data);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Settings Json Data : " + Data, ex);
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
                int ModuleId = ActiveModule.ModuleID;
                string Template = (string)ActiveModule.ModuleSettings["template"];
                var manifest = OpenContentUtils.GetTemplateManifest(Template);
                bool ListMode = manifest != null && manifest.IsListTemplate;
                OpenContentController ctrl = new OpenContentController();
                OpenContentInfo content = null;
                if (ListMode)
                {
                    int ItemId;
                    if (int.TryParse(json["id"].ToString(), out ItemId))
                    {
                        content = ctrl.GetContent(ItemId, ModuleId);
                    }
                }
                else
                {
                    content = ctrl.GetFirstContent(ModuleId);
                }

                if (content == null)
                {
                    content = new OpenContentInfo()
                    {
                        ModuleId = ModuleId,
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
                int ModuleId = ActiveModule.ModuleID;

                var data = json["data"].ToString();
                var template = json["template"].ToString();

                ModuleController mc = new ModuleController();
                mc.UpdateModuleSetting(ModuleId, "template", template);
                mc.UpdateModuleSetting(ModuleId, "data", data);
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
            string Template = (string)module.ModuleSettings["template"];
            var manifest = OpenContentUtils.GetTemplateManifest(Template);
            bool ListMode = manifest != null && manifest.IsListTemplate;
            //JToken json = new JObject();
            List<LookupResultDTO> res = new List<LookupResultDTO>();
            try
            {
                OpenContentController ctrl = new OpenContentController();
                if (ListMode)
                {
                    var items = ctrl.GetContents(req.moduleid);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            res.Add(new LookupResultDTO(){
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
                                        value = item[req.valueField] == null ? "": item[req.valueField].ToString(),
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

