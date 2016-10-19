#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using Newtonsoft.Json;
using DotNetNuke.Entities.Icons;
using System.Text;
using System.Globalization;
using Satrabel.OpenContent.Components.Dnn;

namespace Satrabel.OpenContent.Components
{
    public class FileUploadController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));
        private readonly IFileManager _fileManager = FileManager.Instance;
        private readonly IFolderManager _folderManager = FolderManager.Instance;

        [DnnAuthorize]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile()
        {
            var statuses = new List<FilesStatus>();
            try
            {
                //todo can we eliminate the HttpContext here
                UploadWholeFile(HttpContextSource.Current, statuses);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return IframeSafeJson(statuses);
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

                var fileName = CleanUpFileName(Path.GetFileName(file.FileName));


                if (IsAllowedExtension(fileName))
                {
                    string uploadfolder = "OpenContent/Files/" + ActiveModule.ModuleID;

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
                    while (fileInfo != null)
                    {
                        suffix++;
                        fileName = baseFileName + "-" + suffix + extension;
                        fileInfo = _fileManager.GetFile(userFolder, fileName);
                    }
                    fileInfo = _fileManager.AddFile(userFolder, fileName, file.InputStream, true);
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
                        url = fileInfo.ToUrl(),
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
            foreach (char character in " []|:;`%&$+,/=?@~#<>()¿¡«»!'’–*…^£".ToCharArray())
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