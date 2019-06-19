using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Satrabel.OpenContent.Components.Export
{
    public class FullExport
    {
        private JObject Files;
        public FullExport(int tabId, int moduleId)
        {
            PortalSettings = PortalSettings.Current;
            PortalId = PortalSettings.PortalId;
            TabId = tabId;
            ModuleId = moduleId;
            ExportDirectory = HostingEnvironment.MapPath("~/" + PortalSettings.HomeDirectory + "OpenContent/Export/");
            ModuleExportDirectory = ExportDirectory + ModuleId.ToString() + "\\";
            ImportDirectory = HostingEnvironment.MapPath("~/" + PortalSettings.HomeDirectory + "OpenContent/Import/");
            ModuleImportDirectory = ImportDirectory + ModuleId.ToString() + "\\";
        }
        public int PortalId { get; private set; }
        public int TabId { get; private set; }
        public int ModuleId { get; private set; }

        public PortalSettings PortalSettings { get; private set; }
        private string ExportDirectory { get; set; }
        private string ModuleExportDirectory { get; set; }

        private string ImportDirectory { get; set; }
        private string ModuleImportDirectory { get; set; }


        public string Export()
        {
            if (Directory.Exists(ModuleExportDirectory))
                Directory.Delete(ModuleExportDirectory, true);

            Directory.CreateDirectory(ModuleExportDirectory);

            Files = new JObject();
            var module = OpenContentModuleConfig.Create(ModuleId, TabId, PortalSettings);
            IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(module);
            var alpaca = ds.GetAlpaca(dsContext, true, true, false);
            if (module.IsListMode())
            {
                var dsItems = ds.GetAll(dsContext, null);
                var arr = new JArray();
                foreach (var dsItem in dsItems.Items)
                {
                    var json = dsItem.Data;
                    ExportTraverse(alpaca, json);
                    arr.Add(json);
                }
                SaveData(arr);
            }
            else
            {
                dsContext.Single = true;
                var dsItem = ds.Get(dsContext, null);
                var json = dsItem.Data;
                ExportTraverse(alpaca, json);
                SaveData(json);
            }

            // additional data
            if (module.Settings.Manifest.AdditionalDataDefinition != null)
            {
                foreach (var item in module.Settings.Manifest.AdditionalDataDefinition)
                {
                    alpaca = ds.GetDataAlpaca(dsContext, true, true, false, item.Key);
                    var dsItem = ds.GetData(dsContext, item.Value.ScopeType, item.Key);
                    if (dsItem != null)
                    {
                        var json = dsItem.Data;
                        ExportTraverse(alpaca, json);
                        SaveData(json, item.Key);
                    }
                }
            }
            // save settings
            SaveSettings(module.Settings);
            // zip template
            string startPath = module.Settings.Template.Key.TemplateDir.PhysicalFullDirectory;
            string zipPath = ModuleExportDirectory + Path.GetFileName(module.Settings.Template.Key.Folder) + ".zip";
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(startPath, zipPath);

            // zip all
            string allStartPath = ModuleExportDirectory;
            string allZipPath = ExportDirectory + Path.GetFileName(module.Settings.Template.Key.Folder) + "-" + PortalId + "-" + TabId + "-" + ModuleId + "-" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + ".zip";
            if (File.Exists(allZipPath)) File.Delete(allZipPath);
            ZipFile.CreateFromDirectory(allStartPath, allZipPath);
            //Directory.Delete(ModuleExportDirectory, true);
            return allZipPath;
        }

        private void SaveSettings(OpenContentSettings settings)
        {
            var set = new JObject();
            set["Name"] = Path.GetFileName(settings.Template.Key.Folder);
            set["PortalId"] = PortalId;
            set["TabId"] = TabId;
            set["ModuleId"] = ModuleId;
            set["Template"] = settings.Template.Key.ToString();
            if (!string.IsNullOrEmpty(settings.Data))
            {
                set["Settings"] = JObject.Parse(settings.Data);
            }
            set["Query"] = settings.Query;
            set["Files"] = Files;
            File.WriteAllText(ModuleExportDirectory + "export.json", set.ToString());
        }

        //private string TokenizePath(string path)
        //{
        //    return path.Replace("Portals/" + PortalId + "/", "Portals/[PORTALID]/")
        //                .Replace("/OpenContent/Files/" + ModuleId + "/", "/OpenContent/Files/[MODULEID]/")
        //                .Replace("/OpenContent/Cropped/" + ModuleId + "/", "/OpenContent/Cropped/[MODULEID]/");
        //}

        private void SaveData(JToken json, string key = "data")
        {
            File.WriteAllText(ModuleExportDirectory + key + ".json", json.ToString());
        }
        private void ExportTraverse(JObject alpaca, JToken json)
        {
            JsonTraverse.Traverse(json.DeepClone(), alpaca["schema"] as JObject, alpaca["options"] as JObject, (data, schema, options) =>
            {
                var optionsType = options?.Value<string>("type");
                if (optionsType == "image" || optionsType == "file")
                {
                    SaveFile(data.ToString(), "");
                }
                else if (optionsType == "mlimage" || optionsType == "mlfile")
                {
                    if (json is JObject)
                    {
                        foreach (var item in (data as JObject).Children<JProperty>())
                        {
                            SaveFile(item.Value.ToString(), "");
                        }
                    }
                }
                else if (optionsType == "imagex")
                {
                    var folder = "";
                    var optionsFolder = options?.Value<string>("uploadfolder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }

                    SaveFile(data?["url"].ToString(), folder);
                    SaveFile(data?["cropUrl"].ToString(), folder);
                }
                else if (optionsType == "mlimagex")
                {
                    var folder = "";
                    var optionsFolder = options?.Value<string>("uploadfolder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    if (json is JObject)
                    {
                        foreach (var item in (data as JObject).Children<JProperty>())
                        {
                            if (item.Type == JTokenType.Object)
                            {
                                SaveFile(item.Value?["url"].ToString(), folder);
                                SaveFile(item.Value?["cropUrl"].ToString(), folder);
                            }
                        }
                    }
                }
                else if (optionsType == "file2")
                {
                    var folder = "";
                    var optionsFolder = options?.Value<string>("folder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    SaveFile(data.Value<int>(), folder);
                }
                else if (optionsType == "mlfile2")
                {
                    var folder = "";
                    var optionsFolder = options?.Value<string>("folder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    if (json is JObject)
                    {
                        foreach (var item in (data as JObject).Children<JProperty>())
                        {
                            SaveFile(item.Value<int>(), folder);
                        }
                    }
                }
            });
        }
        private JToken ImportTraverse(JObject alpaca, JToken json)
        {
            var ModuleFilesFolder = /*PortalSettings.HomeDirectory +*/ "OpenContent/Files/" + ModuleId.ToString() + "/";
            var ModuleCroppedFolder = /*PortalSettings.HomeDirectory +*/ "OpenContent/Cropped/" + ModuleId.ToString() + "/";
            return JsonTraverse.Traverse(json.DeepClone(), alpaca["schema"] as JObject, alpaca["options"] as JObject, (data, schema, options) =>
            {
                var optionsType = options?.Value<string>("type");
                if (optionsType == "image" || optionsType == "file")
                {
                    return ImportFile(data, ModuleFilesFolder);
                }
                else if (optionsType == "mlimage" || optionsType == "mlfile")
                {
                    if (json is JObject)
                    {
                        var newObj = new JObject();
                        foreach (var item in (data as JObject).Children<JProperty>())
                        {
                            newObj[item.Name] = ImportFile(item.Value, ModuleFilesFolder);
                        }
                        return newObj;
                    }
                }
                else if (optionsType == "imagex")
                {
                    var folder = "";
                    var optionsFolder = options?.Value<string>("uploadfolder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    ImportFile(data, "url", ModuleFilesFolder, folder, true);
                    ImportFile(data, "cropUrl", ModuleCroppedFolder, folder);
                }
                else if (optionsType == "mlimagex")
                {
                    var folder = "";
                    var optionsFolder = options?.Value<string>("uploadfolder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    if (json is JObject)
                    {
                        foreach (var item in (data as JObject).Children<JProperty>())
                        {
                            ImportFile(item.Value, "url", ModuleFilesFolder, folder, true);
                            ImportFile(item.Value, "cropUrl", ModuleCroppedFolder, folder);
                        }
                    }
                }
                else if (optionsType == "file2")
                {
                    var filename = Files[data.ToString()].ToString();
                    var folder = ""; // ModuleFilesFolder;
                    var optionsFolder = options?.Value<string>("folder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    return ImportFileId(filename, ModuleFilesFolder, folder);
                }
                else if (optionsType == "mlfile2")
                {
                    var folder = ""; // ModuleFilesFolder;
                    var optionsFolder = options?.Value<string>("folder");
                    if (!string.IsNullOrEmpty(optionsFolder))
                    {
                        folder = optionsFolder + "/";
                    }
                    if (json is JObject)
                    {
                        var newObj = new JObject();
                        foreach (var item in (data as JObject).Children<JProperty>())
                        {
                            var filename = Files[data.ToString()].ToString();
                            newObj[item.Name] = ImportFileId(filename, ModuleFilesFolder, folder);
                        }
                        return newObj;
                    }
                }
                return data;
            });
        }

        private void ImportFile(JToken data, string field, string ModuleFilesFolder, string folder, bool setId = false)
        {
            var filename = Path.GetFileName(data[field].ToString());
            var sourceFolder = folder;
            var destinationFolder = string.IsNullOrEmpty(folder) ? ModuleFilesFolder : folder;
            data[field] = new JValue(PortalSettings.HomeDirectory + destinationFolder + filename);
            var file = ImportFile(filename, sourceFolder, destinationFolder);
            if (setId && file != null)
            {
                data["id"] = new JValue(file.FileId.ToString());
            }
        }

        private JToken ImportFile(JToken data, string ModuleFilesFolder)
        {
            var filename = Path.GetFileName(data.ToString());
            ImportFile(filename, "", ModuleFilesFolder);
            return new JValue(PortalSettings.HomeDirectory + ModuleFilesFolder + filename);
        }

        private JToken ImportFileId(string filename, string ModuleFilesFolder, string folder)
        {
            var sourceFolder = folder;
            var destinationFolder = string.IsNullOrEmpty(folder) ? ModuleFilesFolder : folder;
            var file = ImportFile(filename, sourceFolder, destinationFolder);
            if (file != null)
                return new JValue(file.FileId.ToString());
            else
                return null;
        }

        private IFileInfo ImportFile(string filename, string sourceFolder, string destinationFolder)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            var sourceFilename = sourceFolder + filename;
            if (!File.Exists(sourceFilename)) return null;
            return AddFile(Path.GetFileName(filename), destinationFolder, sourceFilename);
        }

        private IFileInfo AddFile(string filename, string folder, string sourceFilename)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;

            var f = folderManager.GetFolder(PortalId, folder);
            if (f == null)
            {
                f = folderManager.AddFolder(PortalId, folder);
            }
            if (sourceFilename.IndexOf('?') > 0)
            {
                sourceFilename = sourceFilename.Substring(0, sourceFilename.IndexOf('?'));
            }
            if (filename.IndexOf('?') > 0)
            {
                filename = filename.Substring(0, filename.IndexOf('?'));
            }
            return fileManager.AddFile(f, filename, File.OpenRead(sourceFilename), true);
        }

        private void SaveFile(string url, string folder)
        {
            if (string.IsNullOrEmpty(url)) return;

            if (url.IndexOf('?') > 0)
            {
                url = url.Substring(0, url.IndexOf('?'));
            }

            var sourceFilename = HostingEnvironment.MapPath("~/" + url);
            var destinationFilename = ModuleExportDirectory + folder + Path.GetFileName(url);
            if (File.Exists(sourceFilename))
            {
                if (!Directory.Exists(ModuleExportDirectory + folder))
                    Directory.CreateDirectory(ModuleExportDirectory + folder);

                File.Copy(sourceFilename, destinationFilename, true);
                if (Files[url] == null)
                    Files.Add(url, folder + Path.GetFileName(url));
            }
        }
        private void SaveFile(int fileId, string folder)
        {
            var fileManager = FileManager.Instance;
            var file = fileManager.GetFile(fileId);

            var sourceFilename = file.PhysicalPath;
            var destinationFilename = ModuleExportDirectory + folder + file.FileName;
            if (!Directory.Exists(ModuleExportDirectory + folder))
                Directory.CreateDirectory(ModuleExportDirectory + folder);

            if (File.Exists(sourceFilename))
            {
                File.Copy(sourceFilename, destinationFilename, true);
                if (Files[fileId.ToString()] == null)
                    Files.Add(fileId.ToString(), folder + file.FileName);
            }
        }
        public void Import(string filename, bool importTemplate, bool importData, bool importAdditionalData, bool importSettings)
        {
            // unzip all
            string allStartPath = ModuleImportDirectory;
            string allZipPath = ImportDirectory + filename;

            if (Directory.Exists(allStartPath))
                Directory.Delete(allStartPath, true);

            ZipFile.ExtractToDirectory(allZipPath, allStartPath);

            var export = JObject.Parse(File.ReadAllText(ModuleImportDirectory + "export.json"));

            Files = export["Files"] as JObject;
            // unzip template
            if (importTemplate)
            {
                string startPath = HostingEnvironment.MapPath("~/" + PortalSettings.HomeDirectory + "OpenContent/Templates/" + export["Name"]);
                if (!Directory.Exists(startPath))
                    Directory.CreateDirectory(startPath);
                string zipPath = ModuleImportDirectory + export["Name"] + ".zip";

                if (Directory.Exists(startPath))
                    Directory.Delete(startPath, true);

                ZipFile.ExtractToDirectory(zipPath, startPath);
            }
            if (importSettings)
            {
                ModuleController mc = new ModuleController();
                mc.UpdateModuleSetting(ModuleId, "template", export["Template"].ToString()
                    .Replace("Portals/" + export["PortalId"] + "/", "Portals/" + PortalId + "/"));
                //.Replace("[PORTALID]", PortalId.ToString()));
                if (export["Settings"] != null)
                {
                    mc.UpdateModuleSetting(ModuleId, "data", export["Settings"].ToString());
                }
                mc.UpdateModuleSetting(ModuleId, "query", export["Query"].ToString());

                DataCache.ClearCache();
            }
            var module = OpenContentModuleConfig.Create(ModuleId, TabId, PortalSettings);
            IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(module);
            if (importData)
            {
                var alpaca = ds.GetAlpaca(dsContext, true, true, false);
                var data = JToken.Parse(File.ReadAllText(ModuleImportDirectory + "data.json"));
                if (module.IsListMode())
                {
                    var dataList = ds.GetAll(dsContext, null).Items;
                    foreach (var item in dataList)
                    {
                        ds.Delete(dsContext, item);
                    }
                    if (data is JArray)
                    {
                        foreach (JObject json in data)
                        {
                            ds.Add(dsContext, ImportTraverse(alpaca, json));
                        }
                    }
                }
                else
                {
                    var dsItem = ds.Get(dsContext, null);
                    data = ImportTraverse(alpaca, data);
                    if (dsItem == null)
                    {
                        ds.Add(dsContext, data);
                    }
                    else
                    {
                        ds.Update(dsContext, dsItem, data);
                    }
                }
            }

            if (importAdditionalData && module.Settings.Manifest.AdditionalDataDefinition != null)
            {
                foreach (var item in module.Settings.Manifest.AdditionalDataDefinition)
                {
                    var alpaca = ds.GetDataAlpaca(dsContext, true, true, false, item.Key);
                    var dataFile = ModuleImportDirectory + item.Key + ".json";
                    if (File.Exists(dataFile))
                    {
                        var data = JToken.Parse(File.ReadAllText(dataFile));

                        var dsItem = ds.GetData(dsContext, item.Value.ScopeType, item.Key);
                        data = ImportTraverse(alpaca, data);
                        if (dsItem == null)
                        {
                            ds.AddData(dsContext, item.Value.ScopeType, item.Key, data);
                        }
                        else
                        {
                            ds.UpdateData(dsContext, dsItem, data);
                        }
                    }
                }
            }
            //Directory.Delete(allStartPath, true);
        }
    }
}