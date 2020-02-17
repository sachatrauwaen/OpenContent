using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Form;
using Satrabel.OpenContent.Components.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class FormAPIController : DnnApiController
    {
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Form(string key)
        {
            //string template = (string)ActiveModule.ModuleSettings["template"];

            JObject json = new JObject();
            try
            {
                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                if (settings.TemplateAvailable)
                {
                    var formBuilder = new FormBuilder(settings.TemplateDir);
                    json = formBuilder.BuildForm(key);

                    if (UserInfo.UserID > 0 && json["schema"] is JObject)
                    {
                        json["schema"] = FormUtils.InitFields(json["schema"] as JObject, UserInfo);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, json);
            }
            catch (Exception exc)
            {
                LoggingUtils.ProcessApiLoadException(this, exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Submit()
        {
            SubmitDTO req = JsonConvert.DeserializeObject<SubmitDTO>(HttpContextSource.Current.Request.Form["data"].ToString());
            var form = req.form;
            var statuses = new List<FilesStatus>();
            try
            {
                //todo can we eliminate the HttpContext here
                UploadWholeFile(HttpContextSource.Current, statuses);
                var files = new JArray();
                form["Files"] = files;
                int i = 1;
                foreach (var item in statuses)
                {
                    var file = new JObject();
                    file["id"] = item.id;
                    file["name"] = item.name;
                    file["url"] = FormUtils.ToAbsoluteUrl(item.url);
                    files.Add(file);
                    //form["File"+i] = OpenFormUtils.ToAbsoluteUrl(item.url);                    
                    i++;
                }
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
            }
            try
            {
                var data = new JObject();
                data["form"] = req.form;
                string jsonSettings = ActiveModule.ModuleSettings["formsettings"] as string;
                if (!string.IsNullOrEmpty(jsonSettings))
                {
                    data["formSettings"] = JObject.Parse(jsonSettings);
                }
                var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                var dsItem = ds.Get(dsContext, req.id);
                var res = ds.Action(dsContext, string.IsNullOrEmpty(req.action) ? "FormSubmit" : req.action, dsItem, data);
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private void UploadWholeFile(HttpContextBase context, ICollection<FilesStatus> statuses)
        {
            IFileManager _fileManager = FileManager.Instance;
            IFolderManager _folderManager = FolderManager.Instance;
            for (var i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                if (file == null) continue;

                var fileName = FileUploadController.CleanUpFileName(Path.GetFileName(file.FileName));


                if (IsAllowedExtension(fileName))
                {
                    string uploadfolder = "OpenContent/FormFiles/" + ActiveModule.ModuleID;

                    if (!string.IsNullOrEmpty(context.Request.Form["uploadfolder"]))
                    {
                        uploadfolder = context.Request.Form["uploadfolder"];
                    }
                    var userFolder = _folderManager.GetFolder(PortalSettings.PortalId, uploadfolder);
                    if (userFolder == null)
                    {
                        // Get folder mapping
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(PortalSettings.PortalId, "Secure");
                        userFolder = _folderManager.AddFolder(folderMapping, uploadfolder);
                        //userFolder = _folderManager.AddFolder(PortalSettings.PortalId, uploadfolder);
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
                        url = _fileManager.GetUrl(fileInfo),
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

    }

    public class SubmitDTO
    {
        public JObject form { get; set; }
        public string id { get; set; }
        public string action { get; set; }
    }
}