#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
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
using System.Drawing.Imaging;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.TemplateHelpers;

#endregion

namespace Satrabel.OpenContent.Components
{

    public class DnnEntitiesAPIController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnEntitiesAPIController));

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Tabs(string q, string l)
        {
            try
            {
                var tabs = TabController.GetTabsBySortOrder(PortalSettings.PortalId).Where(t => t.ParentId != PortalSettings.AdminTabId).Where(t => t.TabName.ToLower().Contains(q.ToLower())).Select(t => new { name = t.TabName + " (" + t.TabPath.Replace("//", "/").Replace("/" + t.TabName + "/", "") + " " + l + ")", value = (new System.Uri(NavigateUrl(t, l, PortalSettings))).PathAndQuery });
                return Request.CreateResponse(HttpStatusCode.OK, tabs);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static string NavigateUrl(TabInfo t, string culture, PortalSettings portalsettings)
        {
            return Globals.NavigateURL(t.TabID, false, portalsettings, "", culture);
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Images(string q, string d)
        {
            try
            {
                var folderManager = FolderManager.Instance;
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
        /// <summary>
        /// Imageses the lookup.
        /// </summary>
        /// <param name="q">The string that should be Contained in the name of the file (case insensitive). Use * to get all the files.</param>
        /// <param name="d">The Folder path to retrieve</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage ImagesLookup(string q, string d)
        {
            try
            {
                if (string.IsNullOrEmpty(d))
                {
                    var exc = new ArgumentException("Folder path not specified. Missing ['folder': 'FolderPath'] in optionfile? ");
                    Logger.Error(exc);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
                }

                var folderManager = FolderManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                if (portalFolder==null)
                {
                    var exc = new ArgumentException("Folder path not found. Adjust ['folder': 'FolderPath'] in optionfile. ");
                    Logger.Error(exc);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
                }
                var files = folderManager.GetFiles(portalFolder, true);
                files = files.Where(f => IsImageFile(f));
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                int folderLength = d.Length;

                var res = files.Select(f => new { value = f.FileId.ToString(), url = ImageHelper.GetImageUrl(f, new Ratio(40, 40)), text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName })
                               .Take(100);

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
        public HttpResponseMessage FilesLookup(string q, string d)
        {
            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                var files = folderManager.GetFiles(portalFolder, true);
                //files = files.Where(f => IsImageFile(f));
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                int folderLength = d.Length;
                var res = files.Select(f => new { value = f.FileId.ToString(), url = fileManager.GetUrl(f), text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName /*+ (string.IsNullOrEmpty(f.Folder) ? "" : " (" + f.Folder.Trim('/') + ")")*/ });
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
        public HttpResponseMessage TabsLookup(string q, string l)
        {
            try
            {
                var tabs = TabController.GetTabsBySortOrder(PortalSettings.PortalId)
                            .Where(t => t.ParentId != PortalSettings.AdminTabId);
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    tabs = tabs.Where(t => t.TabName.ToLower().Contains(q.ToLower()));
                }
                var tabsDtos = tabs.Select(t => new { value = t.TabID.ToString(), text = t.TabName + " (" + t.TabPath.Replace("//", "/").Replace("/" + t.TabName + "/", "") + " " + l + ")", url = (new System.Uri(NavigateUrl(t, l, PortalSettings))).PathAndQuery });
                return Request.CreateResponse(HttpStatusCode.OK, tabsDtos);
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
        public HttpResponseMessage FileUrl(int fileid)
        {
            try
            {
                var fileManager = FileManager.Instance;
                IFileInfo File = fileManager.GetFile(fileid);
                return Request.CreateResponse(HttpStatusCode.OK, fileManager.GetUrl(File));
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

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage CropImage(CropResizeDTO cropData)
        {
            FilesStatus fs = null;
            try
            {
                var res = new CropResizeResultDTO()
                {
                    crop = new CropDTO()
                    {
                        x = cropData.crop.x,
                        y = cropData.crop.y,
                        width = cropData.crop.width,
                        height = cropData.crop.height
                    }
                };
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;


                string RawImageUrl = cropData.url;
                if (RawImageUrl.IndexOf('?') > 0)
                {
                    RawImageUrl = RawImageUrl.Substring(0, RawImageUrl.IndexOf('?'));
                }
                RawImageUrl = RawImageUrl.Replace(PortalSettings.HomeDirectory, "");
                var file = fileManager.GetFile(ActiveModule.PortalID, RawImageUrl);

                string cropfolder = "OpenContent/Files/" + ActiveModule.ModuleID;

                if (!string.IsNullOrEmpty(cropData.cropfolder))
                {
                    cropfolder = cropData.cropfolder;
                }
                var userFolder = folderManager.GetFolder(PortalSettings.PortalId, cropfolder);
                if (userFolder == null)
                {
                    userFolder = folderManager.AddFolder(PortalSettings.PortalId, cropfolder);
                }
                string newFilename = Path.GetFileNameWithoutExtension(file.FileName) + "-" + cropData.id + Path.GetExtension(file.FileName);

                if (file != null)
                {
                    var folder = folderManager.GetFolder(file.FolderId);
                    var image = Image.FromFile(file.PhysicalPath);
                    Image imageCropped;
                    //int x = cropData.crop.x;
                    //int y = cropData.crop.y;
                    if (cropData.crop.x < 0 && cropData.crop.y < 0) // center
                    {
                        int left = 0;
                        int top = 0;
                        int width = 0;
                        int height = 0;
                        imageCropped = ImageHelper.SaveCroppedImage(image, cropData.crop.width, cropData.crop.height, out left, out top, out width, out height);
                        res.crop.x = left;
                        res.crop.y = top;
                        res.crop.width = width;
                        res.crop.height = height;
                    }
                    else
                    {
                        imageCropped = ImageHelper.Crop(image, cropData.crop.x, cropData.crop.y, cropData.crop.width, cropData.crop.height);
                        if (cropData.resize != null && cropData.resize.width > 0 && cropData.resize.height > 0)
                        {
                            imageCropped = ImageHelper.Resize(imageCropped, cropData.resize.width, cropData.resize.height);
                        }
                    }

                    Stream content = new MemoryStream();
                    ImageFormat imgFormat = ImageFormat.Bmp;
                    if (file.Extension.ToLowerInvariant() == "png")
                    {
                        imgFormat = ImageFormat.Png;
                    }
                    else if (file.Extension.ToLowerInvariant() == "gif")
                    {
                        imgFormat = ImageFormat.Gif;
                    }
                    else if (file.Extension.ToLowerInvariant() == "jpg")
                    {
                        imgFormat = ImageFormat.Jpeg;
                    }
                    imageCropped.Save(content, imgFormat);
                    var newFile = fileManager.AddFile(userFolder, newFilename, content, true);
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
                res.url = fs.url;
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
        [HttpPost]
        public HttpResponseMessage CropImages(CroppersDTO cropData)
        {
            FilesStatus fs = null;
            try
            {
                var res = new CroppersResultDTO();
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                string rawImageUrl = cropData.url;
                if (rawImageUrl.IndexOf('?') > 0)
                {
                    rawImageUrl = rawImageUrl.Substring(0, rawImageUrl.IndexOf('?'));
                }
                rawImageUrl = rawImageUrl.Replace(PortalSettings.HomeDirectory, "");
                var file = fileManager.GetFile(ActiveModule.PortalID, rawImageUrl);
                if (file != null)
                {
                    string cropfolder = "OpenContent/Files/" + ActiveModule.ModuleID;
                    if (!string.IsNullOrEmpty(cropData.cropfolder))
                    {
                        cropfolder = cropData.cropfolder;
                    }
                    var userFolder = folderManager.GetFolder(PortalSettings.PortalId, cropfolder);
                    if (userFolder == null)
                    {
                        userFolder = folderManager.AddFolder(PortalSettings.PortalId, cropfolder);
                    }
                    foreach (var cropper in cropData.croppers)
                    {
                        string key = cropper.Key;
                        string newFilename = Path.GetFileNameWithoutExtension(file.FileName) + "-" + key + Path.GetExtension(file.FileName);
                        var resizeInfo = cropper.Value;
                        CropDTO cropInfo = null;
                        if (cropData.cropdata.ContainsKey(key))
                        {
                            cropInfo = cropData.cropdata[key].cropper;
                        }
                        var cropResult = CropFile(file, newFilename, cropInfo, resizeInfo, userFolder);
                        res.cropdata.Add(key, cropResult);
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
        private CropResizeResultDTO CropFile(IFileInfo file, string newFilename, CropDTO crop, ResizeDTO resize, IFolderInfo userFolder)
        {
            var cropresult = new CropResizeResultDTO();
            if (crop != null)
            {
                cropresult.crop = crop;
            }
            else
            {
                cropresult.crop = new CropDTO();
            }

            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;

            var folder = folderManager.GetFolder(file.FolderId);
            var image = Image.FromFile(file.PhysicalPath);
            Image imageCropped = null;
            if (crop == null || crop.x < 0 || crop.y < 0) // center
            {
                int left = 0;
                int top = 0;
                int width = 0;
                int height = 0;
                imageCropped = ImageHelper.SaveCroppedImage(image, resize.width, resize.height, out left, out top, out width, out height);
                cropresult.crop.x = left;
                cropresult.crop.y = top;
                cropresult.crop.width = width;
                cropresult.crop.height = height;

            }
            else if (crop.width > 0 && crop.width > 0)
            {
                imageCropped = ImageHelper.Crop(image, crop.x, crop.y, crop.width, crop.height);
                if (resize != null && resize.width > 0 && resize.height > 0)
                {
                    imageCropped = ImageHelper.Resize(imageCropped, resize.width, resize.height);
                }
            }
            Stream content = new MemoryStream();
            ImageFormat imgFormat = ImageFormat.Bmp;
            if (file.Extension.ToLowerInvariant() == "png")
            {
                imgFormat = ImageFormat.Png;
            }
            else if (file.Extension.ToLowerInvariant() == "gif")
            {
                imgFormat = ImageFormat.Gif;
            }
            else if (file.Extension.ToLowerInvariant() == "jpg")
            {
                imgFormat = ImageFormat.Jpeg;
            }
            if (imageCropped != null)
            {
                imageCropped.Save(content, imgFormat);
                var newFile = fileManager.AddFile(userFolder, newFilename, content, true);
                cropresult.url = newFile.ToUrl();
                return cropresult;
            }
            return null;
        }



        public class CroppersDTO
        {
            public Dictionary<string, CroppperDTO> cropdata { get; set; }
            public Dictionary<string, ResizeDTO> croppers { get; set; }
            public string cropfolder { get; set; }
            public string url { get; set; }
        }

        public class CroppersResultDTO
        {
            public CroppersResultDTO()
            {
                cropdata = new Dictionary<string, CropResizeResultDTO>();
            }
            public Dictionary<string, CropResizeResultDTO> cropdata { get; set; }
            public string url { get; set; }
        }

        public class CropResizeDTO
        {
            public string id { get; set; }
            public string url { get; set; }
            public CropDTO crop { get; set; }
            public ResizeDTO resize { get; set; }
            public string cropfolder { get; set; }
        }
        public class CropResizeResultDTO
        {
            public string url { get; set; }
            public CropDTO crop { get; set; }
        }

        public class CroppperDTO
        {
            public string url { get; set; }
            public CropDTO cropper { get; set; }
        }
        public class CropDTO
        {
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int rotate { get; set; }
        }
        public class ResizeDTO
        {
            public int width { get; set; }
            public int height { get; set; }
        }
    }
}

