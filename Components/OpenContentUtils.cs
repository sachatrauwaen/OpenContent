using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using DotNetNuke.UI.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Lucene.Config;


namespace Satrabel.OpenContent.Components
{
    public static class OpenContentUtils
    {
        public static void UpdateModuleTitle(ModuleInfo Module, string ModuleTitle)
        {
            if (Module.ModuleTitle != ModuleTitle)
            {
                ModuleController mc = new ModuleController();
                var mod = mc.GetModule(Module.ModuleID, Module.TabID, true);
                mod.ModuleTitle = ModuleTitle;
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
            return GetTemplatesFiles(portalSettings, moduleId, selectedTemplate.ToTemplateManifest(), moduleSubDir);
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
                                var item = new ListItem(templateCat + " : " + templateName, templateUri.FilePath);
                                if (selectedTemplate != null && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.FullKeyString().ToLowerInvariant())
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

                        var item = new ListItem(templateCat + " : " + scriptName, templateUri.FilePath);
                        if (selectedTemplate != null && templateUri.FilePath.ToLowerInvariant() == selectedTemplate.Key.FullKeyString().ToLowerInvariant())
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
                        if (selectedTemplate != null && scriptPath.ToLowerInvariant() == selectedTemplate.Key.FullKeyString().ToLowerInvariant())
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
                if (selectedTemplate != null && scriptPath.ToLowerInvariant() == selectedTemplate.Key.FullKeyString().ToLowerInvariant())
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
                    if (selectedTemplate != null && scriptPath.ToLowerInvariant() == selectedTemplate.Key.FullKeyString().ToLowerInvariant())
                    {
                        item.Selected = true;
                    }
                    lst.Add(item);
                }
            }
            return lst;
        }

        public static string CopyTemplate(int PortalId, string FromFolder, string NewTemplateName)
        {
            string FolderName = "OpenContent/Templates/" + NewTemplateName;
            var folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
            if (folder != null)
            {
                throw new Exception("Template already exist " + folder.FolderPath);
            }
            folder = FolderManager.Instance.AddFolder(PortalId, FolderName);
            foreach (var item in Directory.GetFiles(FromFolder))
            {
                File.Copy(item, folder.PhysicalPath + Path.GetFileName(item));

            }
            return GetDefaultTemplate(folder.PhysicalPath);
        }

