using System;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Migration
{
    public static class MigratorHelper
    {
        /// <summary>
        /// List of supported migrations
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static JToken ConvertTo(MigrationStatusReport report, JToken sourceData, OcFieldInfo sourceField, OcFieldInfo targetField, int portalId, int moduleID, bool dryRun)
        {
            var migratorType = $"{sourceField.Type} => {targetField.Type}";

            switch (migratorType)
            {
                case "file2 => imagex":
                    return MigratorHelper.File2ToImageX(report, sourceData, sourceField, targetField, portalId, moduleID, dryRun);
                case "image2 => imagex":
                    return MigratorHelper.Image2ToImageX(report, sourceData, sourceField, targetField, portalId, moduleID, dryRun);
                case "text => textarea":
                    return MigratorHelper.TextToTextArea(report, sourceData, sourceField, targetField);
                default:
                    throw new NotImplementedException($"Migration from field type {sourceField.Type} to {targetField.Type} is not supported. Consider implementing it yourself.");
            }
        }

        private static JToken File2ToImageX(MigrationStatusReport report, JToken input, OcFieldInfo sourceField, OcFieldInfo targetField, int portalId, int moduleId, bool dryRun)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;
            int fileId = int.Parse(input.ToString());
            var file = fileManager.GetFile(fileId);

            // check if extention is allowed in target field
            if (!targetField.Options["fileExtensions"].Value<string>().ToLowerInvariant().Contains(file.Extension.ToLowerInvariant()))
            {
                report.Skipped($"Item with FileExtention {file.Extension} skipped. Reason: not allowed in target field.");
                return new JObject();
            } 

            // Copy file to default upload folder, if not already there.
            if (sourceField.Options["folder"] != targetField.Options["uploadfolder"] && !dryRun)
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

            report.Migrated();
            return output;
        }

        private static JToken Image2ToImageX(MigrationStatusReport report, JToken input, OcFieldInfo sourceField, OcFieldInfo targetField, int portalId, int moduleId, bool dryRun)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;
            int fileId = int.Parse(input.ToString());
            var file = fileManager.GetFile(fileId);

            // Copy file to default upload folder, if not already there.
            if (sourceField.Options["folder"] != targetField.Options["uploadfolder"] && !dryRun)
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

            report.Migrated();
            return output;
        }

        private static JToken TextToTextArea(MigrationStatusReport report, JToken input, OcFieldInfo sourceField, OcFieldInfo targetField)
        {
            // create TextArea JToken from the original data
            JToken output = new JValue(input.ToString());
            
            report.Migrated();
            return output;
        }
    }
}