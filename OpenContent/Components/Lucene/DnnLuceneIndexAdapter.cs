using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene
{
    public class DnnLuceneIndexAdapter
    {
        /// <summary>
        /// A helper method to force a Datasource of a module to Reindex itself
        /// </summary>
        public static void ReIndexModuleData(OpenContentModuleConfig module)
        {
            var settings = module.Settings;
            bool index = false;
            if (settings.TemplateAvailable)
            {
                index = settings.Manifest.Index;
            }
            IDataSource ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            if (index && ds is IDataIndex)
            {
                var dsContext = OpenContentUtils.CreateDataContext(module);
                var dataIndex = (IDataIndex)ds;
                var indexableData = dataIndex.GetIndexableData(dsContext);

                string scope = OpenContentInfo.GetScope(dsContext.ModuleId, dsContext.Collection);
                var indexConfig = OpenContentUtils.GetIndexConfig(new FolderUri(dsContext.TemplateFolder), dsContext.Collection); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files
                LuceneController.Instance.ReIndexData(indexableData, indexConfig, scope);
            }
        }

        public static void IndexAll()
        {
            LuceneController.Instance.IndexAll(RegisterAllIndexableData);
        }

        /// <summary>
        /// An override to Register all indexable data. This is used by the IndexAll() of the base.BaseLuceneIndexAdapter
        /// </summary>
        protected static void RegisterAllIndexableData(LuceneController lc)
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var modules = DnnUtils.GetDnnOpenContentModules(portal.PortalID);
                foreach (var module in modules)
                {
                    if (!OpenContentUtils.CheckOpenContentSettings(module)) { continue; }
                    if (module.IsListMode() && !module.Settings.IsOtherModule && module.Settings.Manifest.Index)
                    {
                        RegisterModuleDataForIndexing(lc, module);
                    }
                }
            }
        }

        private static void RegisterModuleDataForIndexing(LuceneController lc, OpenContentModuleConfig module)
        {
            bool index = false;
            var settings = module.Settings;
            if (settings.TemplateAvailable)
            {
                index = settings.Manifest.Index;
            }
            if (!index) return;

            IDataSource ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            if (ds is IDataIndex)
            {
                var moduleId = settings.IsOtherModule ? settings.ModuleId : module.ViewModule.ModuleId;
                string scope = OpenContentInfo.GetScope(moduleId, settings.Template.Collection);
                FieldConfig indexConfig = OpenContentUtils.GetIndexConfig(settings.Template); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files
                var dsContext = OpenContentUtils.CreateDataContext(module);
                var dataIndex = (IDataIndex)ds;
                var indexableData = dataIndex.GetIndexableData(dsContext);
                lc.AddList(indexableData, indexConfig, scope);
            }
        }
    }
}