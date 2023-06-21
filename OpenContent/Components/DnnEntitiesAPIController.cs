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
using System.IO;
using DotNetNuke.Security;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Common;
using DotNetNuke.Services.FileSystem;
using System.Drawing;
using System.Drawing.Imaging;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System.Text.RegularExpressions;
using Satrabel.OpenContent.Components.Dnn;
using DotNetNuke.Entities.Users;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent.Components
{

    public class DnnEntitiesAPIController : DnnApiController
    {
        private static readonly ILogAdapter Logger = App.Services.CreateLogger(typeof(DnnEntitiesAPIController));

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpGet]
        public HttpResponseMessage Tabs(string q, string l)
        {
            try
            {
                var tabs = TabController.GetTabsBySortOrder(PortalSettings.PortalId).Where(t => t.ParentId != PortalSettings.AdminTabId)
                    .Where(t => t.TabName.ToLower().Contains(q.ToLower()))
                    .Select(t => new
                    {
                        name = t.TabName + " (" + t.TabPath.Replace("//", "/").Replace("/" + t.TabName + "/", "") + " " + l + ")",
                        value = (new System.Uri(NavigateUrl(t, l, PortalSettings))).PathAndQuery
                    });
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
                files = files.Where(f => f.IsImageFile());
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
                if (portalFolder == null)
                {
                    var exc = new ArgumentException("Folder path not found. Adjust ['folder': " + d + "] in optionfile. ");
                    Logger.Error(exc);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
                }
                var files = folderManager.GetFiles(portalFolder, true);
                files = files.Where(f => f.IsImageFile());
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                int folderLength = d == null ? 0 : d.Length;

                var res = files.Select(f => new
                {
                    value = f.FileId.ToString(),
                    url = ImageHelper.GetImageUrl(f, new Ratio(40, 40)),  //todo for install in application folder is dat niet voldoende ???
                    text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName
                }).Take(1000);

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage ImagesLookupExt(string q, string folder, string itemKey = "")
        {
            return ImagesLookupSecure(q, folder, false, itemKey);
        }

        /// <summary>
        /// Imageses the lookup.
        /// </summary>
        /// <param name="q">The string that should be Contained in the name of the file (case insensitive). Use * to get all the files.</param>
        /// <param name="folder">The Folder path to retrieve</param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage ImagesLookupSecure(string q, string folder, bool secure, string itemKey = "", string itemId = "")
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            try
            {
                //var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                var folderManager = FolderManager.Instance;
                string imageFolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/" + module.DataModule.ModuleId;
                if (module.Settings.Manifest.DeleteFiles)
                {
                    if (!string.IsNullOrEmpty(itemKey))
                    {
                        imageFolder += "/" + itemKey;
                    }
                }
                if (!string.IsNullOrEmpty(folder))
                {
                    imageFolder = folder;
                    if (folder.Contains("[ITEMUSERFOLDER]"))
                    {
                        if (!string.IsNullOrEmpty(itemId))
                        {
                            int userId;
                            if (int.TryParse(itemId, out userId))
                            {
                                var user = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, userId);
                                if (user != null)
                                {
                                    var userFolder = folderManager.GetUserFolder(user).FolderPath;
                                    imageFolder = imageFolder.Replace("[ITEMUSERFOLDER]", userFolder);
                                }
                            }
                        }
                    }
                    if (folder.Contains("[USERFOLDER]"))
                    {
                        if (PortalSettings.UserId > -1)
                        {
                            var userFolder = folderManager.GetUserFolder(PortalSettings.UserInfo).FolderPath;
                            imageFolder = imageFolder.Replace("[USERFOLDER]", userFolder);
                        }
                    }
                }
                var dnnFolder = folderManager.GetFolder(PortalSettings.PortalId, imageFolder);
                if (dnnFolder == null)
                {
                    //dnnFolder = folderManager.AddFolder(PortalSettings.PortalId, imageFolder);
                    return Request.CreateResponse(HttpStatusCode.OK, new string[0]);
                }

                var files = folderManager.GetFiles(dnnFolder, true);
                files = files.Where(f => f.IsImageFile());
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                int folderLength = imageFolder.Length;
                var res = files.Select(f => new
                {
                    id = f.FileId.ToString(),
                    thumbUrl = ImageHelper.GetImageUrl(f, new Ratio(40, 40)),  //todo for install in application folder is dat niet voldoende ???
                    url = FileManager.Instance.GetUrl(f).RemoveCachebuster(),
                    text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName,
                    filename = f.FileName,
                    width = f.Width,
                    height = f.Height
                }).Take(1000);

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }


        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage FilesLookup(string q, string d, string filter = "")
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                if (portalFolder == null)
                {
                    // next three lines are new, but commented out because we need to decide if we realy want to do this as this input is not cleaned
                    //if (d != null)
                    //    portalFolder = FolderManager.Instance.AddFolder(PortalSettings.PortalId, d);
                    //else
                    throw new Exception($"folder {d ?? ""} does not exist");
                }
                var files = folderManager.GetFiles(portalFolder, true);
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    var rx = new Regex(filter, RegexOptions.IgnoreCase);
                    files = files.Where(f => rx.IsMatch(f.FileName));
                }
                int folderLength = d?.Length ?? 0;
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage FilesLookupSecure(string q, string folder, bool secure, string filter = "")
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            try
            {
                //var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                var folderManager = FolderManager.Instance;
                string filesFolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/" + module.DataModule.ModuleId;
                //if (module.Settings.Manifest.DeleteFiles)
                //{
                //    if (!string.IsNullOrEmpty(itemKey))
                //    {
                //        filesFolder += "/" + itemKey;
                //    }
                //}
                if (!string.IsNullOrEmpty(folder))
                {
                    filesFolder = folder;
                }


                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, filesFolder);
                if (portalFolder == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new string[0]);
                }
                var files = folderManager.GetFiles(portalFolder, true);
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    var rx = new Regex(filter, RegexOptions.IgnoreCase);
                    files = files.Where(f => rx.IsMatch(f.FileName));
                }
                int folderLength = folder?.Length ?? 0;
                var res = files.Select(f => new
                {
                    value = f.FileId.ToString(),
                    url = fileManager.GetUrl(f),
                    filename = f.FileName,
                    text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName /*+ (string.IsNullOrEmpty(f.Folder) ? "" : " (" + f.Folder.Trim('/') + ")")*/
                });
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage FilesLookup(string q, string d, string filter, int pageIndex, int pageSize)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                var files = folderManager.GetFiles(portalFolder, true);
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    files = files.Where(f => f.FileName.ToLower().Contains(q.ToLower()));
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    var rx = new Regex(filter, RegexOptions.IgnoreCase);
                    files = files.Where(f => rx.IsMatch(f.FileName));
                }
                int total = files.Count();
                if (pageIndex > 0 && pageSize > 0)
                {
                    files = files.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                }
                int folderLength = (d == null) ? 0 : d.Length;
                var res = files.Select(f => new { id = f.FileId.ToString(), url = fileManager.GetUrl(f), text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName /*+ (string.IsNullOrEmpty(f.Folder) ? "" : " (" + f.Folder.Trim('/') + ")")*/ });
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    items = res,
                    total = total,
                    pageIndex,
                    pageSize
                });
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
        public HttpResponseMessage FoldersLookup(string q, string d, string filter = "")
        {
            try
            {
                IEnumerable<IFolderInfo> folders = new List<IFolderInfo>();
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                var portalFolder = folderManager.GetFolder(PortalSettings.PortalId, d ?? "");
                if (portalFolder != null)
                {
                    folders = GetFolders(folderManager, portalFolder);

                    if (q != "*" && !string.IsNullOrEmpty(q))
                    {
                        folders = folders.Where(f => f.FolderName.ToLower().Contains(q.ToLower()));
                    }
                    if (!string.IsNullOrEmpty(filter))
                    {
                        var rx = new Regex(filter, RegexOptions.IgnoreCase);
                        folders = folders.Where(f => rx.IsMatch(f.FolderName));
                    }
                }
                int folderLength = (d == null) ? 0 : d.Length;

                var res = folders.Select(f => new { value = f.FolderID.ToString(), url = f.FolderPath, text = f.FolderPath.Substring(folderLength).Trim('/') });

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static IEnumerable<IFolderInfo> GetFolders(IFolderManager folderManager, IFolderInfo portalFolder)
        {
            var folders = new List<IFolderInfo>();
            foreach (var item in folderManager.GetFolders(portalFolder))
            {
                folders.Add(item);
                folders.AddRange(GetFolders(folderManager, item));
            }
            return folders;
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
                var tabsDtos = tabs.Select(t => new
                {
                    value = t.TabID,
                    text = t.TabName + " (" + t.TabPath.Replace("//", "/").Replace("/" + t.TabName + "/", "") + " " + l + ")",
                    url = (new System.Uri(NavigateUrl(t, l, PortalSettings))).PathAndQuery
                });
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
        public HttpResponseMessage RoleLookup(string q)
        {
            try
            {
                var roles = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(PortalSettings.PortalId).AsQueryable();
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    roles = roles.Where(t => t.RoleName.ToLower().Contains(q.ToLower()));
                }
                var rolesDtos = roles.Select(t => new { value = t.RoleID.ToString(), text = t.RoleName }).ToList();
                rolesDtos.Add(new { value = "Unauthenticated", text = "Unauthenticated" });
                rolesDtos.Add(new { value = "AllUsers", text = "All Users" });

                return Request.CreateResponse(HttpStatusCode.OK, rolesDtos);
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
        public HttpResponseMessage UserRoleLookup(string q)
        {
            try
            {
                var roles = DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(PortalSettings.PortalId).AsQueryable();

                roles = roles.Where(r => r.RoleName != "Administrators" 
                                    && r.RoleName != "Registered Users" 
                                    && r.RoleName != "Unverified Users");
                if (q != "*" && !string.IsNullOrEmpty(q))
                {
                    roles = roles.Where(t => t.RoleName.ToLower().Contains(q.ToLower()));
                }
                var rolesDtos = roles.Select(t => new { value = t.RoleID.ToString(), text = t.RoleName }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, rolesDtos);
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
        public HttpResponseMessage FileInfo(int fileid, string folder = "")
        {
            try
            {
                var fileManager = FileManager.Instance;
                IFileInfo f = fileManager.GetFile(fileid);
                int folderLength = folder == null ? 0 : folder.Length;
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    id = f.FileId.ToString(),
                    url = fileManager.GetUrl(f),
                    text = f.Folder.Substring(folderLength).TrimStart('/') + f.FileName

                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpGet]
        public HttpResponseMessage Files(string q, string d)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
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

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage CropImage(CropResizeDTO cropData)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            //var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
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
                string rawImageUrl = cropData.url;
                if (rawImageUrl.IndexOf('?') > 0)
                {
                    rawImageUrl = rawImageUrl.Substring(0, rawImageUrl.IndexOf('?'));
                }
                rawImageUrl = rawImageUrl.Replace(PortalSettings.HomeDirectory, "");
                var file = fileManager.GetFile(ActiveModule.PortalID, rawImageUrl);
                string cropfolder = "OpenContent/Cropped/" + module.DataModule.ModuleId;
                if (!string.IsNullOrEmpty(cropData.itemKey))
                {
                    cropfolder += "/" + cropData.itemKey;
                }
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
                    //var folder = folderManager.GetFolder(file.FolderId);
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
                    else if (file.Extension.ToLowerInvariant() == "jpg" || file.Extension.ToLowerInvariant() == "jpeg")
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
                    res.url = fs.url.RemoveCachebuster();
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage CropImages(CroppersDTO cropData)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            //var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
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
                    string cropfolder = "OpenContent/Files/" + module.DataModule.ModuleId;
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
            else if (file.Extension.ToLowerInvariant() == "jpg" || file.Extension.ToLowerInvariant() == "jpeg")
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


        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [HttpPost]
        public HttpResponseMessage DownloadFile(FileDTO req)
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            try
            {
                var folderManager = FolderManager.Instance;
                var fileManager = FileManager.Instance;
                string RawImageUrl = req.url;
                if (RawImageUrl.IndexOf('?') > 0)
                {
                    RawImageUrl = RawImageUrl.Substring(0, RawImageUrl.IndexOf('?'));
                }
                RawImageUrl = RawImageUrl.Replace(PortalSettings.HomeDirectory, "");
                var file = fileManager.GetFile(ActiveModule.PortalID, RawImageUrl);
                string uploadfolder = "OpenContent/Files/" + module.DataModule.ModuleId;
                if (!string.IsNullOrEmpty(req.uploadfolder))
                {
                    uploadfolder = req.uploadfolder;
                }
                var userFolder = folderManager.GetFolder(PortalSettings.PortalId, uploadfolder);
                if (userFolder == null)
                {
                    userFolder = folderManager.AddFolder(PortalSettings.PortalId, uploadfolder);
                }
                string fileName = FileUploadController.CleanUpFileName(Path.GetFileName(req.url));
                if (file == null && (req.url.StartsWith("http://") || req.url.StartsWith("https://")))
                {
                    int suffix = 0;
                    string baseFileName = Path.GetFileNameWithoutExtension(req.url);
                    string extension = Path.GetExtension(req.url);
                    var fileInfo = fileManager.GetFile(userFolder, fileName);
                    while (fileInfo != null)
                    {
                        suffix++;
                        fileName = baseFileName + "-" + suffix + extension;
                        fileInfo = fileManager.GetFile(userFolder, fileName);
                    }
                    using (WebClient myWebClient = new WebClient())
                    {
                        try
                        {
                            var stream = new MemoryStream(myWebClient.DownloadData(req.url));
                            file = fileManager.AddFile(userFolder, fileName, stream, true);
                        }
                        catch (Exception ex)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new
                            {
                                url = "",
                                id = -1,
                                error = ex.Message
                            });
                        }

                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    url = file.ToUrl(),
                    id = file.FileId,
                    error = ""
                });
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
        public HttpResponseMessage UsersLookup(string q, string role)
        {
            try
            {
                q += "%";
                int totalRecords = 0;
                var users = UserController.GetUsersByDisplayName(PortalSettings.PortalId, q, 0, 1000, ref totalRecords, false, false).Cast<UserInfo>();
                if (role != null)
                {
                    users = users.Where(u => u.Roles.Any(r => role.Contains(r)));
                }
                users = users.Where(u => u.IsSuperUser == false); // exclude the superUsers
                var res = users.Select(u => new { value = u.UserID.ToString(), text = u.DisplayName });
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
        public HttpResponseMessage UsersLookup(string q, string[] roles)
        {
            try
            {
                q += "%";
                int totalRecords = 0;
                var users = UserController.GetUsersByDisplayName(PortalSettings.PortalId, q, 0, 1000, ref totalRecords, false, false).Cast<UserInfo>();
                if (roles != null)
                {
                    users = users.Where(u => u.Roles.Any(r => roles.Contains(r)));
                }
                users = users.Where(u => u.IsSuperUser == false); // exclude the superUsers
                var res = users.Select(u => new { value = u.UserID.ToString(), text = u.DisplayName });
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
        public HttpResponseMessage UsersLookup(string q, string role, int pageIndex, int pageSize)
        {
            try
            {
                q += "%";
                int totalRecords = 0;
                IEnumerable<UserInfo> users;
                if (string.IsNullOrEmpty(role))
                {
                    users = UserController.GetUsersByDisplayName(PortalSettings.PortalId, q, pageIndex - 1, pageSize, ref totalRecords, false, false).Cast<UserInfo>();
                }
                else
                {
                    users = UserController.GetUsersByDisplayName(PortalSettings.PortalId, q, 0, 100000, ref totalRecords, false, false).Cast<UserInfo>();
                    users = users.Where(u => u.Roles.Any(r => r == role));
                    if (pageIndex > 0 && pageSize > 0)
                    {
                        users = users.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    }
                }
                var res = users.Select(u => new { id = u.UserID.ToString(), text = u.DisplayName });
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    items = res,
                    total = totalRecords,
                    pageIndex,
                    pageSize
                });
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
        public HttpResponseMessage GetUserInfo(int userid)
        {
            try
            {
                var user = UserController.GetUserById(PortalSettings.PortalId, userid);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    id = user.UserID.ToString(),
                    text = user.DisplayName
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        public class FileDTO
        {
            public string uploadfolder { get; set; }
            public string url { get; set; }

            public string error { get; set; }
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
            public string itemKey { get; set; }
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

