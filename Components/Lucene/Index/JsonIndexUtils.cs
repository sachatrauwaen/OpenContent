using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Mapping;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene.Net.Mapping;
using System;
using System.Linq.Expressions;

namespace Lucene.Net.Index
{
    /// <summary>
    /// ObjectMapping related Extensions for the Lucene.Net.Index namespace.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        #region Add

        /// <summary>
        /// Adds the specified object to the given IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to use.
        /// </param>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        public static void Add(this IndexWriter writer, string type, string id, string json)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == json)
            {
                throw new ArgumentNullException("json");
            }

            writer.AddDocument(JsonMappingUtils.JsonToDocument(type, id, json));
        }

       
        /// <summary>
        /// Adds the specified object to the given IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to use.
        /// </param>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Add(this IndexWriter writer, string type, string id, string json , Analyzer analyzer)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == json)
            {
                throw new ArgumentNullException("json");
            }
            else if (null == analyzer)
            {
                throw new ArgumentNullException("analyzer");
            }

            writer.AddDocument(JsonMappingUtils.JsonToDocument(type, id ,json), analyzer);
        }

        #endregion

        #region Update

       
        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="predicate">
        /// The predicate for selecting the item to update.
        /// </param>
        public static void Update<T>(this IndexWriter writer, string type, string json, int id)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == json)
            {
                throw new ArgumentNullException("json");
            }
           // writer.DeleteDocuments<T>(selection);
            //writer.AddDocument(obj.ToDocument<T>(settings));
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
        public static void Delete<T>(this IndexWriter writer, int id)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }

            //DeleteDocuments<T>(writer, query);
        }

        #endregion
    }
}
