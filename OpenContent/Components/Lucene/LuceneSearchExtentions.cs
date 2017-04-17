using System;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene.Mapping;

namespace Satrabel.OpenContent.Components.Lucene
{
    /// <summary>
    /// Extension class to help searching for Documents which are mapped from objects.
    /// </summary>
    public static class LuceneSearchExtentions
    {
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

    }
}
