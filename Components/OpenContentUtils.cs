using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace Satrabel.OpenContent.Components
{
    class OpenContentUtils
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
        public static string GetSiteTemplateFolder(PortalSettings portalSettings)
        {
            return portalSettings.HomeDirectory + "/OpenContent/Templates/";
        }
        public static List<ListItem> GetTemplatesFiles(PortalSettings portalSettings, int ModuleId, string SelectedTemplate)
        {
            string basePath = HostingEnvironment.MapPath(GetSiteTemplateFolder(portalSettings));
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
                var files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
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
            return lst;
        }
        public static List<ListItem> GetTemplates(PortalSettings portalSettings, int ModuleId, string SelectedTemplate)
        {
            string basePath = HostingEnvironment.MapPath(GetSiteTemplateFolder(portalSettings));
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
            return lst;
        }

        public static string ReverseMapPath(string path)
        {
            string appPath = HostingEnvironment.MapPath("~");
            string res = string.Format("{0}", path.Replace(appPath, "").Replace("\\", "/"));
            if (!res.StartsWith("/")) res = "/" + res;
            return res;
        }
    }

}