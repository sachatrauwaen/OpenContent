using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        public static HtmlString Migrate(object caller, string folder, bool overwriteTargetData, int portalId)
        {
            var validatedFolder = new FolderUri(folder);
            var migrator = new FieldMigrator(validatedFolder, portalId);
            return new HtmlString("Migration Succesful");
        }

        private FieldMigrator(FolderUri folder, int portalId)
        {
            OcFieldInfo targetField;
            OcFieldInfo sourceField;
            DetectMigrationFields(out sourceField, out targetField);

            IEnumerable<ModuleInfo> modules = (new ModuleController()).GetModules(portalId).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == App.Config.Opencontent && !m.IsDeleted && !m.OpenContentSettings().IsOtherModule);
            foreach (var module in modules)
            {
                try
                {
                    var ocConfig = OpenContentModuleConfig.Create(module, new PortalSettings(portalId));
                    var dsContext = OpenContentUtils.CreateDataContext(ocConfig);
                    //var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(dsContext.TemplateFolder), dsContext.Collection);
                    IDataSource ds = DataSourceManager.GetDataSource(ocConfig.Settings.Manifest.DataSource);
                    foreach (var dataItem in ds.GetAll(dsContext, new Select()).Items)
                    {
                        var ocDataItem = (OpenContentInfo)dataItem.Item;
                        JToken sourceData = GetFieldData(ocDataItem, sourceField);
                        var targetData = ConvertToTargetData(sourceField, targetField, sourceData);
                        SetFieldData(ocDataItem, targetField, targetData);
                        //todo: save ocDataItem
                    }
                }
                catch (Exception e)
                {
                    App.Services.Logger.Error("Error during Migration.", e);
                }
            }
        }

        private void SetFieldData(OpenContentInfo openContentInfo, OcFieldInfo targetField, JToken targetData)
        {
            // todo
        }

        private JToken ConvertToTargetData(OcFieldInfo sourceField, OcFieldInfo targetField, JToken sourceData)
        {
            if (sourceField.Type == "image2" && targetField.Type == "imagex")
            {
                return MigratorHelper.Image2ToImageX(sourceData);
            }

            throw new NotImplementedException($"Migration from field type {sourceField.Type} to {targetField.Type} is not supported.");
        }

        private JToken GetFieldData(OpenContentInfo ocDataItem, OcFieldInfo sourceField)
        {
            // todo
            return ocDataItem.JsonAsJToken.First; 
        }

        /// <summary>
        /// Inspect the option file, look for a field with "migrateto":"ImageId2"
        /// </summary>
        private void DetectMigrationFields(out OcFieldInfo sourcefield, out OcFieldInfo targetfield)
        {
            //todo: for each field in the option file
            //todo: if the field has a property 'migrateto' then it is the source field. Fetch its name and type.
            sourcefield = new OcFieldInfo("nameOfSourceField", "image2");
            //todo: the value of the 'migrateto' property is the target field. Fetch its name and type.
            //todo:  throw ex 
            //todo:  a) if no source field is found
            //todo:  b) if targetfield does not exist (on the same level)
            targetfield = new OcFieldInfo("nameOfTargerField", "ImageX");

        }
    }

    public class OcFieldInfo
    {
        public OcFieldInfo(string fieldName, string fieldType)
        {
            Name = fieldName;
            Type = fieldType.ToLowerInvariant();
        }

        public string Name { get; }
        public string Type { get; }
    }
}