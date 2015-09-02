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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using DotNetNuke.UI.Modules;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentUtils
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
        public static string GetTemplateFolder(Hashtable moduleSettings)
        {
            return HttpContext.Current.Request.ApplicationPath + moduleSettings["template"] as string;
        }
        public static string SetTemplateFolder(string templateFullpath)
        {
            if (string.IsNullOrEmpty(HttpContext.Current.Request.ApplicationPath)) return templateFullpath;
            return templateFullpath.Remove(0, HttpContext.Current.Request.ApplicationPath.Length);
        }
        public static List<ListItem> GetTemplatesFiles(PortalSettings portalSettings, int ModuleId, string SelectedTemplate, string moduleSubDir)
        {
            string basePath = HostingEnvironment.MapPath(GetSiteTemplateFolder(portalSettings, moduleSubDir));
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            List<ListItem> lst = new List<ListItem>();
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                string TemplateCat = "Site";
                string DirName = Path.GetFileNameWithoutExtension(dir);
                int ModId = -1;
                if (int.TryParse(DirName, out ModId))
                {
                    if (ModId == ModuleId)
                    {
                        TemplateCat = "Module";
                    }
                    else
                    {
                        continue;
                    }
                }
                IEnumerable<string> files = null;
                string templateVirtualFolder = ReverseMapPath(dir);
                var manifest = GetManifest(templateVirtualFolder);
                if (manifest != null && manifest.Templates != null)
                {
                    files = manifest.Templates.Select(t => t.Key);
                    foreach (var template in manifest.Templates)
                    {
                        string templateName = DirName;
                        if (!string.IsNullOrEmpty(template.Value.Title))
                        {
                            templateName = templateName + " - " + template.Value.Title;
                        }
                        string scriptPath = templateVirtualFolder + "/" + template.Key;
                        var item = new ListItem(TemplateCat + " : " + templateName, scriptPath);
                        if (!(string.IsNullOrEmpty(SelectedTemplate)) && scriptPath.ToLowerInvariant() == SelectedTemplate.ToLowerInvariant())
                        {
                            item.Selected = true;
                        }
                        lst.Add(item);
                    }
                }
                else
                {
                    files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.EndsWith(".cshtml") || s.EndsWith(".vbhtml") || s.EndsWith(".hbs"));
                    foreach (string script in files)
                    {
                        string scriptName = script.Remove(script.LastIndexOf(".")).Replace(basePath, "");
                        if (TemplateCat == "Module")
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

                        string scriptPath = ReverseMapPath(script);
                        var item = new ListItem(TemplateCat + " : " + scriptName, scriptPath);
                        if (!(string.IsNullOrEmpty(SelectedTemplate)) && scriptPath.ToLowerInvariant() == SelectedTemplate.ToLowerInvariant())
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
                    string TemplateCat = "Skin";
                    string DirName = Path.GetFileNameWithoutExtension(dir);
                    var files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                                .Where(s => s.EndsWith(".cshtml") || s.EndsWith(".vbhtml") || s.EndsWith(".hbs"));
                    files = files.Where(f => f.EndsWith("$.hbs") || !f.Contains("$"));
                    foreach (string script in files)
                    {
                        string scriptName = script.Remove(script.LastIndexOf(".")).Replace(basePath, "");
                        scriptName = scriptName.Replace("$", "");
                        if (scriptName.ToLower().EndsWith("template"))
                            scriptName = scriptName.Remove(scriptName.LastIndexOf("\\"));
                        else
                            scriptName = scriptName.Replace("\\", " - ");

                        string scriptPath = ReverseMapPath(script);
                        var item = new ListItem(TemplateCat + " : " + scriptName, scriptPath);
                        if (!(string.IsNullOrEmpty(SelectedTemplate)) && scriptPath.ToLowerInvariant() == SelectedTemplate.ToLowerInvariant())
                        {
                            item.Selected = true;
                        }
                        lst.Add(item);
                    }
                }
            }
            return lst;
        }
        public static List<ListItem> GetTemplates(PortalSettings portalSettings, int ModuleId, string SelectedTemplate, string moduleSubDir)
        {
            string basePath = HostingEnvironment.MapPath(GetSiteTemplateFolder(portalSettings, moduleSubDir));
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            List<ListItem> lst = new List<ListItem>();
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                string TemplateCat = "Site";
                string DirName = Path.GetFileNameWithoutExtension(dir);
                int ModId = -1;
                if (int.TryParse(DirName, out ModId))
                {
                    if (ModId == ModuleId)
                    {
                        TemplateCat = "Module";
                    }
                    else
                    {
                        continue;
                    }
                }
                string scriptName = dir;
                if (TemplateCat == "Module")
                    scriptName = TemplateCat;
                else
                    scriptName = TemplateCat + ":" + scriptName.Substring(scriptName.LastIndexOf("\\") + 1);

                string scriptPath = ReverseMapPath(dir);
                var item = new ListItem(scriptName, scriptPath);
                if (!(string.IsNullOrEmpty(SelectedTemplate)) && scriptPath.ToLowerInvariant() == SelectedTemplate.ToLowerInvariant())
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
                    string TemplateCat = "Skin";
                    string DirName = Path.GetFileNameWithoutExtension(dir);
                    string scriptName = dir;
                    scriptName = TemplateCat + ":" + scriptName.Substring(scriptName.LastIndexOf("\\") + 1);
                    string scriptPath = ReverseMapPath(dir);
                    var item = new ListItem(scriptName, scriptPath);
                    if (!(string.IsNullOrEmpty(SelectedTemplate)) && scriptPath.ToLowerInvariant() == SelectedTemplate.ToLowerInvariant())
                    {
                        item.Selected = true;
                    }
                    lst.Add(item);
                }
            }
            return lst;
        }

        public static string ReverseMapPath(string path)
        {
            string appPath = HostingEnvironment.MapPath("~");
            string res = string.Format("{0}", path.Replace(appPath, "").Replace("\\", "/"));
            if (!res.StartsWith("/")) res = "/" + res;
            return HttpContext.Current.Request.ApplicationPath + res;
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
            return GetDefaultTemlate(folder.PhysicalPath);
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
                    return GetDefaultTemlate(folder.PhysicalPath);
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

        public static string GetDefaultTemlate(string FromFolder)
        {
            string Template = "";
            foreach (var item in Directory.GetFiles(FromFolder))
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
            return ReverseMapPath(Template);
        }
        /*
        public static bool IsListTemplate(string Template)
        {
            return !string.IsNullOrEmpty(Template) && Template.EndsWith("$.hbs");
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
                else if (@" &$+,/=?@~#<>()¿¡«»!'’–".Contains(ch))
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

        public static Manifest GetManifest(string folder)
        {
            Manifest manifest = null;
            string filename = HostingEnvironment.MapPath(folder + "/manifest.json");
            if (File.Exists(filename))
            {
                string content = File.ReadAllText(filename);
                manifest = JsonConvert.DeserializeObject<Manifest>(content);
            }
            return manifest;
        }
        public static TemplateManifest GetTemplateManifest(string Template)
        {
            TemplateManifest templateManifest = null;
            if (!string.IsNullOrEmpty(Template))
            {
                string TemplateFolder = Path.GetDirectoryName(Template);
                string TemplateFile = Path.GetFileNameWithoutExtension(Template);
                Manifest manifest = GetManifest(TemplateFolder);
                if (manifest != null && manifest.Templates != null && manifest.Templates.ContainsKey(TemplateFile))
                {
                    templateManifest = manifest.Templates[TemplateFile];
                }
            }
            return templateManifest;
        }



    }

    public class TemplateManifest
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "main")]
        public TemplateFiles Main { get; set; }
        [JsonProperty(PropertyName = "detail")]
        public TemplateFiles Detail { get; set; }

        public bool IsListTemplate
        {
            get
            {
                return Type == "multiple";
            }
        }
    }
    public class PartialTemplate
    {
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }
    }
    public class TemplateFiles
    {
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }

        [JsonProperty(PropertyName = "partialTemplates")]
        public Dictionary<string, PartialTemplate> PartialTemplates { get; set; }

        [JsonProperty(PropertyName = "schemaInTemplate")]
        public bool SchemaInTemplate { get; set; }

        [JsonProperty(PropertyName = "optionsInTemplate")]
        public bool OptionsInTemplate { get; set; }
    }

    public class Manifest
    {
        [JsonProperty(PropertyName = "editWitoutPostback")]
        public bool EditWitoutPostback { get; set; }
        [JsonProperty(PropertyName = "templates")]
        public Dictionary<string, TemplateManifest> Templates { get; set; }
        [JsonProperty(PropertyName = "additionalEditControl")]
        public string AdditionalEditControl { get; set; }
    }

}