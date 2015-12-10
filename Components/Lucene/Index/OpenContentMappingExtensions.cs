using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using System;
using System.Linq.Expressions;

namespace Satrabel.OpenContent.Components.Lucene.Index
{
    /// <summary>
    /// </summary>
    public static class OpenContentMappingExtensions
    {
        #region Add

        public static void Add(this LuceneController controller, OpenContentInfo data, FieldConfig config)
        {
            if (null == controller)
            {
                throw new ArgumentNullException("controller");
            }
            else if (null == data)
            {
                throw new ArgumentNullException("data");
            }

            controller.Add(JsonMappingUtils.JsonToDocument(data.ModuleId.ToString(), data.ContentId.ToString(), data.Json, config));
        }


        #endregion

        #region Update


        public static void Update(this LuceneController controller, OpenContentInfo data, FieldConfig config)
        {
            if (null == controller)
            {
                throw new ArgumentNullException("controller");
            }
            else if (null == data)
            {
                throw new ArgumentNullException("data");
            }
            controller.Delete(data);
            controller.Add(data, config);
        }

        #endregion

        #region DeleteDocuments

        /// <summary>
        /// Deletes the matching objects in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="predicate">
        /// The predicate for selecting the item to update.
        /// </param>
        public static void Delete(this LuceneController controller, OpenContentInfo data)
        {
            if (null == controller)
            {
                throw new ArgumentNullException("controller");
            }
            else if (null == data)
            {
                throw new ArgumentNullException("data");
            }

            var selection = new TermQuery(new Term(JsonMappingUtils.FieldId, data.ContentId.ToString()));

            Query deleteQuery = new FilteredQuery(selection, JsonMappingUtils.GetTypeFilter(data.ModuleId.ToString()));
            controller.Delete(deleteQuery);
        }

        #endregion
    }
}
