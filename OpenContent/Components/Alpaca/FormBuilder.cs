using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene.Config;
using Newtonsoft.Json;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Cache;

namespace Satrabel.OpenContent.Components.Alpaca
{
    public class FormBuilder
    {
        private readonly FolderUri _templateUri;
        public FormBuilder(FolderUri templateUri)
        {
            this._templateUri = templateUri;
        }
        public JObject BuildQuerySettings(string collection)
        {
            var indexConfig = OpenContentUtils.GetIndexConfig(_templateUri, collection);
            var jsonEdit = Build(collection, DnnLanguageUtils.GetCurrentCultureCode(), true, true, false, false);
            SchemaConfig newSchema = new SchemaConfig(true);
            // max results
            newSchema.Properties.Add("MaxResults", new SchemaConfig()
            {
                Title = "MaxResults",
                Type = "number"
            });
            // Default no results
            newSchema.Properties.Add("DefaultNoResults", new SchemaConfig()
            {
                Title = "Default No Results",
                Type = "boolean"
            });
            // Remove current item
            newSchema.Properties.Add("ExcludeCurrentItem", new SchemaConfig()
            {
                Title = "Exclude Current Item",
                Type = "boolean"
            });
            // Show only Current User Items
            newSchema.Properties.Add("CurrentUserItems", new SchemaConfig()
            {
                Title = "Only Current User Items",
                Type = "boolean"
            });
            // Filter
            SchemaConfig newSchemaFilter = new SchemaConfig(true)
            {
                Title = "Filter"
            };
            newSchema.Properties.Add("Filter", newSchemaFilter);
            OptionsConfig newOptions = new OptionsConfig(true);
            OptionsConfig newOptionsFilter = new OptionsConfig(true);
            newOptions.Fields.Add("Filter", newOptionsFilter);

            SchemaConfig schemaConfig = new SchemaConfig();
            var schemaJson = jsonEdit["schema"];
            if (schemaJson != null)
            {
                schemaConfig = schemaJson.ToObject<SchemaConfig>();
            }
            OptionsConfig optionsConfig = new OptionsConfig();
            JToken optionsJson = jsonEdit["options"];
            if (optionsJson != null)
            {
                optionsConfig = optionsJson.ToObject<OptionsConfig>();
            }
            List<string> fieldLst = new List<string>();
            GetFields(newSchemaFilter, newOptionsFilter, schemaConfig, optionsConfig, fieldLst, indexConfig);
            // Sort
            SchemaConfig newSchemaSort = new SchemaConfig()
            {
                Type = "array",
                Title = "Sort"
            };
            newSchema.Properties.Add("Sort", newSchemaSort);

            newSchemaSort.Type = "array";
            newSchemaSort.Items = new SchemaConfig(true);
            newSchemaSort.Items.Properties.Add("Field", new SchemaConfig()
            {
                Title = "Field",
                Enum = fieldLst
            });
            newSchemaSort.Items.Properties.Add("Order", new SchemaConfig()
            {
                Title = "Order",
                Enum = new List<string>(new string[] { "asc", "desc" })
            });

            OptionsConfig newOptionsSort = new OptionsConfig();
            newOptions.Fields.Add("Sort", newOptionsSort);
            newOptionsSort.Type = "table";
            newOptionsSort.Items = new OptionsConfig(true);
            newOptionsSort.Items.Fields.Add("Field", new OptionsConfig()
            {
                Type = "select",
                RemoveDefaultNone = true
            });
            newOptionsSort.Items.Fields.Add("Order", new OptionsConfig()
            {
                Type = "select",
                RemoveDefaultNone = true
            });

            var json = new JObject();
            json["schema"] = JObject.FromObject(newSchema);
            json["options"] = JObject.FromObject(newOptions);

            //File.WriteAllText(TemplateUri.PhysicalFullDirectory + "\\test.json", json.ToString());
            return json;
        }

