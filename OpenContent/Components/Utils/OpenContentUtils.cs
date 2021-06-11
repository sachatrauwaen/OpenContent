using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
//using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Lucene.Config;
using UserRoleInfo = Satrabel.OpenContent.Components.Querying.UserRoleInfo;
using DotNetNuke.UI.Skins;

namespace Satrabel.OpenContent.Components
{
    public static class OpenContentUtils
    {
        public static void HydrateDefaultFields(this OpenContentInfo content, FieldConfig indexConfig)
        {
            if (indexConfig.HasField(App.Config.FieldNamePublishStartDate)
                   && content.JsonAsJToken != null && content.JsonAsJToken[App.Config.FieldNamePublishStartDate] == null)
            {
                content.JsonAsJToken[App.Config.FieldNamePublishStartDate] = DateTime.MinValue;
            }
            if (indexConfig.HasField(App.Config.FieldNamePublishEndDate)
                && content.JsonAsJToken != null)
            {
                if (content.JsonAsJToken[App.Config.FieldNamePublishEndDate] == null)
                {
                    content.JsonAsJToken[App.Config.FieldNamePublishEndDate] = DateTime.MaxValue;
                }
                else
                {
                    // if the enddata has no time, we add 23:59:59.999 as a time
                    var t = content.JsonAsJToken[App.Config.FieldNamePublishEndDate].Value<DateTime>();
                    if (t.TimeOfDay.TotalMilliseconds == 0)
                    {
                        var contentJToken = content.JsonAsJToken;
                        contentJToken[App.Config.FieldNamePublishEndDate] = t.AddDays(1).AddMilliseconds(-1);
                        content.JsonAsJToken = contentJToken;
                    }
                }
                
            }
            if (indexConfig.HasField(App.Config.FieldNamePublishStatus)
                && content.JsonAsJToken != null && content.JsonAsJToken[App.Config.FieldNamePublishStatus] == null)
            {
                content.JsonAsJToken[App.Config.FieldNamePublishStatus] = "published";
            }
        }

        public static string GetSiteTemplateFolder(PortalSettings portalSettings, string moduleSubDir)
        {
            return portalSettings.HomeDirectory + moduleSubDir + "/Templates/";
        }
        public static string GetSkinTemplateFolder(PortalSettings portalSettings, string moduleSubDir)
        {
            var SkinPath = portalSettings.ActiveTab.SkinPath;
            if (string.IsNullOrEmpty(SkinPath))
            {
                var SkinSrc = SkinController.FormatSkinSrc(!string.IsNullOrEmpty(portalSettings.ActiveTab.SkinSrc) ? portalSettings.ActiveTab.SkinSrc : portalSettings.DefaultPortalSkin, portalSettings);
                SkinPath = SkinController.FormatSkinPath(SkinSrc);
                //SkinPath = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(portalSettings.ActiveTab.TabID, portalSettings.PortalId).SkinPath;
            }
            return SkinPath + moduleSubDir + "/Templates/";
        }

        public static string GetHostTemplateFolder(PortalSettings portalSettings, string moduleSubDir)
        {
            var hostPath = "~/Portals/_Default/";

            return hostPath + moduleSubDir + "/Templates/";
        }

        public static List<ListItem> GetTemplates(PortalSettings portalSettings, int moduleId, string selectedTemplate, string moduleSubDir)
        {
            return GetTemplates(portalSettings, moduleId, new FileUri(selectedTemplate), moduleSubDir);
        }

        /// <summary>
        /// Gets the templates files.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="selectedTemplate">The selected template.</param>
        /// <param name="moduleSubDir">The module sub dir.</param>
        /// <returns></returns>
        /// <remarks>Used by OpenForms</remarks>
        [Obsolete("This method is obsolete since dec 2015; use GetTemplatesFiles(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir) instead")]
        public static List<ListItem> ListOfTemplatesFiles(PortalSettings portalSettings, int moduleId, string selectedTemplate, string moduleSubDir)
        {
            return ListOfTemplatesFiles(portalSettings, moduleId, new FileUri(selectedTemplate).ToTemplateManifest(), moduleSubDir);
        }

