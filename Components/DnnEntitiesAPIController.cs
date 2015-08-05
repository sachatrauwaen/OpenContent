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
using System.Drawing;
using Satrabel.OpenContent.Components.Images;
using System.Drawing.Imaging;

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

        public HttpResponseMessage CropImage(CropDTO cropData)
        {
            FilesStatus fs = null;
            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(ActiveModule.PortalID, "");
                string RawImageUrl = cropData.RawImageUrl;
                if (RawImageUrl.IndexOf('?') > 0)
                {
                    RawImageUrl = RawImageUrl.Substring(0, RawImageUrl.IndexOf('?'));
                }
                RawImageUrl = RawImageUrl.Replace(PortalSettings.HomeDirectory, "");

                var file = fileManager.GetFile(ActiveModule.PortalID, RawImageUrl);
                
                if (file != null)
                {
                    var folder = folderManager.GetFolder(file.FolderId);
                    var image = Image.FromFile(file.PhysicalPath);
                    var imageCropped = ImageUtils.Crop(image, cropData.x, cropData.y, cropData.width, cropData.height);
                    string newFilename = "cropped-" + file.FileName;

                    Stream content = new MemoryStream();
                    ImageFormat imgFormat = ImageFormat.Bmp;
                    if (file.Extension.ToLowerInvariant() == ".png")
                    {
                        imgFormat = ImageFormat.Png;
                    }
                    else if (file.Extension.ToLowerInvariant() == ".gif")
                    {
                        imgFormat = ImageFormat.Gif;
                    }
                    else if (file.Extension.ToLowerInvariant() == ".jpg")
                    {
                        imgFormat = ImageFormat.Jpeg;
                    }

                    imageCropped.Save(content, imgFormat);

                    var newFile = fileManager.AddFile(folder, newFilename, content, true);
                    fs = new FilesStatus()
                    {
                        success = true,
                        name = newFile.FileName,
                        extension = newFile.Extension,
                        type = newFile.ContentType,
                        size = newFile.Size,
                        progress = "1.0",
                        url = FileManager.Instance.GetUrl(newFile),
                        //thumbnail_url = fileIcon,
                        message = "success",
                        id = newFile.FileId,

                    };

                }


                return Request.CreateResponse(HttpStatusCode.OK, fs);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        public class CropDTO
        {
            public string RawImageUrl { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int rotate { get; set; }
        }
    }
}