        private static void GetFields(SchemaConfig newSchemaFilter, OptionsConfig newOptionsFilter, SchemaConfig schemaConfig, OptionsConfig optionsConfig, List<string> fieldLst, FieldConfig indexConfig, string prefix = "")
        {
            foreach (var prop in schemaConfig.Properties)
            {
                string propKey = prefix + prop.Key;
                string propTitle = prefix + prop.Value.Title;
                var opts = optionsConfig?.Fields != null && optionsConfig.Fields.ContainsKey(prop.Key) ? optionsConfig.Fields[prop.Key] : null;
                var idxs = indexConfig?.Fields != null && indexConfig.Fields.ContainsKey(prop.Key) ? indexConfig.Fields[prop.Key] : null;
                if (prop.Key == AppConfig.FieldNamePublishStatus || prop.Key == AppConfig.FieldNamePublishStartDate || prop.Key == AppConfig.FieldNamePublishEndDate)
                {
                    fieldLst.Add(propKey);
                    continue;
                }
                if (prop.Value.Type == "object" && idxs != null)
                {
                    GetFields(newSchemaFilter, newOptionsFilter, prop.Value, opts, fieldLst, idxs, propKey + ".");
                    continue;
                }
                string optType = opts == null ? "text" : opts.Type ?? "text";

                if (prop.Value.Type == "boolean")
                {
                    var newProp = new SchemaConfig()
                    {
                        //Type = prop.Value.Type,
                        Title = prop.Value.Title,
                        Enum = new List<string>(new[] { "true", "false" })
                    };
                    newSchemaFilter.Properties.Add(propKey, newProp);

                    var newField = new OptionsConfig();
                    newOptionsFilter.Fields.Add(propKey, newField);
                    newField.Type = "select";

                    fieldLst.Add(propKey);
                }
                else if (prop.Value.Type == "number")
                {
                    var newProp = new SchemaConfig()
                    {
                        Type = prop.Value.Type,
                        Title = prop.Value.Title
                    };
                    newSchemaFilter.Properties.Add(propKey, newProp);

                    var newField = new OptionsConfig();
                    newOptionsFilter.Fields.Add(propKey, newField);

                    fieldLst.Add(propKey);
                }
                else if (optType == "text" || optType == "mltext" || optType == "checkbox" || optType == "select" || optType == "select2")
                {
                    var newProp = new SchemaConfig()
                    {
                        Type = prop.Value.Type,
                        Title = propTitle,
                        Enum = prop.Value.Enum
                    };
                    newSchemaFilter.Properties.Add(propKey, newProp);

                    var newField = new OptionsConfig();
                    newOptionsFilter.Fields.Add(propKey, newField);
                    if (prop.Value.Enum != null)
                    {
                        newProp.Type = "array";
                        newField.Type = "checkbox";
                    }
                    if (optType == "select2")
                    {
                        newProp.Type = "array";
                        newField.Type = "select2";
                        newField.DataService = opts == null ? null : opts.DataService;
                    }
                    fieldLst.Add(propKey);
                }
                else if (optType == "date" || optType == "datetime" || optType == "time")
                {
                    var newProp = new SchemaConfig(true);
                    newSchemaFilter.Properties.Add(propKey, newProp);
                    newProp.Properties.Add("StartDays", new SchemaConfig()
                    {
                        Type = "number",
                        Title = propTitle + " : from x days in the past"
                    });
                    newProp.Properties.Add("EndDays", new SchemaConfig()
                    {
                        Type = "number",
                        Title = propTitle + " : until x days in the future"
                    });
                    fieldLst.Add(propKey);
                    newProp.Properties.Add("UseTime", new SchemaConfig()
                    {
                        Type = "boolean",
                        Title = propTitle + " Concider time"
                    });

                    /*
                    var newField = new OptionsConfig();
                    newOptionsFilter.Fields.Add(propKey, newField);
                    newField.Helper = "Use 0 for today";
                    */
                }
            }
        }

        //public JObject BuildForm()
        //{
        //    return BuildForm("", DnnLanguageUtils.GetCurrentCultureCode());
        //}

        public JObject BuildForm(string key, string currentCultureCode, bool schema = true, bool options = true, bool view = true)
        {
            return Build(key, currentCultureCode, schema, options, view, true);
        }

        public JObject BuildForm(string key)
        {
            return Build(key, DnnLanguageUtils.GetCurrentCultureCode(), true, true, true, true);
        }

        private JObject Build()
        {
            return Build("", DnnLanguageUtils.GetCurrentCultureCode(), true, true, true, true);
        }