        [Obsolete("This method is obsolete since dec 2015; use GetTemplatesFiles(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir) instead")]
        public static List<ListItem> GetTemplates(PortalSettings portalSettings, int moduleId, FileUri selectedTemplate, string moduleSubDir)
        {
            if (selectedTemplate == null)
            {
                return GetTemplates(portalSettings, moduleId, null as TemplateManifest, moduleSubDir);
            }
            return GetTemplates(portalSettings, moduleId, selectedTemplate.ToTemplateManifest(), moduleSubDir);
        }

        /// <summary>
        /// Gets the templates files.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="selectedTemplate">The selected template.</param>
        /// <param name="moduleSubDir">The module sub dir.</param>
        /// <returns></returns>
        public static List<ListItem> ListOfTemplatesFiles(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir, bool advanced = true)
        {
            return ListOfTemplatesFiles(portalSettings, moduleId, selectedTemplate, moduleSubDir, null, advanced);
        }

        public static List<ListItem> ListOfTemplatesFiles(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir, FileUri otherModuleTemplate, bool advanced = true)
        {
            string basePath = HostingEnvironment.MapPath(GetSiteTemplateFolder(portalSettings, moduleSubDir));
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            var dirs = Directory.GetDirectories(basePath);
            if (otherModuleTemplate != null)
            {
                var selDir = otherModuleTemplate.PhysicalFullDirectory;
                if (!dirs.Contains(selDir))
                {
                    selDir = Path.GetDirectoryName(selDir);
                }
                if (dirs.Contains(selDir))
                    dirs = new string[] { selDir };
                else
                    dirs = new string[] { };
            }
            List<ListItem> lst = new List<ListItem>();
            foreach (var dir in dirs)
            {
                string templateCat = "Site";
                string dirName = dir.Substring(basePath.Length);
                int modId = -1;
                if (Int32.TryParse(dirName, out modId))
                {
                    // if numeric directory name --> module template
                    if (modId == moduleId)
                    {
                        // this module -> show
                        templateCat = "Module";
                    }
                    else
                    {
                        // if it's from an other module -> don't show
                        continue;
                    }
                }

                IEnumerable<string> files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories);
                IEnumerable<string> manifestfiles = files.Where(s => s.EndsWith("manifest.json"));
                var manifestTemplateFound = false;
                if (manifestfiles.Any())
                {
                    foreach (string manifestFile in manifestfiles)
                    {
                        FileUri manifestFileUri = FileUri.FromPath(manifestFile);
                        var manifest = ManifestUtils.LoadManifestFileFromCacheOrDisk(manifestFileUri);
                        if (manifest != null && manifest.HasTemplates)
                        {
                            manifestTemplateFound = true;
                            if (advanced || !manifest.Advanced)
                            {
                                foreach (var template in manifest.Templates)
                                {
                                    FileUri templateUri = new FileUri(manifestFileUri.FolderPath, template.Key);
                                    string templateName = Path.GetDirectoryName(manifestFile).Substring(basePath.Length).Replace("\\", " / ");
                                    if (!String.IsNullOrEmpty(template.Value.Title))
                                    {
                                        if (advanced)
                                            templateName = templateName + " - " + template.Value.Title;

                                    }
                                    var item = new ListItem((templateCat == "Site" ? "" : templateCat + " : ") + templateName, templateUri.FilePath);
                                    if (selectedTemplate != null && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                                    {
                                        item.Selected = true;
                                    }
                                    lst.Add(item);
                                    if (!advanced) break;
                                }
                            }
                        }
                    }
                }
                if (!manifestTemplateFound)
                {
                    IEnumerable<string> scriptfiles = files.Where(s => s.EndsWith(".cshtml") || s.EndsWith(".vbhtml") || s.EndsWith(".hbs"));
                    foreach (string script in scriptfiles)
                    {
                        FileUri templateUri = FileUri.FromPath(script);

                        string scriptName = script.Remove(script.LastIndexOf(".")).Replace(basePath, "");
                        if (templateCat == "Module")
                        {
                            if (scriptName.ToLower().EndsWith("template"))
                                scriptName = "";
                            else
                                scriptName = scriptName.Substring(scriptName.LastIndexOf("\\") + 1);
                        }
                        else if (scriptName.ToLower().EndsWith("template"))
                            scriptName = scriptName.Remove(scriptName.LastIndexOf("\\"));
                        else
                            scriptName = scriptName.Replace("\\", " - ");

                        var item = new ListItem((templateCat == "Site" ? "" : templateCat + " : ") + scriptName, templateUri.FilePath);
                        if (selectedTemplate != null && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                        {
                            item.Selected = true;
                        }
                        lst.Add(item);
                    }
                }
            }
            // skin
            //if (!string.IsNullOrEmpty(portalSettings.ActiveTab.SkinPath))
            {
                basePath = HostingEnvironment.MapPath(GetSkinTemplateFolder(portalSettings, moduleSubDir));

                dirs = GetDirs(selectedTemplate, otherModuleTemplate, advanced, basePath, dirs, lst, "Skin");
            }
            // Host
            {
                basePath = HostingEnvironment.MapPath(GetHostTemplateFolder(portalSettings, moduleSubDir));
                dirs = GetDirs(selectedTemplate, otherModuleTemplate, advanced, basePath, dirs, lst, "Host");
            }
            return lst;
        }

        private static string[] GetDirs(TemplateManifest selectedTemplate, FileUri otherModuleTemplate, bool advanced, string basePath, string[] dirs, List<ListItem> lst, string folderType)
        {
            if (Directory.Exists(basePath))
            {
                dirs = Directory.GetDirectories(basePath);
                if (otherModuleTemplate != null /*&& */ )
                {
                    var selDir = otherModuleTemplate.PhysicalFullDirectory;
                    if (!dirs.Contains(selDir))
                    {
                        selDir = Path.GetDirectoryName(selDir);
                    }
                    if (dirs.Contains(selDir))
                        dirs = new string[] { selDir };
                    else
                        dirs = new string[] { };
                }

                foreach (var dir in dirs)
                {
                    string templateCat = folderType;

                    IEnumerable<string> files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories);
                    IEnumerable<string> manifestfiles = files.Where(s => s.EndsWith("manifest.json"));
                    var manifestTemplateFound = false;

                    if (manifestfiles.Any())
                    {
                        foreach (string manifestFile in manifestfiles)
                        {
                            FileUri manifestFileUri = FileUri.FromPath(manifestFile);
                            var manifest = ManifestUtils.LoadManifestFileFromCacheOrDisk(manifestFileUri);
                            if (manifest != null && manifest.HasTemplates)
                            {
                                manifestTemplateFound = true;
                                if (advanced || !manifest.Advanced)
                                {
                                    foreach (var template in manifest.Templates)
                                    {
                                        FileUri templateUri = new FileUri(manifestFileUri.FolderPath, template.Key);
                                        string templateName = Path.GetDirectoryName(manifestFile).Substring(basePath.Length).Replace("\\", " / ");
                                        if (!String.IsNullOrEmpty(template.Value.Title))
                                        {
                                            if (advanced)
                                                templateName = templateName + " - " + template.Value.Title;
                                        }
                                        var item = new ListItem((templateCat == "Site" ? "" : templateCat + " : ") + templateName, templateUri.FilePath);
                                        if (selectedTemplate != null && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                                        {
                                            item.Selected = true;
                                        }
                                        lst.Add(item);
                                        if (!advanced) break;
                                    }
                                }
                            }
                        }
                    }
                    if (!manifestTemplateFound)
                    {
                        var scriptfiles =
                            Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                                .Where(s => s.EndsWith(".cshtml") || s.EndsWith(".vbhtml") || s.EndsWith(".hbs"));
                        foreach (string script in scriptfiles)
                        {
                            string scriptName = script.Remove(script.LastIndexOf(".")).Replace(basePath, "");
                            if (scriptName.ToLower().EndsWith("template")) scriptName = scriptName.Remove(scriptName.LastIndexOf("\\"));
                            else scriptName = scriptName.Replace("\\", " - ");

                            FileUri templateUri = FileUri.FromPath(script);
                            var item = new ListItem(templateCat + " : " + scriptName, templateUri.FilePath);
                            if (selectedTemplate != null
                                && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                            {
                                item.Selected = true;
                            }
                            lst.Add(item);
                        }
                    }
                }

            }

            return dirs;
        }

        public static List<ListItem> GetTemplates(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir)
        {
            string basePath = HostingEnvironment.MapPath(GetSiteTemplateFolder(portalSettings, moduleSubDir));
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            List<ListItem> lst = new List<ListItem>();
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                string templateCat = "Site";
                string dirName = Path.GetFileNameWithoutExtension(dir);
                int modId = -1;
                if (Int32.TryParse(dirName, out modId))
                {
                    if (modId == moduleId)
                    {
                        templateCat = "Module";
                    }
                    else
                    {
                        continue;
                    }
                }
                string scriptName = dir;
                if (templateCat == "Module")
                    scriptName = templateCat;
                else
                    scriptName = templateCat + ":" + scriptName.Substring(scriptName.LastIndexOf("\\") + 1);

                string scriptPath = FolderUri.ReverseMapPath(dir);
                var item = new ListItem(scriptName, scriptPath);
                if (selectedTemplate != null && scriptPath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                {
                    item.Selected = true;
                }
                lst.Add(item);
            }
            // skin
            //if (!string.IsNullOrEmpty(portalSettings.ActiveTab.SkinPath))
            {
                basePath = HostingEnvironment.MapPath(GetSkinTemplateFolder(portalSettings, moduleSubDir));
                if (Directory.Exists(basePath))
                {
                    foreach (var dir in Directory.GetDirectories(basePath))
                    {
                        string templateCat = "Skin";
                        string scriptName = dir;
                        scriptName = templateCat + ":" + scriptName.Substring(scriptName.LastIndexOf("\\") + 1);
                        string scriptPath = FolderUri.ReverseMapPath(dir);
                        var item = new ListItem(scriptName, scriptPath);
                        if (selectedTemplate != null && scriptPath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                        {
                            item.Selected = true;
                        }
                        lst.Add(item);
                    }
                }
            }
            {
                basePath = HostingEnvironment.MapPath(GetHostTemplateFolder(portalSettings, moduleSubDir));
                if (Directory.Exists(basePath))
                {
                    foreach (var dir in Directory.GetDirectories(basePath))
                    {
                        string templateCat = "Host";
                        string scriptName = dir;
                        scriptName = templateCat + ":" + scriptName.Substring(scriptName.LastIndexOf("\\") + 1);
                        string scriptPath = FolderUri.ReverseMapPath(dir);
                        var item = new ListItem(scriptName, scriptPath);
                        if (selectedTemplate != null && scriptPath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                        {
                            item.Selected = true;
                        }
                        lst.Add(item);
                    }
                }
            }
            return lst;
        }

        public static string CopyTemplate(int portalId, string fromFolder, string newTemplateName)
        {
            string folderName = "OpenContent/Templates/" + newTemplateName;
            var folder = FolderManager.Instance.GetFolder(portalId, folderName);
            if (folder != null)
            {
                throw new Exception("Template already exist " + folder.FolderPath);
            }
            folder = FolderManager.Instance.AddFolder(portalId, folderName);
            foreach (var item in Directory.GetFiles(fromFolder))
            {
                File.Copy(item, folder.PhysicalPath + Path.GetFileName(item));

            }
            return GetDefaultTemplate(folder.PhysicalPath);
        }

        public static string ImportFromWeb(int portalId, string fileName, string newTemplateName)
        {
            //string FileName = ddlWebTemplates.SelectedValue;
            string strMessage = "";
            try
            {
                var folder = FolderManager.Instance.GetFolder(portalId, "OpenContent/Templates");
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(portalId, "OpenContent/Templates");
                }
                if (Path.GetExtension(fileName) == ".zip")
                {
                    if (String.IsNullOrEmpty(newTemplateName))
                    {
                        newTemplateName = Path.GetFileNameWithoutExtension(fileName);
                    }
                    string folderName = "OpenContent/Templates/" + newTemplateName;
                    folder = FolderManager.Instance.GetFolder(portalId, folderName);
                    if (folder != null)
                    {
                        throw new Exception("Template already exist " + folder.FolderName);
                    }
                    folder = FolderManager.Instance.AddFolder(portalId, folderName);
                    var req = (HttpWebRequest)WebRequest.Create(fileName);
                    Stream stream = req.GetResponse().GetResponseStream();
                    //var file = FileManager.Instance.AddFile(folder, fileName, stream, true);
                    //FileManager.Instance.UnzipFile(file);

                    //FileSystemUtils.UnzipResources(new ZipInputStream(stream), folder.PhysicalPath);
                    var zip = new ZipUtils();
                    zip.UnzipFiles(stream, folder.PhysicalPath);
                    return GetDefaultTemplate(folder.PhysicalPath);
                }
            }
            catch (PermissionsNotMetException)
            {
                //Logger.Warn(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("InsufficientFolderPermission"), "OpenContent/Templates");
            }
            catch (NoSpaceAvailableException)
            {
                //Logger.Warn(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("DiskSpaceExceeded"), fileName);
            }
            catch (InvalidFileExtensionException)
            {
                //Logger.Warn(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception exc)
            {
                //Logger.Error(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("SaveFileError") + " - " + exc.Message, fileName);
            }
            if (!String.IsNullOrEmpty(strMessage))
            {
                throw new Exception(strMessage);
            }
            return "";
        }

        public static string GetDefaultTemplate(string physicalFolder)
        {
            string template = "";
            FolderUri folder = new FolderUri(FolderUri.ReverseMapPath(physicalFolder));
            var manifest = ManifestUtils.LoadManifestFileFromCacheOrDisk(folder);
            if (manifest != null && manifest.HasTemplates)
            {
                //get the requested template key
                //var templateManifest = manifest.Templates.First().Value;
                //var templateUri = new FileUri(folder, templateManifest.Main.Template);
                template = folder.FolderPath + "/" + manifest.Templates.First().Key;
            }
            else
            {
                foreach (var item in Directory.GetFiles(physicalFolder))
                {
                    string fileName = Path.GetFileName(item).ToLower();
                    if (fileName == "template.hbs")
                    {
                        template = item;
                        break;
                    }
                    else if (fileName == "template.cshtml")
                    {
                        template = item;
                        break;
                    }
                    if (fileName.EndsWith(".hbs"))
                    {
                        template = item;
                    }
                    if (fileName.EndsWith(".cshtml"))
                    {
                        template = item;
                    }
                }
            }
            return FileUri.ReverseMapPath(template);
        }

        /// <summary>
        /// Checks the OpenContent settings.
        /// </summary>
        public static bool CheckOpenContentTemplateFiles(OpenContentModuleConfig module)
        {
            bool result = true;
            var settings = module.Settings;
            if (settings?.TemplateKey?.TemplateDir != null && !settings.TemplateKey.TemplateDir.FolderExists)
            {
                var url = DnnUrlUtils.NavigateUrl(module.ViewModule.TabId);
                App.Services.Logger.Error($"Error loading OpenContent Template on page [{module.ViewModule.PortalId}-{module.ViewModule.TabId}-{url}] module [{module.ViewModule.ModuleId}-{module.ViewModule.ModuleTitle}]. Reason: Template not found [{settings.TemplateKey}]");
                result = false;
            }
            return result;
        }

        [Obsolete("This method is obsolete since aug 2017; use another constructor instead.")]
        public static DataSourceContext CreateDataContext(OpenContentModuleInfo moduleinfo, int userId = -1, bool single = false, JObject options = null)
        {
            var module = OpenContentModuleConfig.Create(moduleinfo.ModuleId, moduleinfo.TabId, PortalSettings.Current);
            return CreateDataContext(module, userId, single, options);
        }

        public static DataSourceContext CreateDataContext(OpenContentModuleConfig module, int userId = -1, bool single = false, JObject options = null)
        {
            var template = module.Settings.Template;
            if (template == null)
            {
                App.Services.Logger.Error($"Template [{(module.Settings.TemplateAvailable ? module.Settings.TemplateKey.ToString() : "???")}] not found"); // are you importing and forgot to install the files?
            }

            var dsContext = new DataSourceContext
            {
                PortalId = module.ViewModule.PortalId,
                ActiveModuleId = module.ViewModule.ModuleId,
                TabId = module.ViewModule.TabId,
                TabModuleId = module.ViewModule.TabModuleId,
                ModuleId = module.DataModule.ModuleId,
                TemplateFolder = module.Settings.TemplateDir?.FolderPath,
                UserId = userId,
                Config = module.Settings.Manifest?.DataSourceConfig,
                Index = template?.Manifest?.Index ?? false,
                Options = options,
                Single = single,
                Collection = template?.Collection ?? ""
            };
            if (PortalSettings.Current != null)
            {
                //PortalSettings is null if called from scheduler (eg UrlRewriter, Search, ...)
                dsContext.CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode();
            }
            return dsContext;
        }

        public static string ReverseMapPath(string path)
        {
            return FolderUri.ReverseMapPath(path);
        }


        public static FieldConfig GetIndexConfig(TemplateManifest template)
        {
            return GetIndexConfig(template.Key.TemplateDir, template.Collection);
        }
        public static FieldConfig GetIndexConfig(FolderUri folder, string collection)
        {
            try
            {
                var fb = new FormBuilder(folder);
                FieldConfig indexConfig = fb.BuildIndex(collection);
                return indexConfig;
            }
            catch (Exception ex)
            {
                //we should log this
                App.Services.Logger.Error($"Error while parsing json", ex);
                Utils.DebuggerBreak();
                return null;
            }
        }

        [Obsolete("This method is obsolete since may 2016; use UrlHelpers.CleanupUrl(string url) instead")]
        public static string CleanupUrl(string url)
        {
            return url.CleanupUrl();
        }

        internal static bool BuilderExist(FolderUri folder, string prefix = "")
        {
            if (folder.FolderExists)
                return File.Exists(folder.PhysicalFullDirectory + "\\" + (string.IsNullOrEmpty(prefix) ? "" : prefix + "-") + "builder.json");
            return false;
        }

        internal static bool BuildersExist(FolderUri folder)
        {
            if (folder.FolderExists)
                return Directory.GetFiles(folder.PhysicalFullDirectory, "*builder.json").Length > 0;
            return false;
        }

        internal static bool FormExist(FolderUri folder)
        {
            if (folder.FolderExists)
                return Directory.GetFiles(folder.PhysicalFullDirectory, "form-schema.json").Length > 0;
            return false;
        }

        internal static bool IsViewAllowed(IDataItem dsItem, IList<UserRoleInfo> userRoles, FieldConfig indexConfig, out string raison)
        {
            raison = "";
            if (dsItem?.Data == null) return true;

            //publish status , dates
            var isAllowed = IsPublished(dsItem, indexConfig, out raison);
            // user and roles
            if (isAllowed) isAllowed = HaveViewPermissions(dsItem, userRoles, indexConfig, out raison);

            return isAllowed;
        }
        /// <summary>
        /// Check the user's permissions to view the item. NOTE: as of 2021-05-22 use IsViewAllowed to also check publish status and date.
        /// </summary>
        /// <param name="dsItem"></param>
        /// <param name="userRoles"></param>
        /// <param name="indexConfig"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        internal static bool HaveViewPermissions(IDataItem dsItem, IList<UserRoleInfo> userRoles, FieldConfig indexConfig, out string reason)
        {
            reason = "";
            if (dsItem?.Data == null) return true;

            bool permissions = true;

            // Roles                
            string fieldName = "";
            if (indexConfig?.Fields != null && indexConfig.Fields.ContainsKey("userrole"))
            {
                fieldName = "userrole";
            }
            else if (indexConfig?.Fields != null && indexConfig.Fields.ContainsKey("userroles"))
            {
                fieldName = "userroles";
            }
            if (!String.IsNullOrEmpty(fieldName))
            {
                permissions = false;
                string[] dataRoles = { };
                if (dsItem.Data[fieldName] != null)
                {
                    if (dsItem.Data[fieldName].Type == JTokenType.Array)
                    {
                        dataRoles = ((JArray)dsItem.Data[fieldName]).Select(d => d.ToString()).ToArray();
                    }
                    else
                    {
                        dataRoles = new string[] { dsItem.Data[fieldName].ToString() };
                    }
                }
                if (dataRoles.Contains("AllUsers"))
                {
                    permissions = true;
                }
                else
                {
                    var roles = userRoles;
                    if (roles.Any())
                    {
                        permissions = roles.Any(r => dataRoles.Contains(r.RoleId.ToString()));
                    }
                    else
                    {
                        permissions = dataRoles.Contains("Unauthenticated");
                    }
                }
                if (!permissions) reason = fieldName;
            }
            return permissions;

        }

        internal static bool IsPublished(IDataItem dsItem, FieldConfig indexConfig, out string reason)
        {
            reason = "";
            if (dsItem?.Data == null) return true;

            bool isPublished = true;
            if (indexConfig?.Fields != null && indexConfig.Fields.ContainsKey(App.Config.FieldNamePublishStatus))
            {
                isPublished = dsItem.Data[App.Config.FieldNamePublishStatus] != null &&
                    dsItem.Data[App.Config.FieldNamePublishStatus].ToString() == "published";
                if (!isPublished) reason = App.Config.FieldNamePublishStatus + $" being {dsItem.Data[App.Config.FieldNamePublishStatus]}";
            }
            if (isPublished && indexConfig?.Fields != null && indexConfig.Fields.ContainsKey(App.Config.FieldNamePublishStartDate))
            {
                if (dsItem.Data[App.Config.FieldNamePublishStartDate] != null && dsItem.Data[App.Config.FieldNamePublishStartDate].Type == JTokenType.Date)
                {
                    var compareDate = (DateTime)dsItem.Data[App.Config.FieldNamePublishStartDate];
                    // do we need to compare time?
                    if (compareDate.TimeOfDay.TotalMilliseconds > 0)
                    {
                        isPublished = compareDate <= DateTime.Now;
                    }
                    else
                    {
                        isPublished = compareDate <= DateTime.Today;
                    }
                }
                else
                {
                    // not a date
                    isPublished = false;
                }

                if (!isPublished) reason = App.Config.FieldNamePublishStartDate + $" being {dsItem.Data[App.Config.FieldNamePublishStartDate]}";
            }
            if (isPublished && indexConfig?.Fields != null && indexConfig.Fields.ContainsKey(App.Config.FieldNamePublishEndDate))
            {
                if (dsItem.Data[App.Config.FieldNamePublishEndDate] != null && dsItem.Data[App.Config.FieldNamePublishEndDate].Type == JTokenType.Date)
                {
                    var compareDate = (DateTime)dsItem.Data[App.Config.FieldNamePublishEndDate];
                    // do we need to compare time?
                    if (compareDate.TimeOfDay.TotalMilliseconds > 0)
                    {
                        isPublished = compareDate >= DateTime.Now;
                    }
                    else
                    {
                        isPublished = compareDate >= DateTime.Today;
                    }
                }
                else
                {
                    // not a date
                    isPublished = false;
                }

                if (!isPublished) reason = App.Config.FieldNamePublishEndDate + $" being {dsItem.Data[App.Config.FieldNamePublishEndDate]}";
            }

            return isPublished;
        }
    }
}