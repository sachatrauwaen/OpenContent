#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using Newtonsoft.Json;
using DotNetNuke.Entities.Icons;
using System.Text;
using System.Globalization;
using Satrabel.OpenContent.Components.Dnn;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using Satrabel.OpenContent.Components.Manifest;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System.Runtime.Remoting.Contexts;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Security.Policy;
using System.Security.Cryptography;

namespace Satrabel.OpenContent.Components
{
    //[SupportedModules("OpenContent")]
    public class FileUploadController : DnnApiController
    {
        private static readonly ILogAdapter Logger = App.Services.CreateLogger(typeof(FileUploadController));
        private readonly IFileManager _fileManager = FileManager.Instance;
        private readonly IFolderManager _folderManager = FolderManager.Instance;

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile()
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var statuses = new List<FilesStatus>();
            try
            {
                //todo can we eliminate the HttpContext here
                UploadWholeFile(HttpContextSource.Current, statuses);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                throw;
            }
            return IframeSafeJson(statuses);
        }


        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        //[IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFile()
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            try
            {
                var context = HttpContextSource.Current;
                string fileName = "";
                if (!string.IsNullOrEmpty(context.Request.Form["name"]))
                {
                    var name = context.Request.Form["name"];
                    if (name.IndexOf('?') > 0 && !name.StartsWith(@"/LinkClick.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.Substring(0, name.IndexOf('?'));
                        fileName = CleanUpFileName(Path.GetFileName(name));
                    }
                    else
                    {
                        fileName = name;
                    }
                }

                bool secure = false;
                if (!string.IsNullOrEmpty(context.Request.Form["secure"]))
                {
                    secure = context.Request.Form["secure"] == "true";
                }

                string uploadfolder;
                if (!string.IsNullOrEmpty(context.Request.Form["uploadfolder"])) // custom upload folder
                {
                    uploadfolder = context.Request.Form["uploadfolder"];
                    if (uploadfolder.Contains("[ITEMUSERFOLDER]"))
                    {
                        if (!string.IsNullOrEmpty(context.Request.Form["itemId"]))
                        {
                            int itemId;
                            if (int.TryParse(context.Request.Form["itemId"], out itemId))
                            {
                                var user = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, itemId);
                                if (user != null)
                                {
                                    var userFolder = _folderManager.GetUserFolder(user).FolderPath;
                                    uploadfolder = uploadfolder.Replace("[ITEMUSERFOLDER]", userFolder);
                                }
                            }
                        }

                    }
                    if (uploadfolder.Contains("[USERFOLDER]") && PortalSettings.UserId > -1)
                    {
                        var userFolder = _folderManager.GetUserFolder(PortalSettings.UserInfo).FolderPath;
                        uploadfolder = uploadfolder.Replace("[USERFOLDER]", userFolder);
                    }
                    //uploadfolder = uploadfolder.Replace("[MODULEID]", PortalSettings.UserId.ToString());                        
                }
                else
                {
                    string uploadParentFolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/";
                    uploadfolder = uploadParentFolder + module.DataModule.ModuleId + "/";
                    if (module.Settings.Manifest.DeleteFiles)
                    {
                        if (!string.IsNullOrEmpty(context.Request.Form["itemKey"]))
                        {
                            uploadfolder += context.Request.Form["itemKey"];
                        }
                    }
                }
                IFolderInfo uploadFolderInfo = _folderManager.GetFolder(PortalSettings.PortalId, uploadfolder);
                if (uploadFolderInfo != null && !string.IsNullOrEmpty(fileName))
                {
                    IFileInfo oldfileInfo = null;
                    if (fileName.StartsWith(@"/LinkClick.aspx", StringComparison.OrdinalIgnoreCase) && fileName.Contains("fileticket="))
                    {
                        string queryString = fileName.Substring(fileName.IndexOf('?') + 1);
                        NameValueCollection queryParameters = HttpUtility.ParseQueryString(queryString);
                        int fileId = FileLinkClickController.Instance.GetFileIdFromLinkClick(queryParameters);
                        if (fileId > 0)
                        {
                            oldfileInfo = _fileManager.GetFile(fileId);
                        }
                    }
                    else
                    {
                        oldfileInfo = _fileManager.GetFile(uploadFolderInfo, fileName);
                    }
                    if (oldfileInfo != null)
                    {
                        _fileManager.DeleteFile(oldfileInfo);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                throw;
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /*
        [DnnAuthorize]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile2()
        {
            var statuses = new List<FilesStatus>();
            try
            {
                //todo can we eliminate the HttpContext here
                UploadWholeFile2(HttpContextSource.Current, statuses);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            return IframeSafeJson(statuses);
        }
        */
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadEasyImage()
        {
            var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
            var manifest = module.Settings.Template.Manifest;
            if (!DnnPermissionsUtils.HasEditPermissions(module, manifest.GetEditRole(), manifest.GetEditRoleAllItems(), -1))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var statuses = new List<FilesStatus>();
            try
            {
                //todo can we eliminate the HttpContext here
                UploadWholeFile(HttpContextSource.Current, statuses);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                throw;
            }
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new ImageStatus
                {
                    Default = statuses[0].url
                }))
            };
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage EasyImageToken()
        {

            return Request.CreateResponse(HttpStatusCode.OK, "faketoken");
        }

        private HttpResponseMessage IframeSafeJson(List<FilesStatus> statuses)
        {
            //return json but label it as plain text
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(statuses))
            };
        }

        private static bool IsAllowedExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            //regex matches a dot followed by 1 or more chars followed by a semi-colon
            //regex is meant to block files like "foo.asp;.png" which can take advantage
            //of a vulnerability in IIS6 which treasts such files as .asp, not .png
            return !string.IsNullOrEmpty(extension)
                   && Host.AllowedExtensionWhitelist.IsAllowedExtension(extension)
                   && !Regex.IsMatch(fileName, @"\..+;");
        }

        // Upload entire file
        private void UploadWholeFile(HttpContextBase context, ICollection<FilesStatus> statuses)
        {
            for (var i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                if (file == null) continue;
                string fileName;
                if (!string.IsNullOrEmpty(context.Request.Form["name"]))
                {
                    var name = context.Request.Form["name"];
                    fileName = CleanUpFileName(Path.GetFileName(name));
                }
                else
                {
                    fileName = CleanUpFileName(Path.GetFileName(file.FileName));
                }

                if (IsAllowedExtension(fileName))
                {
                    bool? overwrite = null;
                    bool secure = false;
                    string old = context.Request.Form["old"];
                    if (old != null && old.IndexOf('?') > 0 & !old.StartsWith(@"/LinkClick.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        old = old.Substring(0, old.IndexOf('?'));
                    }
                    bool deleteOld = false;

                    var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                    if (!string.IsNullOrEmpty(context.Request.Form["secure"]))
                    {
                        secure = context.Request.Form["secure"] == "true";
                    }
                    if (!string.IsNullOrEmpty(context.Request.Form["overwrite"]))
                    {
                        overwrite = context.Request.Form["overwrite"] == "true";
                    }
                    if (!string.IsNullOrEmpty(context.Request.Form["deleteOld"]))
                    {
                        deleteOld = context.Request.Form["deleteOld"] == "true";
                    }

                    string uploadfolder;
                    if (!string.IsNullOrEmpty(context.Request.Form["hidden"]) && // cropped file
                        context.Request.Form["hidden"] == "true")
                    {
                        //uploadfolder = "OpenContent/" + (secure ? "Secure" : "") + "Cropped/" + ActiveModule.ModuleID;
                        uploadfolder = "OpenContent/" + (secure ? "Secure" : "") + "Cropped/" + module.DataModule.ModuleId;
                        GetOrCreateFolder(secure, uploadfolder);
                        if (!string.IsNullOrEmpty(context.Request.Form["itemKey"]))
                        {
                            uploadfolder += "/" + context.Request.Form["itemKey"];
                            GetOrCreateFolder(secure, uploadfolder);
                        }
                        if (!string.IsNullOrEmpty(context.Request.Form["cropfolder"]))
                        {
                            uploadfolder = context.Request.Form["cropfolder"];
                            GetOrCreateFolder(secure, uploadfolder);
                        }
                    }
                    else if (!string.IsNullOrEmpty(context.Request.Form["uploadfolder"])) // custom upload folder
                    {
                        uploadfolder = context.Request.Form["uploadfolder"];
                        uploadfolder = uploadfolder.TrimStart('/').TrimEnd('/');
                        if (uploadfolder.Contains("[ITEMUSERFOLDER]"))
                        {
                            if (!string.IsNullOrEmpty(context.Request.Form["itemId"]))
                            {
                                int itemId;
                                if (int.TryParse(context.Request.Form["itemId"], out itemId))
                                {
                                    var user = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, itemId);
                                    if (user != null)
                                    {
                                        var userFolder = _folderManager.GetUserFolder(user).FolderPath;
                                        uploadfolder = uploadfolder.Replace("[ITEMUSERFOLDER]", userFolder);
                                    }
                                }
                            }

                        }
                        if (uploadfolder.Contains("[USERFOLDER]") && PortalSettings.UserId > -1)
                        {
                            var userFolder = _folderManager.GetUserFolder(PortalSettings.UserInfo).FolderPath;
                            uploadfolder = uploadfolder.Replace("[USERFOLDER]", userFolder);
                        }
                        //uploadfolder = uploadfolder.Replace("[MODULEID]", PortalSettings.UserId.ToString());                        
                    }
                    else
                    {
                        string uploadParentFolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/";
                        GetOrCreateFolder(secure, uploadParentFolder);
                        uploadfolder = uploadParentFolder + module.DataModule.ModuleId + "/";
                        if (module.Settings.Manifest.DeleteFiles)
                        {
                            if (!string.IsNullOrEmpty(context.Request.Form["itemKey"]))
                            {
                                uploadfolder += context.Request.Form["itemKey"];
                            }
                            GetOrCreateFolder(secure, uploadfolder);
                        }
                    }
                    IFolderInfo uploadFolderInfo = GetOrCreateFolder(secure, uploadfolder);

                    if (deleteOld && !string.IsNullOrEmpty(old))
                    {
                        IFileInfo oldfileInfo = null;
                        if (old.StartsWith(@"/LinkClick.aspx", StringComparison.OrdinalIgnoreCase) && old.Contains("fileticket="))
                        {
                            string queryString = old.Substring(old.IndexOf('?') + 1);
                            NameValueCollection queryParameters = HttpUtility.ParseQueryString(queryString);
                            int fileId = FileLinkClickController.Instance.GetFileIdFromLinkClick(queryParameters);
                            if (fileId > 0)
                            {
                                oldfileInfo = _fileManager.GetFile(fileId);
                            }
                        }
                        else
                        {
                            oldfileInfo = _fileManager.GetFile(uploadFolderInfo, Path.GetFileName(old));
                        }
                        if (oldfileInfo != null)
                        {
                            _fileManager.DeleteFile(oldfileInfo);
                        }
                    }

                    int suffix = 0;
                    string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    var fileInfo = _fileManager.GetFile(uploadFolderInfo, fileName);
                    if (fileInfo != null && overwrite.HasValue && overwrite.Value)
                    {
                        //fileInfo = _fileManager.UpdateFile(fileInfo, file.InputStream);
                        fileInfo = _fileManager.AddFile(uploadFolderInfo, fileName, file.InputStream, true);
                    }
                    else if (fileInfo != null && overwrite.HasValue && !overwrite.Value)
                    {
                        statuses.Add(new FilesStatus
                        {
                            success = false,
                            name = fileName,
                            message = "File exist already."
                        });
                        return;
                    }
                    else
                    {
                        while (fileInfo != null)
                        {
                            suffix++;
                            fileName = baseFileName + "-" + suffix + extension;
                            fileInfo = _fileManager.GetFile(uploadFolderInfo, fileName);
                        }
                        fileInfo = _fileManager.AddFile(uploadFolderInfo, fileName, file.InputStream, true);
                    }

                    var fileIcon = IconController.IconURL("Ext" + fileInfo.Extension, "32x32");
                    if (!File.Exists(context.Server.MapPath(fileIcon)))
                    {
                        fileIcon = IconController.IconURL("File", "32x32");
                    }
                    statuses.Add(new FilesStatus
                    {
                        success = true,
                        name = fileName,
                        extension = fileInfo.Extension,
                        type = fileInfo.ContentType,
                        size = file.ContentLength,
                        progress = "1.0",
                        url = fileInfo.ToUrl().RemoveCachebuster(),
                        //thumbnail_url = fileIcon,
                        thumbnail_url = ImageHelper.GetImageUrl(fileInfo, new Ratio(40, 40)),  //todo for install in application folder is dat niet voldoende ???
                        message = "success",
                        id = fileInfo.FileId,
                    });
                }
                else
                {
                    statuses.Add(new FilesStatus
                    {
                        success = false,
                        name = fileName,
                        message = "File type not allowed."
                    });
                }
            }
        }

        private IFolderInfo GetOrCreateFolder(bool secure, string uploadfolder)
        {
            var userFolder = _folderManager.GetFolder(PortalSettings.PortalId, uploadfolder);
            if (userFolder == null)
            {
                var folders = uploadfolder.Split('/');
                string parentFolder="";
                foreach (var folder in folders)
                {
                    parentFolder += "/"+folder;
                    parentFolder = parentFolder.Trim('/');
                    var dnnFolder = _folderManager.GetFolder(PortalSettings.PortalId, parentFolder);
                    if (dnnFolder == null)
                    {
                        if (secure)
                        {
                            var folderMapping = FolderMappingController.Instance.GetFolderMapping(PortalSettings.PortalId, "Secure");
                            userFolder = _folderManager.AddFolder(folderMapping, parentFolder);
                        }
                        else
                        {
                            userFolder = _folderManager.AddFolder(PortalSettings.PortalId, parentFolder);
                        }
                    }
                }
            }

            return userFolder;
        }

        /*
        private void UploadWholeFile2(HttpContextBase context, ICollection<FilesStatus> statuses)
        {
        for (var i = 0; i < context.Request.Files.Count; i++)
        {
        var file = context.Request.Files[i];
        if (file == null) continue;
        string fileName;
        if (!string.IsNullOrEmpty(context.Request.Form["name"]))
        {
           var name = context.Request.Form["name"];
           fileName = CleanUpFileName(Path.GetFileName(name));
        }
        else
        {
           fileName = CleanUpFileName(Path.GetFileName(file.FileName));
        }

        if (IsAllowedExtension(fileName))
        {
           bool? overwrite = null;

           var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
           if (!string.IsNullOrEmpty(context.Request.Form["overwrite"]))
           {
               overwrite = context.Request.Form["overwrite"] == "true";
           }
           string uploadfolder = "OpenContent/Files/" + ActiveModule.ModuleID;
           if (module.Settings.Manifest.DeleteFiles)
           {
               if (!string.IsNullOrEmpty(context.Request.Form["itemKey"]))
               {
                   uploadfolder += "/" + context.Request.Form["itemKey"];
               }
           }
           if (!string.IsNullOrEmpty(context.Request.Form["uploadfolder"]))
           {
               uploadfolder = context.Request.Form["uploadfolder"];
           }
           var userFolder = _folderManager.GetFolder(PortalSettings.PortalId, uploadfolder);
           if (userFolder == null)
           {
               userFolder = _folderManager.AddFolder(PortalSettings.PortalId, uploadfolder);
           }
           int suffix = 0;
           string baseFileName = Path.GetFileNameWithoutExtension(fileName);
           string extension = Path.GetExtension(fileName);
           var fileInfo = _fileManager.GetFile(userFolder, fileName);
           if (fileInfo != null && overwrite.HasValue && overwrite.Value)
           {
               //fileInfo = _fileManager.UpdateFile(fileInfo, file.InputStream);
               fileInfo = _fileManager.AddFile(userFolder, fileName, file.InputStream, true);
           }
           else if (fileInfo != null && overwrite.HasValue && !overwrite.Value)
           {
               statuses.Add(new FilesStatus
               {
                   success = false,
                   name = fileName,
                   message = "File exist already."
               });
               return;
           }
           else
           {
               while (fileInfo != null)
               {
                   suffix++;
                   fileName = baseFileName + "-" + suffix + extension;
                   fileInfo = _fileManager.GetFile(userFolder, fileName);
               }
               fileInfo = _fileManager.AddFile(userFolder, fileName, file.InputStream, true);
           }

           var fileIcon = IconController.IconURL("Ext" + fileInfo.Extension, "32x32");
           if (!File.Exists(context.Server.MapPath(fileIcon)))
           {
               fileIcon = IconController.IconURL("File", "32x32");
           }
           if (int.TryParse(context.Request.Form["width"], out int width) && width > 0 &&
               int.TryParse(context.Request.Form["height"], out int height) && height > 0)
           {

               var image = Image.FromFile(fileInfo.PhysicalPath);
               Image imageCropped;

               int cropleft = 0;
               int croptop = 0;
               int cropwidth = 0;
               int cropheight = 0;
               imageCropped = TemplateHelpers.ImageHelper.SaveCroppedImage(image, width, height, out cropleft, out croptop, out cropwidth, out cropheight);
               Stream content = new MemoryStream();
               ImageFormat imgFormat = ImageFormat.Bmp;
               if (fileInfo.Extension.ToLowerInvariant() == "png")
               {
                   imgFormat = ImageFormat.Png;
               }
               else if (fileInfo.Extension.ToLowerInvariant() == "gif")
               {
                   imgFormat = ImageFormat.Gif;
               }
               else if (fileInfo.Extension.ToLowerInvariant() == "jpg")
               {
                   imgFormat = ImageFormat.Jpeg;
               }
               imageCropped.Save(content, imgFormat);

               uploadfolder = "OpenContent/Cropped/" + ActiveModule.ModuleID;
               if (!string.IsNullOrEmpty(context.Request.Form["itemKey"]))
               {
                   uploadfolder += "/" + context.Request.Form["itemKey"];
               }
               if (!string.IsNullOrEmpty(context.Request.Form["cropfolder"]))
               {
                   uploadfolder = context.Request.Form["cropfolder"];
               }
               userFolder = _folderManager.GetFolder(PortalSettings.PortalId, uploadfolder);
               if (userFolder == null)
               {
                   userFolder = _folderManager.AddFolder(PortalSettings.PortalId, uploadfolder);
               }
               fileInfo = _fileManager.GetFile(userFolder, fileName);
               if (fileInfo != null )
               {
                   fileInfo = _fileManager.AddFile(userFolder, fileName, content, true);
               }
           }

           statuses.Add(new FilesStatus
           {
               success = true,
               name = fileName,
               extension = fileInfo.Extension,
               type = fileInfo.ContentType,
               size = file.ContentLength,
               progress = "1.0",
               url = fileInfo.ToUrl().RemoveCachebuster(),
               thumbnail_url = fileIcon,
               message = "success",
               id = fileInfo.FileId,
           });
        }
        else
        {
           statuses.Add(new FilesStatus
           {
               success = false,
               name = fileName,
               message = "File type not allowed."
           });
        }
        }
        }
        */
        public static string CleanUpFileName(string filename)
        {
            var newName = HttpUtility.UrlDecode(filename);
            newName = RemoveDiacritics(newName);
            newName = returnSafeString(newName);
            return newName;
        }

        public static string returnSafeString(string s)
        {
            foreach (char character in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(character.ToString(), "-");
            }

            foreach (char character in Path.GetInvalidPathChars())
            {
                s = s.Replace(character.ToString(), "-");
            }
            foreach (char character in " []|:;`%&$+,/=?@~#<>()����!'��*�^�".ToCharArray())
            {
                s = s.Replace(character.ToString(), "_");
            }
            return (s);
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

    }
}