        public static string ImportFromWeb(int PortalId, string FileName, string NewTemplateName)
        {
            //string FileName = ddlWebTemplates.SelectedValue;
            string strMessage = "";
            try
            {
                var folder = FolderManager.Instance.GetFolder(PortalId, "OpenContent/Templates");
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(PortalId, "OpenContent/Templates");
                }
                var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
                if (Path.GetExtension(FileName) == ".zip")
                {
                    if (string.IsNullOrEmpty(NewTemplateName))
                    {
                        NewTemplateName = Path.GetFileNameWithoutExtension(FileName);
                    }
                    string FolderName = "OpenContent/Templates/" + NewTemplateName;
                    folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                    if (folder != null)
                    {
                        throw new Exception("Template already exist " + folder.FolderName);
                    }
                    folder = FolderManager.Instance.AddFolder(PortalId, FolderName);
                    var req = (HttpWebRequest)WebRequest.Create(FileName);
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
                strMessage = string.Format(Localization.GetString("DiskSpaceExceeded"), FileName);
            }
            catch (InvalidFileExtensionException)
            {
                //Logger.Warn(exc);
                strMessage = string.Format(Localization.GetString("RestrictedFileType"), FileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception exc)
            {
                //Logger.Error(exc);
                strMessage = string.Format(Localization.GetString("SaveFileError") + " - " + exc.Message, FileName);
            }
            if (!string.IsNullOrEmpty(strMessage))
            {
                throw new Exception(strMessage);
            }
            return "";
        }

        public static string GetDefaultTemplate(string physicalFolder)
        {
            string Template = "";
            FolderUri folder = new FolderUri(FolderUri.ReverseMapPath(physicalFolder));
            var manifest = ManifestUtils.GetFileManifest(folder);
            if (manifest != null && manifest.HasTemplates)
            {
                //get the requested template key
                //var templateManifest = manifest.Templates.First().Value;
                //var templateUri = new FileUri(folder, templateManifest.Main.Template);
                Template = folder.FolderPath + "/" + manifest.Templates.First().Key;
            }
            else
            {
                foreach (var item in Directory.GetFiles(physicalFolder))
                {
                    string FileName = Path.GetFileName(item).ToLower();
                    if (FileName == "template.hbs")
                    {
                        Template = item;
                        break;
                    }
                    else if (FileName == "template.cshtml")
                    {
                        Template = item;
                        break;
                    }
                    if (FileName.EndsWith(".hbs"))
                    {
                        Template = item;
                    }
                    if (FileName.EndsWith(".cshtml"))
                    {
                        Template = item;
                    }
                }
            }
            return FileUri.ReverseMapPath(Template);
        }
        /*
        public static bool IsListTemplate(string Template)
        {
            return template.IsDefined() && Template.EndsWith("$.hbs");
        }
        */
        public static string CleanupUrl(string Url)
        {
            string replaceWith = "-";

            string AccentFrom = "ÀÁÂÃÄÅàáâãäåảạăắằẳẵặấầẩẫậÒÓÔÕÖØòóôõöøỏõọồốổỗộơớờởợÈÉÊËèéêëẻẽẹếềểễệÌÍÎÏìíîïỉĩịÙÚÛÜùúûüủũụưứừửữựÿýỳỷỹỵÑñÇçĞğİıŞş₤€ßđ";
            string AccentTo = "AAAAAAaaaaaaaaaaaaaaaaaaaOOOOOOoooooooooooooooooooEEEEeeeeeeeeeeeeIIIIiiiiiiiUUUUuuuuuuuuuuuuuyyyyyyNnCcGgIiSsLEsd";

            Url = Url.ToLower().Trim();

            StringBuilder result = new StringBuilder(Url.Length);
            string ch = ""; int i = 0; int last = Url.ToCharArray().GetUpperBound(0);
            foreach (char c in Url.ToCharArray())
            {

                //use string for manipulation
                ch = c.ToString();
                if (ch == " ")
                {
                    ch = replaceWith;
                }
                else if (@".[]|:;`%\\""".Contains(ch))
                    ch = "";
                else if (@" &$+,/=?@~#<>()¿¡«»!'’–*…".Contains(ch))
                    ch = replaceWith;
                else
                {
                    for (int ii = 0; ii < AccentFrom.Length; ii++)
                    {
                        if (ch == AccentFrom[ii].ToString())
                        {
                            ch = AccentTo[ii].ToString();
                        }
                    }
                }

                if (i == last)
                {
                    if (!(ch == "-" || ch == replaceWith))
                    {   //only append if not the same as the replacement character
                        result.Append(ch);
                    }
                }
                else
                    result.Append(ch);
                i++;//increment counter
            }
            result = result.Replace(replaceWith + replaceWith, replaceWith);
            result = result.Replace(replaceWith + replaceWith, replaceWith);

            // remove ending -
            while (result.Length > 1 && result[result.Length - 1] == replaceWith[0])
            {
                result.Remove(result.Length - 1, 1);
            }
            // remove starting -
            while (result.Length > 1 && result[0] == replaceWith[0])
            {
                result.Remove(0, 1);
            }

            return result.ToString();
        }


        public static string ReverseMapPath(string path)
        {
            return FileUri.ReverseMapPath(path);
        }

        public static bool HasEditPermissions(PortalSettings portalSettings, ModuleInfo module, string editrole, int CreatedByUserId)
        {
            return portalSettings.UserInfo.IsSuperUser ||
                    portalSettings.UserInfo.IsInRole(portalSettings.AdministratorRoleName) ||
                    ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", module) ||
                    (!string.IsNullOrEmpty(editrole) && portalSettings.UserInfo.IsInRole(editrole) && (CreatedByUserId == -1 || portalSettings.UserId == CreatedByUserId)) ||
                    (!string.IsNullOrEmpty(editrole) &&  editrole.ToLower() == "all");
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
                if (Debugger.IsAttached) Debugger.Break();
                return null;
            }
        }
    }
}