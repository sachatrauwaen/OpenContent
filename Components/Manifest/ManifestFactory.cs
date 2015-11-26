using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components
{
    public static class ManifestFactory
    {
        #region Manifest Factory

        internal static Manifest GetManifest(TemplateKey templateKey, out TemplateManifest templateManifest)
        {
            templateManifest = null;
            if (templateKey == null) return null;

            var manifest = templateKey.Extention == "manifest" ? GetManifest(templateKey.TemplateDir) : GetManifest(templateKey);

            if (manifest != null && manifest.HasTemplates)
            {
                //store the Folder into the Manifest object and its templates
                manifest.ManifestDir = templateKey.TemplateDir;
                foreach (KeyValuePair<string, TemplateManifest> keyValuePair in manifest.Templates)
                {
                    keyValuePair.Value.ManifestDir = templateKey.TemplateDir;
                }
                //get the requested template key
                templateManifest = manifest.GetTemplateManifest(templateKey);
            }
            return manifest;
        }

        //private static Manifest GetManifest(OpenContentSettings settings, out TemplateManifest templateManifest)
        //{
        //    templateManifest = null;
        //    Manifest manifest = null;
        //    if (!settings.TemplateAvailable)
        //    {
        //        templateManifest = null;
        //        return null;
        //    }

        //    //get the manifest or create one from the template file
        //    manifest = GetManifest(settings.TemplateDir) ?? GetManifest(settings.Template);

        //    if (manifest != null && manifest.HasTemplates)
        //    {
        //        //store the Folder into the Manifest object and its templates
        //        manifest.ManifestDir = settings.TemplateDir;
        //        foreach (KeyValuePair<string, TemplateManifest> keyValuePair in manifest.Templates)
        //        {
        //            keyValuePair.Value.ManifestDir = settings.TemplateDir;
        //        }
        //        //get the requested template key
        //        templateManifest = manifest.GetTemplateManifest(settings.TemplateKey);
        //    }
        //    return manifest;
        //}

        internal static Manifest GetManifest(FolderUri folder)
        {
            try
            {
                Manifest manifest = null;
                var file = new FileUri(folder.UrlFolder, "manifest.json");
                if (file.FileExists)
                {
                    string content = File.ReadAllText(file.PhysicalFilePath);
                    manifest = JsonConvert.DeserializeObject<Manifest>(content);
                }
                return manifest;
            }
            catch (Exception ex)
            {
                //we should log this
                if (Debugger.IsAttached) Debugger.Break();
                return null;
            }
        }

        private static Manifest GetManifest(TemplateKey templeteKey)
        {
            try
            {
                Manifest manifest = null;
                var file = new FileUri("DesktopModules/OpenContent/Templates", "default-manifest.json");
                if (file.FileExists)
                {
                    string content = File.ReadAllText(file.PhysicalFilePath);
                    content = content.Replace("{{templatekey}}", templeteKey.Key);
                    content = content.Replace("{{templateextention}}", templeteKey.Extention);
                    manifest = JsonConvert.DeserializeObject<Manifest>(content);
                }
                return manifest;
            }
            catch (Exception ex)
            {
                //we should log this
                if (Debugger.IsAttached) Debugger.Break();
                return null;
            }
        }
        //private static TemplateManifest GetTemplateManifest(FileUri template)  //todo - dit bestaat ook als methode van een classe
        //{
        //    if (template == null)
        //    {
        //        throw new ArgumentNullException("template is null");
        //    }
        //    TemplateManifest templateManifest = null;
        //    Manifest manifest = GetManifest(template);
        //    if (manifest != null)
        //    {
        //        templateManifest = manifest.GetTemplateManifest(template);
        //    }
        //    return templateManifest;
        //}

        #endregion
    }
}