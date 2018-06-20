using System.Collections.Generic;
using System.Linq;
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
        /// A helper method to force a Datasource of a module to Reindex itself
        /// </summary>
        public static void ReIndexModuleData(OpenContentModuleConfig module)
        {
            var indexableData = GetModuleIndexableData(module);
            var dataExample = indexableData?.ToList().FirstOrDefault();
            if (dataExample == null) return;

            var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files

            string scope = dataExample.GetScope();

            LuceneController.Instance.ReIndexData(indexableData, indexConfig, scope);
        }

        #endregion

        #region private helpers

        /// <summary>
        /// A method to Register all indexable data. This is used by IndexAll()
        /// </summary>
        private static void RegisterAllIndexableData(LuceneController lc)
        {
            App.Services.Logger.Info("Start Reindexing all OpenContent Data");
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var modules = DnnUtils.GetDnnOpenContentModules(portal.PortalID);
                foreach (var module in modules)
                {
                    if (!OpenContentUtils.CheckOpenContentTemplateFiles(module)) { continue; }
                    if (module.IsListMode() && !module.Settings.IsOtherModule && module.Settings.Manifest.Index)
                    {
                        RegisterModuleDataForIndexing(lc, module);
                    }
                }
            }
            App.Services.Logger.Info("Finished Reindexing all OpenContent Data");
        }

        private static void RegisterModuleDataForIndexing(LuceneController lc, OpenContentModuleConfig module)
        {
            var indexableData = GetModuleIndexableData(module);
            var dataExample = indexableData?.ToList().FirstOrDefault();
            if (dataExample == null) return;

            var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template); //todo index is being build from schema & options. But they should be provided by the provider, not directly from the files

            string scope = dataExample.GetScope();

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