using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using Satrabel.OpenContent.Components.Querying.Search;

namespace Satrabel.OpenContent.Components.Lucene
{
    public class BaseLuceneIndexAdapter : IDisposable, IIndexAdapter
    {
        public IIndexAdapter Instance => _instance;

        protected static BaseLuceneIndexAdapter _instance { get; set; }
        protected static string _luceneIndexFolder;

        private LuceneService _serviceInstance;


        protected internal LuceneService Store
        {
            get
            {
                if (_serviceInstance == null)
                    throw new Exception("LuceneIndexAdapter not initialized properly");
                return _serviceInstance;
            }
        }

        #region constructor

        protected BaseLuceneIndexAdapter(string luceneIndexFolder)
        {
            _instance = this;
            _luceneIndexFolder = luceneIndexFolder;
            _serviceInstance = new LuceneService(luceneIndexFolder, JsonMappingUtils.GetAnalyser());
        }

        #endregion

        #region Search

        public SearchResults Search(string indexScope, Select selectQuery)
        {
            var def = new SelectQueryDefinition();
            def.Build(selectQuery);


            var results = Search(indexScope, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
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
            ClearInstance();
            try
            {
                using (var lc = _instance)
                {
                    GetIndexData(lc);
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
                ClearInstance();
            }
            App.Services.Logger.Info("Finished Reindexing all OpenContent data, from all portals");
        }

        protected virtual void GetIndexData(BaseLuceneIndexAdapter lc)
        {
            throw new Exception("method GetIndexData should be overridden");
        }

        //public abstract void ReIndexModuleData(IEnumerable<IIndexableItem> list, FieldConfig indexConfig, string scope);

        /// <summary>
        /// Use this to 
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="indexConfig">The index configuration.</param>
        /// <param name="scope">The scope.</param>
        public void ReIndexData(IEnumerable<IIndexableItem> list, FieldConfig indexConfig, string scope)
        {

            try
            {
                using (BaseLuceneIndexAdapter lc = _instance)
                {
                    DeleteAllOfType(lc, scope);
                    foreach (IIndexableItem item in list)
                    {
                        lc.Add(item, indexConfig);
                    }
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            finally
            {
                BaseLuceneIndexAdapter.ClearInstance();
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

        internal void DeleteAllOfType(string scope)
        {
            DeleteAllOfType(this, scope);
        }

        internal void DeleteAllOfType(BaseLuceneIndexAdapter lc, string scope)
        {
            var selection = new TermQuery(new Term("$type", scope));
            lc.Store.Delete(selection);
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
            _instance = new BaseLuceneIndexAdapter(_luceneIndexFolder);
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