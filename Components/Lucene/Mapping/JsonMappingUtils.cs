using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene.Config;
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

        public static Document JsonToDocument(string type, string id, string source, FieldConfig config, bool StoreSource = false)
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
            ObjectMapper.AddJsonToDocument(source, doc, config);
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
        public static Filter GetTypeFilter(string type, Query Filter)
        {
            var typeTermQuery = new TermQuery(new Term(FieldType, type));
            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            query.Add(Filter, Occur.MUST);
            Filter filter = new QueryWrapperFilter(query);
            return filter;
        }

        public static Analyzer GetAnalyser()
        {
            var analyser = new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30);
            return analyser;
            /*
            var analyzerList = new List<KeyValuePair<string, Analyzer>>
            {
                //new KeyValuePair<string, Analyzer>("PortalId", new KeywordAnalyzer()),
                //new KeyValuePair<string, Analyzer>("FileId", new KeywordAnalyzer()),
                new KeyValuePair<string, Analyzer>("Title", new SimpleAnalyzer()),
                //new KeyValuePair<string, Analyzer>("FileName", new SimpleAnalyzer()),
                new KeyValuePair<string, Analyzer>("Description", new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30)),
                //new KeyValuePair<string, Analyzer>("FileContent", new StandardAnalyzer(Version.LUCENE_30)),
                //new KeyValuePair<string, Analyzer>("Folder", new LowercaseKeywordAnalyzer()),
                new KeyValuePair<string, Analyzer>("Category", new KeywordAnalyzer())
            };
            return new PerFieldAnalyzerWrapper(new KeywordAnalyzer(), analyzerList);
            */
            
        }
    }
}
