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
        public static JToken ConvertTo(MigrationStatusReport report, JToken sourceData, OcFieldInfo sourceField, OcFieldInfo targetField, MigrationConfig config, int moduleId)
        {
            if (sourceField.Type == targetField.Type)
                return MigratorHelper.SameType(report, sourceData);

            var migratorType = $"{sourceField.Type} => {targetField.Type}";
            switch (migratorType)
            {
                case "file2 => imagex":
                    return MigratorHelper.File2ToImageX(report, sourceData, sourceField, targetField, config, moduleId);
                case "image2 => imagex":
                    return MigratorHelper.Image2ToImageX(report, sourceData, sourceField, targetField, config, moduleId);
                case "text => textarea":
                    return MigratorHelper.TextToTextArea(report, sourceData, sourceField, targetField);
                default:
                    throw new NotImplementedException($"Migration from field type {sourceField.Type} to {targetField.Type} is not supported. Consider implementing it yourself.");
            }
        }

        private static JToken File2ToImageX(MigrationStatusReport report, JToken input, OcFieldInfo sourceField, OcFieldInfo targetField, MigrationConfig config, int moduleId)
        {
            var fileManager = FileManager.Instance;
            int fileId = int.Parse(input.ToString());
            var file = fileManager.GetFile(fileId);

            if (file == null)
            {
                report.LogError($"Error in File2ToImageX: Source file not found: id='{fileId}', module='{moduleId}'");
                return null;
            }

            // check if extention is allowed in target field
            if (!targetField.Options["fileExtensions"].Value<string>().ToLowerInvariant().Contains(file.Extension.ToLowerInvariant()))
            {
                report.Skipped($"Item with FileExtention {file.Extension} skipped. Reason: not allowed in target field.");
                return new JObject();
            }

            file = CopyFileIfNeeded(fileId, report, sourceField, targetField, config, moduleId);

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

        private static JToken Image2ToImageX(MigrationStatusReport report, JToken input, OcFieldInfo sourceField, OcFieldInfo targetField, MigrationConfig config, int moduleId)
        {
            var fileManager = FileManager.Instance;
            int fileId = int.Parse(input.ToString());
            var file = CopyFileIfNeeded(fileId, report, sourceField, targetField, config, moduleId);

            if (file == null)
            {
                report.LogError($"Error in Image2ToImageX: Source file not found: id='{fileId}', module='{moduleId}'");
                return null;
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

        private static JToken SameType(MigrationStatusReport report, JToken input)
        {
            report.Migrated();
            return input;
        }

        private static IFileInfo CopyFileIfNeeded(int fileId, MigrationStatusReport report, OcFieldInfo sourceField, OcFieldInfo targetField, MigrationConfig config, int moduleId)
        {
            var fileManager = FileManager.Instance;
            var file = fileManager.GetFile(fileId);

            if (file == null)
                return null;

            // Copy file to default upload folder, if not already there.
            if (sourceField.Options["folder"] != targetField.Options["uploadfolder"] && !config.DryRun)
            {
                var folderManager = FolderManager.Instance;

                var secure = targetField.Options["secure"] != null && targetField.Options["secure"].Value<bool>();
                var uploadParentFolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/";
                var parentFolder = folderManager.GetFolder(config.PortalId, uploadParentFolder);
                if (parentFolder == null)
                {
                    if (secure)
                    {
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(config.PortalId, "Secure");
                        folderManager.AddFolder(folderMapping, uploadParentFolder);
                    }
                    else
                    {
                        folderManager.AddFolder(config.PortalId, uploadParentFolder);
                    }
                }
                var uploadfolder = "OpenContent/" + (secure ? "Secure" : "") + "Files/" + moduleId + "/";
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
                var targetFolder = folderManager.GetFolder(config.PortalId, uploadfolder);
                if (targetFolder == null)
                {
                    if (secure)
                    {
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(config.PortalId, "Secure");
                        targetFolder = folderManager.AddFolder(folderMapping, uploadfolder);
                    }
                    else
                    {
                        targetFolder = folderManager.AddFolder(config.PortalId, uploadfolder);
                    }
                }

                if (fileManager.FileExists(targetFolder, file.FileName))
                {
                    report.FoundExistingFile();
                    file = fileManager.GetFile(targetFolder, file.FileName);
                }
                else
                    file = fileManager.CopyFile(file, targetFolder);
            }

            return file;
        }
    }
}