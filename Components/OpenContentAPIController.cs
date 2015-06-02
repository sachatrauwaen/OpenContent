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
            string Template = (string)ActiveModule.ModuleSettings["template"];
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
                var struc = ctrl.GetFirstContent(ModuleId);
                if (struc != null)                
                {
                    json["data"] = JObject.Parse(struc.Json);
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
            int ModuleId = ActiveModule.ModuleID;
            OpenContentController ctrl = new OpenContentController();
            var content = ctrl.GetFirstContent(ModuleId);
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

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Tabs(string q)
        {
            try
            {
                var tabs = TabController.GetTabsBySortOrder(PortalSettings.PortalId).Where(t => t.ParentId != PortalSettings.AdminTabId).Where(t => t.TabName.ToLower().Contains(q.ToLower())).Select(t => new { name = t.TabName + " (" + t.TabPath.Replace("//", "/").Replace("/" + t.TabName + "/", "") + ")", value = (new Uri(Globals.NavigateURL(t.TabID))).PathAndQuery });
                return Request.CreateResponse(HttpStatusCode.OK, tabs);
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
        public HttpResponseMessage Images(string q, string d)
        {
            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                var files = folderManager.GetFiles(portalFolder, true);
                files = files.Where(f => IsImageFile(f));
                if (q != "*")
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                files = files.Where(f => IsImageFile(f)).Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                var res = files.Select(f => new { value = PortalSettings.HomeDirectory + f.RelativePath, name = f.FileName + " (" + f.Folder + ")" });
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private bool IsImageFile(IFileInfo file)
        {
            return (Globals.glbImageFileTypes + ",").IndexOf(file.Extension.ToLower().Replace(".", "") + ",") > -1;
        }
    }
}

