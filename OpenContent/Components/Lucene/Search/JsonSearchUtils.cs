using Satrabel.OpenContent.Components.Lucene.Mapping;
using System;
using Satrabel.OpenContent.Components;

namespace Lucene.Net.Search
{
    /// <summary>
    /// Extension class to help searching for Documents which are mapped from objects.
    /// </summary>
    public static class JsonSearchUtils
    {
        #region Plain Query, no Sort
        public static TopDocs Search(this Searcher searcher, string type, Query query, int numResults)
        {
            return searcher.Search(query, JsonMappingUtils.GetTypeFilter(type), numResults);
        }
        public static TopDocs Search(this Searcher searcher, string type, Query Filter, Query query, int numResults)
        {
            Filter filter = new QueryWrapperFilter(query);
            return searcher.Search(query, JsonMappingUtils.GetTypeFilter(type, Filter), numResults);
        }
        #endregion

        #region With Sort
        public static TopDocs Search(this Searcher searcher, string type, Query query, int numResults, Sort sort)
        {
            var res = searcher.Search(query, JsonMappingUtils.GetTypeFilter(type), numResults, sort);
            return res;
        }
        public static TopDocs Search(this Searcher searcher, string type, Query query, Query filter, int numResults, Sort sort)
        {
            try
            {
                return searcher.Search(query, JsonMappingUtils.GetTypeFilter(type, filter), numResults, sort);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error while searching {type}, {query}");
                throw;
            }
        }
        #endregion

        #region Search with Collector

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query and Collector.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="type">
        /// The type of the object to search documents for.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="results">
        /// The Collector to use to gather results.
        /// </param>
        public static void Search(this Searcher searcher, string type, Query query, Collector results)
        {
            searcher.Search(query, JsonMappingUtils.GetTypeFilter(type), results);
        }


        #endregion
    }
}
