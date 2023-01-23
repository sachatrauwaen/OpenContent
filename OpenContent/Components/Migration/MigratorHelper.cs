using System;
using System.IO;
using System.Web.Services.Description;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Migration
{
    public static class MigratorHelper
    {
        public static JToken Image2ToImageX(JToken input, OcFieldInfo sourceField, OcFieldInfo targetField, int portalId, int moduleId)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;
            int fileId = int.Parse(input.ToString());
            var file = fileManager.GetFile(fileId);

            if (sourceField.Options["folder"] != targetField.Options["uploadfolder"])
            {
                bool secure = targetField.Options["secure"] != null && targetField.Options["secure"].Value<Boolean>();
                string uploadParentFolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/";
                var parentFolder = folderManager.GetFolder(portalId, uploadParentFolder);
                if (parentFolder == null)
                {
                    if (secure)
                    {
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, "Secure");
                        folderManager.AddFolder(folderMapping, uploadParentFolder);
                    }
                    else
                    {
                        folderManager.AddFolder(portalId, uploadParentFolder);
                    }
                }
                string uploadfolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/" + moduleId + "/";
                //if (module.Settings.Manifest.DeleteFiles)
                //{
                //    if (!string.IsNullOrEmpty(context.Request.Form["itemKey"]))
                //    {
                //        uploadfolder += context.Request.Form["itemKey"];
                //    }
                //}
                if (targetField.Options["uploadfolder"] != null && 
                    !string.IsNullOrEmpty(targetField.Options["uploadfolder"].ToString()))
                {
                    uploadfolder = targetField.Options["uploadfolder"].ToString();
                }
                var targetFolder = folderManager.GetFolder(portalId, uploadfolder);
                if (targetFolder == null)
                {
                    if (secure)
                    {
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, "Secure");
                        targetFolder = folderManager.AddFolder(folderMapping, uploadfolder);
                    }
                    else
                    {
                        targetFolder = folderManager.AddFolder(portalId, uploadfolder);
                    }
                }
                file = fileManager.CopyFile(file, targetFolder);
            }

            // create ImageX JToken, but also copy the original image to the new image location
            JToken output = new JObject();
            output["id"] = file.FileId.ToString();
            output["url"] = fileManager.GetUrl(file);
            output["filename"] = file.FileName;
            output["width"] = file.Width;
            output["height"] = file.Height;
            //output["cropUrl"] = "";
            //output["crop"] = new JObject();

            return output;
        }

        public static JToken TextToTextArea(JToken input, OcFieldInfo sourceField, OcFieldInfo targetField)
        {
            // create ImageX JToken, but also copy the original image to the new image location
            JToken output = new JValue(input.ToString());
            
            return output;
        }
    }
}