        private JObject Build(string key, string currentCultureCode, bool schema, bool options, bool view, bool translations)
        {
            string prefix = (string.IsNullOrEmpty(key) || key == "Items") ? "" : key + "-";

            JObject json = new JObject();
            // schema
            if (schema)
            {
                var schemaJson = JsonUtils.LoadJsonFromFile(_templateUri.UrlFolder + prefix + "schema.json");
                if (schemaJson != null)
                    json["schema"] = schemaJson;
            }
            // default options
            if (options)
            {
                var optionsJson = JsonUtils.LoadJsonFromFile(_templateUri.UrlFolder + prefix + "options.json");
                if (optionsJson != null)
                {
                    json["options"] = optionsJson;
                    if (translations)
                    {
                        // language options
                        optionsJson = JsonUtils.LoadJsonFromFile(_templateUri.UrlFolder + prefix + "options." + currentCultureCode + ".json");
                        if (optionsJson != null)
                            json["options"] = json["options"].JsonMerge(optionsJson);
                    }
                }
            }
            // view
            if (view)
            {
                var viewJson = JsonUtils.LoadJsonFromFile(_templateUri.UrlFolder + prefix + "view.json");
                if (viewJson != null)
                    json["view"] = viewJson;
            }
            return json;
        }
        public FieldConfig BuildIndex(string key)
        {
            string prefix = (string.IsNullOrEmpty(key) || key == "Items") ? "" : key + "-";
            string cacheKey = _templateUri.UrlFolder + prefix + "index.json";
            FieldConfig newConfig = (FieldConfig)DataCache.GetCache(cacheKey);
            if (newConfig == null)
            {
                var file = new FileUri(_templateUri.UrlFolder, prefix + "index.json");
                if (file.FileExists)
                {
                    string content = File.ReadAllText(file.PhysicalFilePath);
                    newConfig = JsonConvert.DeserializeObject<FieldConfig>(content);
                    DataCache.SetCache(cacheKey, newConfig, new DNNCacheDependency(file.PhysicalFilePath));
                    return newConfig;
                }
                else
                {
                    newConfig = new FieldConfig(true);
                    var jsonEdit = Build(key, DnnLanguageUtils.GetCurrentCultureCode(), true, true, false, false);
                    SchemaConfig schemaConfig = new SchemaConfig();
                    var schemaJson = jsonEdit["schema"];
                    if (schemaJson != null)
                    {
                        schemaConfig = schemaJson.ToObject<SchemaConfig>();
                    }
                    OptionsConfig optionsConfig = new OptionsConfig();
                    JToken optionsJson = jsonEdit["options"];
                    if (optionsJson != null)
                    {
                        optionsConfig = optionsJson.ToObject<OptionsConfig>();
                    }
                    foreach (var prop in schemaConfig.Properties)
                    {
                        OptionsConfig opts = null;
                        if (optionsConfig.Fields != null)
                        {
                            opts = optionsConfig.Fields.ContainsKey(prop.Key) ? optionsConfig.Fields[prop.Key] : null;
                        }
                        string optType = (opts == null) ? "text" : opts.Type ?? "text";
                        if (prop.Value.Type == "array" && (prop.Value.Enum != null || optType == "select" || optType == "select2" || optType == "role2"))
                        {
                            var newField = new FieldConfig()
                            {
                                Items = new FieldConfig()
                                {
                                    IndexType = "key",
                                    Index = true,
                                    Sort = true
                                }
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (prop.Value.Enum != null || optType == "select" || optType == "select2" || optType == "role2")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "key",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (prop.Value.Type == "boolean")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "boolean",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (prop.Value.Type == "number")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "float",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "text" || optType == "textarea" || optType == "email")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "text",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "wysihtml")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "html",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "ckeditor")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "html",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "mltext")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "text",
                                Index = true,
                                Sort = true,
                                MultiLanguage = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "mlwysihtml")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "html",
                                Index = true,
                                Sort = true,
                                MultiLanguage = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "mlckeditor")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "html",
                                Index = true,
                                Sort = true,
                                MultiLanguage = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }
                        else if (optType == "date" || optType == "datetime" || optType == "time")
                        {
                            var newField = new FieldConfig()
                            {
                                IndexType = "datetime",
                                Index = true,
                                Sort = true
                            };
                            newConfig.Fields.Add(prop.Key, newField);
                        }

                    }
                    //var json = JObject.FromObject(newConfig);
                    //File.WriteAllText(templateUri.PhysicalFullDirectory + "\\test.json", json.ToString());

                    var schemaFile = new FileUri(_templateUri.UrlFolder, prefix + "schema.json");
                    var optionsFile = new FileUri(_templateUri.UrlFolder, prefix + "options.json");
                    DataCache.SetCache(cacheKey, newConfig, new DNNCacheDependency(new[] { schemaFile.PhysicalFilePath, optionsFile.PhysicalFilePath }));
                }
            }

            return newConfig;
        }
    }
}