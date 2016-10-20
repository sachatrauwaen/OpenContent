using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Lucene.Index;
using Satrabel.OpenContent.Components.TemplateHelpers;
using Newtonsoft.Json.Linq;


namespace Satrabel.OpenContent.Components
{
    public static class OpenContentUtils
    {
        public static void HydrateDefaultFields(this OpenContentInfo content, FieldConfig indexConfig)
        {
            if (indexConfig.HasField(AppConfig.FieldNamePublishStartDate)
                   && content.JsonAsJToken != null && content.JsonAsJToken[AppConfig.FieldNamePublishStartDate] == null)
            {
                content.JsonAsJToken[AppConfig.FieldNamePublishStartDate] = DateTime.MinValue;
            }
            if (indexConfig.HasField(AppConfig.FieldNamePublishEndDate)
                && content.JsonAsJToken != null && content.JsonAsJToken[AppConfig.FieldNamePublishEndDate] == null)
            {
                content.JsonAsJToken[AppConfig.FieldNamePublishEndDate] = DateTime.MaxValue;
            }
            if (indexConfig.HasField(AppConfig.FieldNamePublishStatus)
                && content.JsonAsJToken != null && content.JsonAsJToken[AppConfig.FieldNamePublishStatus] == null)
            {
                content.JsonAsJToken[AppConfig.FieldNamePublishStatus] = "published";
            }
        }

        public static void UpdateModuleTitle(ModuleInfo module, string moduleTitle)
        {
            if (module.ModuleTitle != moduleTitle)
            {
                ModuleController mc = new ModuleController();
                var mod = mc.GetModule(module.ModuleID, module.TabID, true);
                mod.ModuleTitle = moduleTitle;
                mc.UpdateModule(mod);
            }
        }
        public static string GetSiteTemplateFolder(PortalSettings portalSettings, string moduleSubDir)
        {
            return portalSettings.HomeDirectory + moduleSubDir + "/Templates/";
        }
        public static string GetSkinTemplateFolder(PortalSettings portalSettings, string moduleSubDir)
        {
            return portalSettings.ActiveTab.SkinPath + moduleSubDir + "/Templates/";
        }

        public static List<System.Web.UI.WebControls.ListItem> GetTemplates(PortalSettings portalSettings, int moduleId, string selectedTemplate, string moduleSubDir)
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
        public static List<ListItem> GetTemplatesFiles(PortalSettings portalSettings, int moduleId, string selectedTemplate, string moduleSubDir)
        {
            return GetTemplatesFiles(portalSettings, moduleId, new FileUri(selectedTemplate).ToTemplateManifest(), moduleSubDir);
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
        public static List<ListItem> GetTemplatesFiles(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir)
        {
            return GetTemplatesFiles(portalSettings, moduleId, selectedTemplate, moduleSubDir, null);
        }

        public static List<ListItem> GetTemplatesFiles(PortalSettings portalSettings, int moduleId, TemplateManifest selectedTemplate, string moduleSubDir, FileUri otherModuleTemplate)
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
                dirs = new string[] { selDir };
            }

