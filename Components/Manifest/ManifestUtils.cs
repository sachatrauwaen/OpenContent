using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public static class ManifestUtils
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

        internal static TemplateManifest ToTemplateManifest(this FileUri templateUri)
        {
            TemplateManifest templateManifest;
            ManifestUtils.GetManifest(new TemplateKey(templateUri), out templateManifest);
            return templateManifest;
        }


        internal static FileUri Uri(this TemplateManifest templateUri)
        {
            if (templateUri == null) return null;
            return new FileUri(templateUri.ManifestDir, templateUri.Main.Template);
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

        #endregion

        internal static bool SettingsNeeded(this FileUri template)
        {
            var schemaFileUri = new FileUri(template.FolderPath + "schema.json");
            return schemaFileUri.FileExists;
        }
    }
}