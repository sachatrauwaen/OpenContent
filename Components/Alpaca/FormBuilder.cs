using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene.Config;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Alpaca
{
    public class FormBuilder
    {
        private readonly FolderUri templateUri;
        public FormBuilder(FolderUri templateUri)
        {
            this.templateUri = templateUri;
        }
        public JObject BuildQuerySettings()
        {
            var indexConfig = OpenContentUtils.GetIndexConfig(templateUri);
            var jsonEdit = Build();
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
                var opts = optionsConfig.Fields.ContainsKey(prop.Key) ? optionsConfig.Fields[prop.Key] : null;
                var idxs = indexConfig.Fields.ContainsKey(prop.Key) ? indexConfig.Fields[prop.Key] : null;
                if (prop.Key == "publishstatus" || prop.Key == "publishstartdate" || prop.Key == "publishenddate")
                {
                    fieldLst.Add(propKey);
                    continue;
                }
                if (prop.Value.Type == "object" && idxs != null)
                {
                    GetFields(newSchemaFilter, newOptionsFilter, prop.Value, opts, fieldLst, idxs, propKey + ".");
                    continue;
                }
                string optType = opts == null ? "text" : opts.Type;

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
                else if (optType == "date")
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
                    /*
                    var newField = new OptionsConfig();
                    newOptionsFilter.Fields.Add(propKey, newField);
                    newField.Helper = "Use 0 for today";
                    */
                }
            }
        }

        public JObject BuildForm(string key = "")
        {
            if (key == "query")
            {
                return BuildQuerySettings();
            }
            else
            {
                return Build(key);
            }
        }
        private JObject Build(string key = "")
        {
            string prefix = string.IsNullOrEmpty(key) ? "" : key + "-";
            JObject json = new JObject();
            // schema
            var schemaJson = JsonUtils.LoadJsonFromFile(templateUri.UrlFolder + prefix + "schema.json");
            if (schemaJson != null)
                json["schema"] = schemaJson;

            // default options
            var optionsJson = JsonUtils.LoadJsonFromFile(templateUri.UrlFolder + prefix + "options.json");
            if (optionsJson != null)
                json["options"] = optionsJson;

            // language options
            optionsJson = JsonUtils.LoadJsonFromFile(templateUri.UrlFolder + prefix + "options." + DnnUtils.GetCurrentCultureCode() + ".json");
            if (optionsJson != null)
                json["options"] = json["options"].JsonMerge(optionsJson);

            // view
            optionsJson = JsonUtils.LoadJsonFromFile(templateUri.UrlFolder + prefix + "view.json");
            if (optionsJson != null)
                json["view"] = optionsJson;

            return json;
        }
        public FieldConfig BuildIndex()
        {
            var file = new FileUri(templateUri.UrlFolder, "index.json");
            if (file.FileExists)
            {
                string content = File.ReadAllText(file.PhysicalFilePath);
                var indexConfig = JsonConvert.DeserializeObject<FieldConfig>(content);
                return indexConfig;
            }

            FieldConfig newConfig = new FieldConfig(true);

            var jsonEdit = Build();
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
            foreach (var prop in schemaConfig.Properties)
            {
                OptionsConfig opts = null;
                if (optionsConfig.Fields != null)
                {
                    opts = optionsConfig.Fields.ContainsKey(prop.Key) ? optionsConfig.Fields[prop.Key] : null;
                }
                string optType = opts == null ? "text" : opts.Type;
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
                else if (optType == "text")
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
                else if (optType == "date")
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
            return newConfig;
        }
    }
}