using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Lucene.Mapping
{
    public class JsonMappingUtils
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

        #endregion

        public static Document JsonToDocument(string type, string id, string source, bool StoreSource = false)
        {
            var ObjectMapper = new JsonObjectMapper();
            Document doc = new Document();
            string json = source;  //JsonConvert.SerializeObject(source, typeof(TSource), settings);
            doc.Add(new Field(FieldType, type, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FieldId, id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            if (StoreSource)
            {
                doc.Add(new Field(FieldSource, json, Field.Store.YES, Field.Index.NO));
            }
            doc.Add(new NumericField(FieldTimestamp, Field.Store.YES, true).SetLongValue(DateTime.UtcNow.Ticks));
            ObjectMapper.AddJsonToDocument(source, doc);
            return doc;
        }

        public static Filter GetTypeFilter(string type)
        {
            var typeTermQuery = new TermQuery(new Term(FieldType, type));
            
            //var analyzer = new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30);
            //var parser = new QueryParser(global::Lucene.Net.Util.Version.LUCENE_30, "Category", analyzer);
            //var xquery = parser.Parse("Category3");

            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            //query.Add(xquery, Occur.MUST);


            Filter filter = new QueryWrapperFilter(query);
            //filter.AddTerm(new Term(FieldType, type));
            return filter;
        }

        public static Filter GetTypeFilter(string type, Query Filter)
        {
            var typeTermQuery = new TermQuery(new Term(FieldType, type));
            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            query.Add(Filter, Occur.MUST);
            Filter filter = new QueryWrapperFilter(query);
            return filter;
        }

    }
}
