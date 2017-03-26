#region Usings

using System;
using System.Collections;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using Satrabel.OpenContent.Components.Lucene.Config;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using Version = Lucene.Net.Util.Version;
using Satrabel.OpenContent.Components.Lucene.Index;

#endregion

namespace Satrabel.OpenContent.Components.Lucene
{
    public class LuceneController : IDisposable
    {
        private static LuceneController _instance = new LuceneController();
        private LuceneService _serviceInstance;

        public static LuceneController Instance
        {
            get { return _instance; }
        }

        public LuceneService Store
        {
            get
            {
                if (_serviceInstance == null)
                    throw new Exception("LuceneController not initialized properly");
                return _serviceInstance;
            }
        }

        #region constructor

        private LuceneController()
        {
            _serviceInstance = new LuceneService(AppConfig.Instance.LuceneIndexFolder, JsonMappingUtils.GetAnalyser());
        }

        public static void ClearInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
            _instance = new LuceneController();
        }

        #endregion

        #region Search

        public SearchResults Search(string type, QueryDefinition def)
        {
            return Search(type, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
        }

        public SearchResults Search(string type, Query filter, Query query, Sort sort, int pageSize, int pageIndex)
        {
            var luceneResults = new SearchResults();

            //validate whether index folder is exist and contains index files, otherwise return null.
            if (!Store.ValidateIndexFolder())
            {
                IndexAll();
                return luceneResults;
            }

            var searcher = Store.GetSearcher();
            TopDocs topDocs;
            var numOfItemsToReturn = (pageIndex + 1) * pageSize;
            if (filter == null)
                topDocs = searcher.Search(type, query, numOfItemsToReturn, sort);
            else
                topDocs = searcher.Search(type, filter, query, numOfItemsToReturn, sort);
            luceneResults.TotalResults = topDocs.TotalHits;
            luceneResults.ids = topDocs.ScoreDocs.Skip(pageIndex * pageSize)
                    .Select(d => searcher.Doc(d.Doc).GetField(JsonMappingUtils.FieldId).StringValue)
                    .ToArray();
            return luceneResults;
        }

        #endregion

        #region Index

        public void ReIndexModuleData(int moduleId, OpenContentSettings settings)
        {
            try
            {
                using (LuceneController lc = LuceneController.Instance)
                {
                    IndexModuleData(lc, moduleId, settings);
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            finally
            {
                LuceneController.ClearInstance();
            }
        }

        /// <summary>
        /// Reindex all OpenContent modules of all portals.
        /// </summary>
        internal void IndexAll()
        {
            Log.Logger.Info("Reindexing all OpenContent data, from all portals");
            LuceneController.ClearInstance();
            try
            {
                using (var lc = LuceneController.Instance)
                {
                    ModuleController mc = new ModuleController();
                    foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                    {
                        var modules = DnnUtils.GetDnnOpenContentModules(portal.PortalID);
                        foreach (var module in modules)
                        {
                            IndexModule(lc, module);
                        }
                    }
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Error while Reindexing all OpenContent data, from all portals", ex);
            }
            finally
            {
                LuceneController.ClearInstance();
            }
            Log.Logger.Info("Finished Reindexing all OpenContent data, from all portals");
        }

        private void IndexModule(LuceneController lc, OpenContentModuleInfo module)
        {
            OpenContentUtils.CheckOpenContentSettings(module);

            if (module.IsListMode() && !module.Settings.IsOtherModule)
            {
                IndexModuleData(lc, module.ViewModule.ModuleID, module.Settings);
            }
        }

        private void IndexModuleData(LuceneController lc, int moduleId, OpenContentSettings settings)
        {
            bool index = false;
            if (settings.TemplateAvailable)
            {
                index = settings.Manifest.Index;
            }
            FieldConfig indexConfig = null;
            if (index)
            {
                indexConfig = OpenContentUtils.GetIndexConfig(settings.Template);
            }

            if (settings.IsOtherModule)
            {
                moduleId = settings.ModuleId;
            }

            lc.Store.Delete(new TermQuery(new Term("$type", OpenContentInfo.GetScope(moduleId, settings.Template.Collection))));
            OpenContentController occ = new OpenContentController();
            foreach (var item in occ.GetContents(moduleId, settings.Template.Collection))
            {
                lc.Add(item, indexConfig);
            }
        }

        #endregion

        #region Operations

        public void Add(IIndexableItem data, FieldConfig config)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }
            Store.Add(JsonMappingUtils.JsonToDocument(data.GetScope(), data.GetId(), data.GetCreatedByUserId(), data.GetCreatedOnDate(), data.GetData(), data.GetSource(), config));
        }

        public void Update(IIndexableItem data, FieldConfig config)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }
            Delete(data);
            Add(data, config);
        }

        /// <summary>
        /// Deletes the matching objects in the IndexWriter.
        /// </summary>
        /// <param name="data"></param>
        public void Delete(IIndexableItem data)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }
            var selection = new TermQuery(new Term(JsonMappingUtils.FieldId, data.GetId()));
            Query deleteQuery = new FilteredQuery(selection, JsonMappingUtils.GetTypeFilter(data.GetScope()));
            Store.Delete(deleteQuery);
        }

        #endregion

        #region Private

        public static Query ParseQuery(string searchQuery, string defaultFieldName)
        {
            var parser = new QueryParser(Version.LUCENE_30, defaultFieldName, JsonMappingUtils.GetAnalyser());
            Query query;
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    query = new MatchAllDocsQuery();
                }
                else
                {
                    query = parser.Parse(searchQuery.Trim());
                }
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        #endregion

        public void Dispose()
        {
            if (_serviceInstance != null)
            {
                _serviceInstance.Dispose();
                _serviceInstance = null;
            }
        }
    }
}