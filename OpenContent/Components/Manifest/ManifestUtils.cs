using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public static class ManifestUtils
    {
        #region Manifest Factory

        internal static Manifest GetManifest(TemplateKey templateKey)
        {
            TemplateManifest templateManifest;
            var manifest = GetManifest(templateKey, out templateManifest);
            return manifest;
        }

        internal static Manifest GetManifest(TemplateKey templateKey, out TemplateManifest templateManifest)
        {
            templateManifest = null;
            if (templateKey == null) return null;

            Manifest manifest;
            if (templateKey.Extention == "manifest")
            {
                manifest = LoadManifestFileFromCacheOrDisk(templateKey.TemplateDir);
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
                    keyValuePair.Value.SetSource(new TemplateKey(templateKey, keyValuePair.Key));
                }
                //get the requested template by Key
                templateManifest = manifest.GetTemplateManifest(templateKey);
            }
            return manifest;
        }

        internal static Manifest LoadManifestFileFromCacheOrDisk(FolderUri folder)
        {
            try
            {
                var file = new FileUri(folder.UrlFolder, "manifest.json");
                var manifest = App.Services.FileRepository.LoadDeserializedJsonFileFromCacheOrDisk<Manifest>(file);
                return manifest;
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Failed to load manifest from folder {folder.UrlFolder}. Error: {ex}");
                throw;
            }
        }

        internal static TemplateManifest ToTemplateManifest(this FileUri templateUri)
        {
            TemplateManifest templateManifest;
            GetManifest(new TemplateKey(templateUri), out templateManifest);
            return templateManifest;
        }

        internal static FileUri MainTemplateUri(this TemplateManifest templateUri)
        {
            return templateUri?.Main == null ? null : new FileUri(templateUri.ManifestFolderUri, templateUri.Main.Template);
        }

        private static Manifest GetVirtualManifest(TemplateKey templeteKey)
        {
            /*
            string content = @"
                                    {
                                        ""editWitoutPostback"": false,
                                        ""templates"": {
                                            ""{{templatekey}}"": {
                                                ""type"": ""single"",
                                                ""title"": ""{{templatekey}}"",
                                                ""main"": {
                                                    ""template"": ""{{templatekey}}{{templateextention}}"",
                                                    ""schemaInTemplate"": false,
                                                    ""optionsInTemplate"": false,
                                                    ""clientSideData"": false
                                                }
                                            }
                                        }
                                    }
                                ";

            content = content.Replace("{{templatekey}}", templeteKey.ShortKey);
            content = content.Replace("{{templateextention}}", templeteKey.Extention);
            var manifest = JsonConvert.DeserializeObject<Manifest>(content);
            */
            var manifest = new Manifest();
            manifest.EditWitoutPostback = false;
            manifest.Templates = new Dictionary<string, TemplateManifest>();
            manifest.Templates.Add(templeteKey.ShortKey, new TemplateManifest()
            {
                Type = "single",
                Title = templeteKey.ShortKey,
                ClientSideData = false,
                Main = new TemplateFiles()
                {
                    Template = templeteKey.ShortKey + templeteKey.Extention,
                    SchemaInTemplate = false,
                    OptionsInTemplate = false
                }
            });
            return manifest;
        }

        #endregion

        internal static bool SettingsNeeded(this TemplateManifest template)
        {
            var schemaFileUri = new FileUri(template.ManifestFolderUri.UrlFolder, template.Key.ShortKey + "-schema.json");
            if (schemaFileUri.FileExists)
                return true;

            return false;
        }
        internal static bool DataNeeded(this TemplateManifest template)
        {
            var schemaFileUri = new FileUri(template.ManifestFolderUri.UrlFolder, "schema.json");
            if (schemaFileUri.FileExists)
                return true;

            return false;
        }
        internal static bool QueryAvailable(this TemplateManifest template)
        {
            var schemaFileUri = new FileUri(template.ManifestFolderUri.UrlFolder, "query-schema.json");
            if (schemaFileUri.FileExists)
                return true;

            return false;
        }

        internal static string GetEditRole(this Manifest manifest)
        {
            return manifest == null ? "" : manifest.EditRole;
        }
    }
}