using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;

namespace Satrabel.OpenContent.Components.Migration
{
    /// <summary>
    /// Service to help migrate the data from one field (source field) to another field (target field).
    /// Usage: <see cref="https://opencontent.readme.io/v5.0/docs/datafield-migration"/>
    /// </summary>
    public class FieldMigrator
    {
        /// <summary>
        /// A Factory class that creates a FieldMigrator and executes the migration
        /// </summary>
        public static HtmlString Migrate(OpenContentWebPage caller, string folder, string migrationVersion, bool overwriteTargetData, bool dryRun)
        {
            try
            {
                int portalId = caller.Dnn.Portal.PortalId;
                string templateFolder = HostingEnvironment.MapPath("~/" + caller.Dnn.Portal.HomeDirectory + "OpenContent/Templates/" + folder);
                if (!Directory.Exists(templateFolder)) return new HtmlString($"The folder '{templateFolder}' does not exist.");
                if (!HasOptionsFile(templateFolder)) return new HtmlString($"The folder '{templateFolder}' does not have an options.json file.");
                var migrator = new FieldMigrator(templateFolder, portalId, overwriteTargetData, migrationVersion, dryRun);

                return migrator.Report.Print();
            }

            catch (Exception ex)
            {
                return new HtmlString("Migration Error : " + ex.Message);
            }
        }

        private MigrationStatusReport Report { get; }

        private FieldMigrator(string folder, int portalId, bool overwriteTargetData, string migrationVersion, bool dryRun)
        {
            // Initialize status report
            Report = new MigrationStatusReport(folder, migrationVersion, dryRun);

            // fetch all modules with data and with the requested template
            var modules = (new ModuleController()).GetModules(portalId).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == App.Config.Opencontent &&
                                        !m.IsDeleted &&
                                        !m.OpenContentSettings().IsOtherModule &&
                                        m.OpenContentSettings().TemplateDir != null &&
                                        m.OpenContentSettings().TemplateDir.PhysicalFullDirectory == folder);

            // migrate the data of each module
            foreach (var module in modules)
            {
                try
                {
                    Report.FoundModule();
                    var ocConfig = OpenContentModuleConfig.Create(module, new PortalSettings(portalId));
                    var dsContext = OpenContentUtils.CreateDataContext(ocConfig);
                    var ds = DataSourceManager.GetDataSource(ocConfig.Settings.Manifest.DataSource);
                    var alpaca = ds.GetAlpaca(dsContext, true, true, false);
                    JObject schema = null;
                    JObject options = null;
                    if (alpaca != null)
                    {
                        schema = alpaca["schema"] as JObject; // cache
                        options = alpaca["options"] as JObject; // cache
                    }

                    if (schema == null || options == null)
                        throw new Exception("Cannot Migrate: no Options found.");

                    foreach (var dataItem in ds.GetAll(dsContext, null).Items)
                    {
                        Report.FoundModuleData();
                        JToken sourceData = dataItem.Data;
                        if (sourceData["migration"] == null || sourceData["migration"].ToString() != migrationVersion)
                        {
                            MigrateData(Report, sourceData, schema, options, portalId, module.ModuleID, overwriteTargetData, dryRun);
                            sourceData["migration"] = migrationVersion; // mark an item as migrated
                            ds.Update(dsContext, dataItem, sourceData);
                        }
                        else
                            Report.FoundAlreadyUpgradedData();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error during Migration : " + e.Message, e);
                }
            }
        }

        /// <summary>
        /// Recursive method that traverses the schema and Migrates the data of any field that is marked with 'migrateto'
        /// </summary>
        private static void MigrateData(MigrationStatusReport report, JToken data, JObject schema, JObject options, int portalId, int moduleID, bool overwriteTargetData, bool dryRun)
        {
            if (schema?["properties"] == null)
                return;
            if (options?["fields"] == null)
                return;

            foreach (var childProperty in data.Children<JProperty>().ToList())
            {
                var schemaOfCurrentField = schema["properties"][childProperty.Name] as JObject;
                var optionsOfCurrentField = options["fields"][childProperty.Name] as JObject;
                if (schemaOfCurrentField == null) continue;
                if (optionsOfCurrentField == null) continue;
                if (childProperty.Value is JArray) // the property is of type Array
                {
                    var array = childProperty.Value as JArray;
                    //JArray newArray = new JArray();
                    foreach (var value in array)
                    {
                        var obj = value as JObject; // are we dealing with an data object? 
                        if (obj != null && optionsOfCurrentField != null && schemaOfCurrentField["items"] != null && optionsOfCurrentField["items"] != null)
                        {
                            // loop again to check the next level
                            MigrateData(report, obj, schemaOfCurrentField["items"] as JObject, optionsOfCurrentField["items"] as JObject, portalId, moduleID, overwriteTargetData, dryRun);
                        }
                        else // we are dealing with array of JValues 
                        {
                            // This Migration is not yet supported. It probably never will, as the builder does not allow creating Arrays of values.
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
                else if (childProperty.Value is JObject) // the property is an Object
                {
                    var obj = childProperty.Value as JObject;
                    if (obj != null && schemaOfCurrentField != null && optionsOfCurrentField != null)
                    {
                        MigrateData(report, obj, schemaOfCurrentField, optionsOfCurrentField, portalId, moduleID, overwriteTargetData, dryRun);
                    }
                }
                else if (childProperty.Value is JValue)  // the property is an Value
                {
                    if (optionsOfCurrentField != null && optionsOfCurrentField["migrateTo"] != null)
                    {
                        report.FoundMigrateTo();

                        string migrateTo = optionsOfCurrentField["migrateTo"].ToString();
                        if (string.IsNullOrEmpty(migrateTo))
                            throw new Exception("Found 'migrateTo' tag which was empty.  Remove it first and try again.");

                        var schemaOfTargetField = schema["properties"][migrateTo] as JObject;
                        var optionsOfTargetField = options["fields"][migrateTo] as JObject;
                        if (schemaOfTargetField == null || optionsOfTargetField == null)
                        {
                            throw new ArgumentOutOfRangeException($"Migration target field {migrateTo} doesn't exist.");
                        }
                        var sourceField = new OcFieldInfo(schemaOfCurrentField, optionsOfCurrentField);
                        var targetField = new OcFieldInfo(schemaOfTargetField, optionsOfTargetField);

                        if (data[migrateTo] != null && !overwriteTargetData)
                        {
                            report.FoundDoNotOverwrite();
                            continue;
                        }

                        JToken val = childProperty.Value;
                        var newData = MigratorHelper.ConvertTo(report, val, sourceField, targetField, portalId, moduleID, dryRun);
                        if (!dryRun)
                            data[migrateTo] = newData;
                    }
                }
            }
        }

        private static bool HasOptionsFile(string folder)
        {
            string path = Path.Combine(folder, "options.json");
            return File.Exists(path);
        }
    }
}