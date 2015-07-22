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
    
    public class DnnEntitiesAPIController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnEntitiesAPIController));

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
                //files = files.Where(f => IsImageFile(f)).Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                var res = files.Select(f => new { value = PortalSettings.HomeDirectory + f.RelativePath, name = f.FileName + " (" + f.Folder + ")" });
                return Request.CreateResponse(HttpStatusCode.OK, res);
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
        public HttpResponseMessage Files(string q, string d)
        {
            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                var files = folderManager.GetFiles(portalFolder, true);
                //files = files.Where(f => IsImageFile(f));
                if (q != "*")
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                //files = files.Where(f => IsImageFile(f)).Where(f => f.FileName.ToLower().Contains(q.ToLower()));
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

