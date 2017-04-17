using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using System;
using Satrabel.OpenContent.Components.Indexing;

namespace Satrabel.OpenContent.Components.Lucene.Mapping
{
    public static class JsonMappingUtils
    {
        #region Consts
        /// <summary>
        /// The name of the field which holds the type.
        /// </summary>
        public static readonly string FieldType = "$type";
        /// <summary>
        /// The name of the field which holds the JSON-serialized source of the object.
        /// </summary>
        public static readonly string FieldSource = "$source";

        /// <summary>
        /// The name of the field which holds the timestamp when the document was created.
        /// </summary>
        public static readonly string FieldTimestamp = "$timestamp";
        public static readonly string FieldId = "$id";
        public static readonly string FieldUserId = "$userid";
        public static readonly string FieldCreatedOnDate = "$createdondate";
        #endregion

        public static Document JsonToDocument(string type, string id, string userId, DateTime createdOnDate, JToken json, string source, FieldConfig config, bool storeSource = false)
        {
            var objectMapper = new JsonObjectMapper();
            Document doc = new Document();

            doc.Add(new Field(FieldType, type, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldId, id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldUserId, userId, Field.Store.NO, Field.Index.NOT_ANALYZED));
            if (storeSource)
            {
                doc.Add(new Field(FieldSource, source, Field.Store.YES, Field.Index.NO));
            }
            doc.Add(new NumericField(FieldTimestamp, Field.Store.YES, true).SetLongValue(DateTime.UtcNow.Ticks));
            doc.Add(new NumericField(FieldCreatedOnDate, Field.Store.NO, true).SetLongValue(createdOnDate.Ticks));
            objectMapper.AddJsonToDocument(json, doc, config);
            return doc;
        }

        public static Filter GetTypeFilter(string type)
        {
            var typeTermQuery = new TermQuery(new Term(FieldType, type));
            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            Filter filter = new QueryWrapperFilter(query);
            return filter;
        }
        public static Filter GetTypeFilter(string type, Query filter)
        {
            var typeTermQuery = new TermQuery(new Term(FieldType, type));
            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            query.Add(filter, Occur.MUST);
            Filter resultFilter = new QueryWrapperFilter(query);
            return resultFilter;
        }

        public static Analyzer GetAnalyser()
        {
            var analyser = new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30);
            return analyser;
        }
    }
}
