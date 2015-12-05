using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DotNetNuke.Services.Installer.Log;
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
                Log.Logger.ErrorFormat("Failed to load manifest from folder {0}. Error:{1}", folder.UrlFolder, ex.ToString());
                throw ex;
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
            string content = @"
                                    {
                                        ""editWitoutPostback"": false,
                                        ""templates"": {
                                            ""{{templatekey}}"": {
                                                ""type"": ""single"", /* single or multiple*/
                                                ""title"": ""{{templatekey}}"",
                                                ""main"": {
                                                    ""template"": ""{{templatekey}}{{templateextention}}"",
                                                    ""schemaInTemplate"": true,
                                                    ""optionsInTemplate"": true,
                                                    ""clientSideData"": false
                                                }
                                            }
                                        }
                                    }
                                ";

            Manifest manifest = null;
            content = content.Replace("{{templatekey}}", templeteKey.Key);
            content = content.Replace("{{templateextention}}", templeteKey.Extention);
            manifest = JsonConvert.DeserializeObject<Manifest>(content);
            return manifest;
        }

        #endregion

        internal static bool SettingsNeeded(this FileUri template)
        {
            var schemaFileUri = new FileUri(template.FolderPath, "schema.json");
            return schemaFileUri.FileExists;
        }
    }
}