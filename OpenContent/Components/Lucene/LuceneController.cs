using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using Satrabel.OpenContent.Components.Lucene.Config;
using Version = Lucene.Net.Util.Version;


namespace Satrabel.OpenContent.Components.Lucene
{
    public class LuceneController : IDisposable
    {
        private LuceneService _serviceStoreInstance;

        public static LuceneController Instance { get; private set; } = new LuceneController();

        [Obsolete("Do not use the Lucene Store.Commit() (as of June 2017 v3.2.3). Use LuceneController.Commit() instead.")]
        public LuceneService Store
        {
            get
            {
                if (_serviceStoreInstance == null)
                    throw new Exception("LuceneController not initialized properly");
                return _serviceStoreInstance;
            }
        }

        #region constructor

        private LuceneController()
        {
            _serviceStoreInstance = new LuceneService(App.Config.LuceneIndexFolder, JsonMappingUtils.GetAnalyser());
        }

        public static void ClearInstance()
        {
            if (Instance != null)
            {
                Instance.Dispose();
                Instance = null;
            }
            Instance = new LuceneController();
        }

        public void Dispose()
        {
            if (_serviceStoreInstance != null)
            {
                _serviceStoreInstance.Dispose();
                _serviceStoreInstance = null;
            }
        }

        #endregion

        #region Search

        public SearchResults Search(string type, SelectQueryDefinition def)
        {
            return Search(type, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
        }

        public SearchResults Search(string type, Query filter, Query query, Sort sort, int pageSize, int pageIndex)
        {
            var luceneResults = new SearchResults();

            //validate whether index folder is exist and contains index files, otherwise return null.
            if (!Store.ValidateIndexFolder())
            {
                App.Config.LuceneIndexAllDelegate();  //execute the IndexAll delegate
                return luceneResults;
            }
            if (query == null)
            {
                query = new MatchAllDocsQuery();
            }
            var searcher = Store.GetSearcher();
            TopDocs topDocs;
            var numOfItemsToReturn = (pageIndex + 1) * pageSize;
            if (filter == null)
                topDocs = Search(searcher, type, query, numOfItemsToReturn, sort);
            else
                topDocs = Search(searcher, type, filter, query, numOfItemsToReturn, sort);
            luceneResults.TotalResults = topDocs.TotalHits;

            //App.Services.Logger.Error("Search:" + string.Join(",", topDocs.ScoreDocs.Select(d => d.Score.ToString())));

            luceneResults.ids = topDocs.ScoreDocs.Skip(pageIndex * pageSize)
                .Select(d => searcher.Doc(d.Doc).GetField(JsonMappingUtils.FIELD_ID).StringValue)
                .ToArray();
            return luceneResults;
        }

        #endregion

        #region Index

        /// <summary>
        /// Reindex all content.
        /// </summary>
        public void IndexAll(Action<LuceneController> funcRegisterAllIndexableData)
        {
            App.Services.Logger.Info("Reindexing all OpenContent data, from all portals");
            LuceneController.ClearInstance();
            try
            {
                using (var lc = LuceneController.Instance)
                {
                    lc.Store.DeleteAll();
                    funcRegisterAllIndexableData(lc);
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
                LuceneController.ClearInstance();
            }
            App.Services.Logger.Info("Finished Reindexing all OpenContent data, from all portals");
        }

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
                using (var lc = LuceneController.Instance)
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
                LuceneController.ClearInstance();
            }
        }

        #endregion

        #region Operations

        public void AddList(IEnumerable<IIndexableItem> list, FieldConfig indexConfig, string scope)
        {
            DeleteAllOfType(this, scope);
            foreach (IIndexableItem item in list)
            {
                Add(item, indexConfig);
            }
        }

        public void Add(IIndexableItem data, FieldConfig config)
        {
            Requires.NotNull(data);

            Store.Add(JsonMappingUtils.JsonToDocument(data.GetScope(), data.GetId(), data.GetCreatedByUserId(), data.GetCreatedOnDate(), data.GetData(), data.GetSource(), config));
        }

        public void Update(IIndexableItem data, FieldConfig config)
        {
            Requires.NotNull(data);

            Delete(data);
            Add(data, config);
        }

        /// <summary>
        /// Deletes the matching objects in the IndexWriter.
        /// </summary>
        /// <param name="data"></param>
        public void Delete(IIndexableItem data)
        {
            Requires.NotNull(data);

            var selection = new TermQuery(new Term(JsonMappingUtils.FIELD_ID, data.GetId()));
            Query deleteQuery = new FilteredQuery(selection, JsonMappingUtils.GetTypeFilter(data.GetScope()));
            Store.Delete(deleteQuery);
        }

        public void Commit()
        {
            Store.Commit();
        }

        #endregion

        #region Private

        private static void DeleteAllOfType(LuceneController lc, string scope)
        {
            var selection = JsonMappingUtils.CreateTypeQuery(scope);
            lc.Store.Delete(selection);
        }

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
                query = parser.Parse(QueryParser.Escape(searchQuery?.Trim()));
            }
            return query;
        }

        public static TopDocs Search(Searcher searcher, string type, Query query, int numResults, Sort sort)
        {
            var res = searcher.Search(query, JsonMappingUtils.GetTypeFilter(type), numResults, sort);
            return res;
        }

        public static TopDocs Search(Searcher searcher, string type, Query query, Query filter, int numResults, Sort sort)
        {
            try
            {
                return searcher.Search(query, JsonMappingUtils.GetTypeFilter(type, filter), numResults, sort);
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Error while searching {type}, {query}", ex);
                throw;
            }
        }

        #endregion
    }
}