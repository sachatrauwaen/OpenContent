using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene
{
    public static class LuceneUtils
    {
        #region Search Utils

        public static SearchResults Search(string indexScope, Select selectQuery)
        {
            var def = new SelectQueryDefinition();
            def.Build(selectQuery);

            var results = LuceneController.Instance.Search(indexScope, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
            results.ResultDefinition = new ResultDefinition()
            {
                Filter = def.Filter.ToString(),
                Query = def.Query.ToString(),
                Sort = def.Sort.ToString(),
                PageIndex = def.PageIndex,
                PageSize = def.PageSize
            };
            return results;
        }

        #endregion

        #region Indexing Utils

        public static void IndexAll()
        {
            LuceneController.Instance.IndexAll(RegisterAllIndexableData);
        }

        /// <summary>
        /// A method to Register all indexable data. This is used by IndexAll()
        /// </summary>
        public static void RegisterAllIndexableData(LuceneController lc)
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

        /// <summary>
        /// A helper method to force a Datasource of a module to Reindex itself
        /// </summary>
        public static void ReIndexModuleData(OpenContentModuleConfig module)
        {
            var indexableData = GetModuleIndexableData(module);
            if (indexableData == null) return;

            var settings = module.Settings;
            var moduleId = settings.IsOtherModule ? settings.ModuleId : module.ViewModule.ModuleId;
            string scope = OpenContentInfo.GetScope(moduleId, settings.Template.Collection);
            var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files

            LuceneController.Instance.ReIndexData(indexableData, indexConfig, scope);
        }

        #endregion

        #region private helpers

        private static void RegisterModuleDataForIndexing(LuceneController lc, OpenContentModuleConfig module)
        {
            var indexableData = GetModuleIndexableData(module);
            if (indexableData == null) return;

            var settings = module.Settings;
            var moduleId = settings.IsOtherModule ? settings.ModuleId : module.ViewModule.ModuleId;
            string scope = OpenContentInfo.GetScope(moduleId, settings.Template.Collection);
            var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files

            lc.AddList(indexableData, indexConfig, scope);
        }

        private static IEnumerable<IIndexableItem> GetModuleIndexableData(OpenContentModuleConfig module)
        {
            bool index = false;
            var settings = module.Settings;

            if (settings.TemplateAvailable)
            {
                index = settings.Manifest.Index;
            }
            if (!index) return null;

            IDataSource ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            if (!(ds is IDataIndex)) return null;

            var dsContext = OpenContentUtils.CreateDataContext(module);
            var dataIndex = (IDataIndex)ds;
            return dataIndex.GetIndexableData(dsContext);
        }

        #endregion
    }
}