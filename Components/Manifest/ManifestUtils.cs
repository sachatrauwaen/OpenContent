using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DotNetNuke.Services.Installer.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public static class ManifestUtils
    {
        #region Manifest Factory

        internal static Manifest GetManifest(TemplateKey templateKey)
        {
            TemplateManifest templateManifest;
            var manifest = GetManifest(templateKey, out  templateManifest);
            return manifest;
        }
        internal static Manifest GetManifest(TemplateKey templateKey, out TemplateManifest templateManifest)
        {
            templateManifest = null;
            if (templateKey == null) return null;

            Manifest manifest;
            if (templateKey.Extention == "manifest")
            {
                manifest = GetFileManifest(templateKey.TemplateDir);
                //todo downgrade template directories that stop using manifests
            }
            else
            {
                //todo upgrade template directories that start using manifests
                //manifest = GetFileManifest(templateKey.TemplateDir);
                //if (manifest == null)
                //    manifest = GetVirtualManifest(templateKey);
                //else if (manifest.Templates == null)
                    manifest = GetVirtualManifest(templateKey);
            }

            if (manifest != null && manifest.HasTemplates)
            {
                //store some info into the Manifest object and its templates for backlink reference
                manifest.ManifestDir = templateKey.TemplateDir;
                foreach (KeyValuePair<string, TemplateManifest> keyValuePair in manifest.Templates)
                {
                    keyValuePair.Value.SetSource(templateKey);
                }
                //get the requested template by Key
                templateManifest = manifest.GetTemplateManifest(templateKey);
            }
            return manifest;
        }

        internal static Manifest GetFileManifest(FolderUri folder)
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
            GetManifest(new TemplateKey(templateUri), out templateManifest);
            return templateManifest;
        }


        internal static FileUri Uri(this TemplateManifest templateUri)
        {
            return templateUri == null ? null : new FileUri(templateUri.ManifestDir, templateUri.Main.Template);
        }

        private static Manifest GetVirtualManifest(TemplateKey templeteKey)
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

            content = content.Replace("{{templatekey}}", templeteKey.ShortKey);
            content = content.Replace("{{templateextention}}", templeteKey.Extention);
            var manifest = JsonConvert.DeserializeObject<Manifest>(content);
            return manifest;
        }

        #endregion

        internal static bool SettingsNeeded(this TemplateManifest template)
        {
            var schemaFileUri = new FileUri(template.Uri().UrlFolder, template.Key.ShortKey + "-schema.json");
            if (schemaFileUri.FileExists)
                return true;

            return false;
        }
        internal static bool QueryAvailable(this TemplateManifest template)
        {
            var schemaFileUri = new FileUri(template.Uri().UrlFolder, "query-schema.json");
            if (schemaFileUri.FileExists)
                return true;

            return false;
        }
    }
}