            List<ListItem> lst = new List<ListItem>();
            foreach (var dir in dirs)
            {
                string templateCat = "Site";
                string dirName = Path.GetFileNameWithoutExtension(dir);
                int modId = -1;
                if (int.TryParse(dirName, out modId))
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
                        var manifest = ManifestUtils.GetFileManifest(manifestFileUri);
                        if (manifest != null && manifest.HasTemplates)
                        {
                            manifestTemplateFound = true;
                            foreach (var template in manifest.Templates)
                            {
                                FileUri templateUri = new FileUri(manifestFileUri.FolderPath, template.Key);
                                string templateName = dirName;
                                if (!string.IsNullOrEmpty(template.Value.Title))
                                {
                                    templateName = templateName + " - " + template.Value.Title;
                                }
                                var item = new ListItem((templateCat == "Site" ? "" : templateCat + " : ") + templateName, templateUri.FilePath);
                                if (selectedTemplate != null && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.ToString().ToLowerInvariant())
                                {
                                    item.Selected = true;
                                }
                                lst.Add(item);
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
            basePath = HostingEnvironment.MapPath(GetSkinTemplateFolder(portalSettings, moduleSubDir));
            if (Directory.Exists(basePath))
            {
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    string templateCat = "Skin";
                    var files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                                .Where(s => s.EndsWith(".cshtml") || s.EndsWith(".vbhtml") || s.EndsWith(".hbs"));
                    foreach (string script in files)
                    {
                        string scriptName = script.Remove(script.LastIndexOf(".")).Replace(basePath, "");
                        if (scriptName.ToLower().EndsWith("template"))
                            scriptName = scriptName.Remove(scriptName.LastIndexOf("\\"));
                        else
                            scriptName = scriptName.Replace("\\", " - ");

                        string scriptPath = FolderUri.ReverseMapPath(script);
                        var item = new ListItem(templateCat + " : " + scriptName, scriptPath);
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
                if (int.TryParse(dirName, out modId))
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
                    if (string.IsNullOrEmpty(newTemplateName))
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

                    FileSystemUtils.UnzipResources(new ZipInputStream(stream), folder.PhysicalPath);
                    return GetDefaultTemplate(folder.PhysicalPath);
                }
            }
            catch (PermissionsNotMetException)
            {
                //Logger.Warn(exc);
                strMessage = string.Format(Localization.GetString("InsufficientFolderPermission"), "OpenContent/Templates");
            }
            catch (NoSpaceAvailableException)
            {
                //Logger.Warn(exc);
                strMessage = string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
            }
            catch (InvalidFileExtensionException)
            {
                //Logger.Warn(exc);
                strMessage = string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception exc)
            {
                //Logger.Error(exc);
                strMessage = string.Format(Localization.GetString("SaveFileError") + " - " + exc.Message, fileName);
            }
            if (!string.IsNullOrEmpty(strMessage))
            {
                throw new Exception(strMessage);
            }
            return "";
        }

        public static string GetDefaultTemplate(string physicalFolder)
        {
            string template = "";
            FolderUri folder = new FolderUri(FolderUri.ReverseMapPath(physicalFolder));
            var manifest = ManifestUtils.GetFileManifest(folder);
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

        public static bool CheckOpenContentSettings(ModuleInfo module, OpenContentSettings settings)
        {
            bool result = true;
            if (module != null && settings != null && settings.TemplateKey != null && settings.TemplateKey.TemplateDir != null && !settings.TemplateKey.TemplateDir.FolderExists)
            {
                var url = DnnUrlUtils.NavigateUrl(module.TabID);
                Log.Logger.ErrorFormat("Error loading OpenContent Template on page [{5}-{4}-{1}] module [{2}-{3}]. Reason: Template not found [{0}]", settings.TemplateKey.ToString(), url, module.ModuleID, module.ModuleTitle, module.TabID, module.PortalID);
                result = false;
            }
            return result;
        }

        public static string ReverseMapPath(string path)
        {
            return FolderUri.ReverseMapPath(path);
        }

        public static bool HasEditPermissions(PortalSettings portalSettings, ModuleInfo module, string editrole, int createdByUserId)
        {
            return module.HasEditRights() || HasEditRole(portalSettings, module, editrole, createdByUserId);
        }
        public static bool HasEditRole(PortalSettings portalSettings, ModuleInfo module, string editrole, int createdByUserId)
        {
            return (!string.IsNullOrEmpty(editrole) && portalSettings.UserInfo.IsInRole(editrole) && (createdByUserId == -1 || portalSettings.UserId == createdByUserId)) ||
                    (!string.IsNullOrEmpty(editrole) && editrole.ToLower() == "all");
        }

        internal static FieldConfig GetIndexConfig(FolderUri folder)
        {
            try
            {
                var fb = new FormBuilder(folder);
                FieldConfig indexConfig = fb.BuildIndex();
                return indexConfig;
            }
            catch (Exception ex)
            {
                //we should log this
                Log.Logger.ErrorFormat("Error while parsing json", ex);
                if (Debugger.IsAttached) Debugger.Break();
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


        internal static bool HaveViewPermissions(Datasource.IDataItem dsItem, DotNetNuke.Entities.Users.UserInfo userInfo, FieldConfig IndexConfig, out string raison)
        {
            raison = "";
            if (dsItem == null || dsItem.Data == null) return true;

            bool permissions = true;
            //publish status , dates
            if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey(AppConfig.FieldNamePublishStatus))
            {
                permissions = dsItem.Data[AppConfig.FieldNamePublishStatus] != null &&
                    dsItem.Data[AppConfig.FieldNamePublishStatus].ToString() == "published";
                if (!permissions) raison = AppConfig.FieldNamePublishStatus;
            }
            if (permissions && IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey(AppConfig.FieldNamePublishStartDate))
            {
                permissions =   dsItem.Data[AppConfig.FieldNamePublishStartDate] != null && 
                                dsItem.Data[AppConfig.FieldNamePublishStartDate].Type == JTokenType.Date &&
                                ((DateTime)dsItem.Data[AppConfig.FieldNamePublishStartDate]) <= DateTime.Today;
                if (!permissions) raison = AppConfig.FieldNamePublishStartDate;
            }
            if (permissions && IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey(AppConfig.FieldNamePublishEndDate))
            {
                permissions =   dsItem.Data[AppConfig.FieldNamePublishEndDate] != null &&
                                dsItem.Data[AppConfig.FieldNamePublishEndDate].Type == JTokenType.Date &&
                                ((DateTime)dsItem.Data[AppConfig.FieldNamePublishEndDate])  >= DateTime.Today;
                if (!permissions) raison = AppConfig.FieldNamePublishEndDate;
            }
            if (permissions)
            {
                // Roles                
                string fieldName = "";
                if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("userrole"))
                {
                    fieldName = "userrole";
                }
                else if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.ContainsKey("userroles"))
                {
                    fieldName = "userroles";
                }
                if (!string.IsNullOrEmpty(fieldName))
                {
                    permissions = false;
                    string[] dataRoles = null;
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
                        var roles = userInfo.Social.Roles;
                        if (roles.Any())
                        {
                            permissions = roles.Any(r => dataRoles.Contains(r.RoleID.ToString()));
                        }
                        else
                        {
                            permissions = dataRoles.Contains("Unauthenticated");
                        }
                    }
                    if (!permissions) raison = fieldName;
                }
            }
            return permissions;

        }
    }
}