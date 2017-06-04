#region Usings

using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Querying.Search;

#endregion

namespace Satrabel.OpenContent.Components.Lucene
{
    public class DnnLuceneIndexAdapter : IDisposable, IIndexAdapter
    {
        public IIndexAdapter Instance => _instance;

        private static DnnLuceneIndexAdapter _instance { get; set; }
        private static string _luceneIndexFolder;

        private LuceneService _serviceInstance;


        private LuceneService Store
        {
            get
            {
                if (_serviceInstance == null)
                    throw new Exception("LuceneIndexAdapter not initialized properly");
                return _serviceInstance;
            }
        }

        #region constructor

        internal DnnLuceneIndexAdapter(string luceneIndexFolder)
        {
            DnnLuceneIndexAdapter._instance = this;
            _luceneIndexFolder = luceneIndexFolder;
            _serviceInstance = new LuceneService(luceneIndexFolder, JsonMappingUtils.GetAnalyser());
        }

        #endregion

        #region Search

        public SearchResults Search(string indexScope, Select selectQuery)
        {
            var def = new SelectQueryDefinition();
            def.Build(selectQuery);


            var results = this.Search(indexScope, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
            results.QueryDefinition = new QueryDefinition()
            {
                Filter = def.Filter.ToString(),
                Query = def.Query.ToString(),
                Sort = def.Sort.ToString(),
                PageIndex = def.PageIndex,
                PageSize = def.PageSize
            };
            return results;
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


        /// <summary>
        /// Reindex all OpenContent modules of all portals.
        /// </summary>
        public void IndexAll() //todo: this should only be called from DataSourceProviders
        {
            App.Services.Logger.Info("Reindexing all OpenContent data, from all portals");
            DnnLuceneIndexAdapter.ClearInstance();
            try
            {
                using (var lc = DnnLuceneIndexAdapter._instance)
                {
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
                App.Services.Logger.Error("Error while Reindexing all OpenContent data, from all portals", ex);
            }
            finally
            {
                DnnLuceneIndexAdapter.ClearInstance();
            }
            App.Services.Logger.Info("Finished Reindexing all OpenContent data, from all portals");
        }

        /// <summary>
        /// Use this to 
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="indexConfig">The index configuration.</param>
        /// <param name="scope">The scope.</param>
        public void ReIndexModuleData(IEnumerable<IIndexableItem> list, FieldConfig indexConfig, string scope)
        {
            try
            {
                using (DnnLuceneIndexAdapter lc = DnnLuceneIndexAdapter._instance)
                {
                    lc.Store.Delete(new TermQuery(new Term("$type", scope)));
                    foreach (var item in list)
                    {
                        lc.Add(item, indexConfig);
                    }
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            finally
            {
                DnnLuceneIndexAdapter.ClearInstance();
            }
        }

        private static void IndexModule(DnnLuceneIndexAdapter lc, OpenContentModuleConfig module)
        {
            OpenContentUtils.CheckOpenContentSettings(module);

            if (module.IsListMode() && !module.Settings.IsOtherModule && module.Settings.Manifest.Index)
            {
                IndexModuleData(lc, module.ViewModule.ModuleId, module.Settings);
            }
        }

        private static void IndexModuleData(DnnLuceneIndexAdapter lc, int moduleId, OpenContentSettings settings)
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
                throw new ArgumentNullException(nameof(data));
            }
            Store.Add(JsonMappingUtils.JsonToDocument(data.GetScope(), data.GetId(), data.GetCreatedByUserId(), data.GetCreatedOnDate(), data.GetData(), data.GetSource(), config));
        }

        public void Update(IIndexableItem data, FieldConfig config)
        {
            if (null == data)
            {
                throw new ArgumentNullException(nameof(data));
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
                throw new ArgumentNullException(nameof(data));
            }
            var selection = new TermQuery(new Term(JsonMappingUtils.FieldId, data.GetId()));
            Query deleteQuery = new FilteredQuery(selection, JsonMappingUtils.GetTypeFilter(data.GetScope()));
            Store.Delete(deleteQuery);
        }

        public void Commit()
        {
            Store.Commit();
        }

        private static void ClearInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
            _instance = new DnnLuceneIndexAdapter(_luceneIndexFolder);
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