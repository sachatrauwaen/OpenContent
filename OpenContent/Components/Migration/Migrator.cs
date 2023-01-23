using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Datasource.Search;

namespace Satrabel.OpenContent.Components.Migration
{
    /// <summary>
    /// Service to help migrate the data from one field (source field) to another field (target field).
    /// Usage:
    /// a) Modify your option file:
    ///     - add a target field to your schema. It should be on the same level as the source field
    ///     - modify your option file manually: add to your source field: "migrateto": "nameOfTargetField"
    /// b) Create a Razor template with these lines and use it on a page and visit that page to run this code.
    ///     var templatefolder = "NameOfMyOpenContentTemplateFolder"  // do NOT include /portals/x/OpenContent/Templates/
    ///     var overwriteTargetData = false; // set to true if any data of the target field needs to be overwritten
    ///     @Satrabel.OpenContent.Components.Migration.FieldMigrator.Migrate(templatefolder, overwriteTargetData)
    /// </summary>
    public class FieldMigrator
    {
        /// <summary>
        /// A Factory class that creates a FieldMigrator and executes the migration
        /// </summary>
        public static HtmlString Migrate(OpenContentWebPage caller, string folder, bool overwriteTargetData, string migrationVersion)
        {
            try
            {
                int portalId = caller.Dnn.Portal.PortalId;
                string templateFolder = HostingEnvironment.MapPath("~/" + caller.Dnn.Portal.HomeDirectory + "OpenContent/Templates/" + folder);
                if (!Directory.Exists(templateFolder)) return new HtmlString($"The folder '{templateFolder}' does not exist.");
                if (!HasOptionsFile(templateFolder)) return new HtmlString($"The folder '{templateFolder}' does not have an options.json file.");
                var migrator = new FieldMigrator(templateFolder, portalId, overwriteTargetData, migrationVersion);
                return new HtmlString("Migration Succesful");
            }

            catch (Exception ex)
            {
                return new HtmlString("Migration Error : " + ex.Message);
            }
        }

        private FieldMigrator(string folder, int portalId, bool overwriteTargetData, string migration)
        {

            var modules = (new ModuleController()).GetModules(portalId).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == App.Config.Opencontent &&
                                        !m.IsDeleted &&
                                        !m.OpenContentSettings().IsOtherModule &&
                                        m.OpenContentSettings().TemplateDir.PhysicalFullDirectory == folder);
            foreach (var module in modules)
            {
                try
                {
                    var ocConfig = OpenContentModuleConfig.Create(module, new PortalSettings(portalId));
                    var dsContext = OpenContentUtils.CreateDataContext(ocConfig);
                    //var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(dsContext.TemplateFolder), dsContext.Collection);
                    IDataSource ds = DataSourceManager.GetDataSource(ocConfig.Settings.Manifest.DataSource);
                    var alpaca = ds.GetAlpaca(dsContext, true, true, false);
                    JObject schema = null;
                    JObject options = null;
                    if (alpaca != null)
                    {
                        schema = alpaca["schema"] as JObject; // cache
                        options = alpaca["options"] as JObject; // cache
                    }

                    //if (m.OpenContentSettings().Manifest.li)

                    foreach (var dataItem in ds.GetAll(dsContext, null).Items)
                    {
                        var ocDataItem = (OpenContentInfo)dataItem.Item;
                        JToken sourceData = dataItem.Data;
                        if (sourceData["migration"] == null || sourceData["migration"].ToString() != migration)
                        {
                            MigrateData(sourceData, schema, options, portalId, module.ModuleID, overwriteTargetData);
                            sourceData["migration"] = migration;
                            ds.Update(dsContext, dataItem, sourceData);
                        }
                        //var targetData = ConvertToTargetData(sourceField, targetField, sourceData);
                        //SetFieldData(ocDataItem, targetField, targetData, overwriteTargetData);
                        //todo: save ocDataItem
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error during Migration : " + e.Message, e);
                    App.Services.Logger.Error("Error during Migration.", e);
                }
            }
        }

        private void MigrateData(JToken data, JObject schema, JObject options, int portalId, int moduleID, bool overwriteTargetData)
        {
            if (schema?["properties"] == null)
                return;
            if (options?["fields"] == null)
                return;

            foreach (var child in data.Children<JProperty>().ToList())
            {
                JObject sch = schema["properties"][child.Name] as JObject;
                JObject opt = options["fields"][child.Name] as JObject;
                if (sch == null) continue;
                if (opt == null) continue;
                var childProperty = child;
                if (childProperty.Value is JArray)
                {
                    var array = childProperty.Value as JArray;
                    //JArray newArray = new JArray();
                    foreach (var value in array)
                    {
                        var obj = value as JObject;
                        if (obj != null && opt != null && sch["items"] != null && opt["items"] != null)
                        {
                            MigrateData(obj, sch["items"] as JObject, opt["items"] as JObject, portalId, moduleID, overwriteTargetData);
                        }
                        else // no migrations of array of value 
                        {
                            //var val = value as JValue;
                            //if (val != null)
                            //{
                            //    try
                            //    {
                            //        newArray.Add(GenerateImage(reqOpt, val.ToString(), isEditable));
                            //    }
                            //    catch (Exception)
                            //    {
                            //    }
                            //}
                        }
                    }
                    //if (image)
                    //{
                    //    childProperty.Value = newArray;
                    //}
                }
                else if (childProperty.Value is JObject)
                {
                    var obj = childProperty.Value as JObject;
                    if (obj != null && sch != null && opt != null)
                    {
                        MigrateData(obj, sch, opt, portalId, moduleID, overwriteTargetData);
                    }
                }
                else if (childProperty.Value is JValue)
                {
                    if (opt != null && opt["migrateTo"] != null)
                    {
                        string migrateTo = opt["migrateTo"].ToString();
                        if (!string.IsNullOrEmpty(migrateTo))
                        {
                            JObject toSch = schema["properties"][migrateTo] as JObject;
                            JObject toOpt = options["fields"][migrateTo] as JObject;
                            if (toSch == null || toOpt == null)
                            {
                                throw new ArgumentOutOfRangeException($"Migration target field  {migrateTo} dousnt exist.");
                            }
                            var sourceField = new OcFieldInfo(sch, opt);
                            var targetField = new OcFieldInfo(toSch, toOpt);

                            if (data[migrateTo] == null || overwriteTargetData)
                            {
                                JToken val = childProperty.Value;
                                data[migrateTo] = ConvertTo(val, sourceField, targetField, portalId, moduleID);
                            }
                        }
                    }
                }
            }
        }

        private JToken ConvertTo(JToken sourceData, OcFieldInfo sourceField, OcFieldInfo targetField, int portalId, int moduleID)
        {
            if (sourceField.Type == "image2" && targetField.Type == "imagex")
            {
                return MigratorHelper.Image2ToImageX(sourceData, sourceField, targetField, portalId, moduleID);
            }
            if (sourceField.Type == "text" && targetField.Type == "textarea")
            {
                return MigratorHelper.TextToTextArea(sourceData, sourceField, targetField);
            }
            throw new NotImplementedException($"Migration from field type {sourceField.Type} to {targetField.Type} is not supported.");

        }

        private static bool HasOptionsFile(string folder)
        {
            string path = Path.Combine(folder, "options.json");
            return File.Exists(path);
        }
    }

    public class OcFieldInfo
    {
        public OcFieldInfo(JObject schema, JObject options)
        {
            Schema = schema;
            Options = options;
        }

        public JObject Schema { get; }
        public JObject Options { get; }

        public string Type
        {
            get
            {
                return Options["type"].ToString();
            }
        }
    }
}