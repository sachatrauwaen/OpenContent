using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Mapping;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Lucene.Net.Mapping
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

        public static Document JsonToDocument(string type, string id, string source, bool StoreSource = true)
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
            Filter filter = new QueryWrapperFilter(new TermQuery(new Term(FieldType, type)));
            //filter.AddTerm(new Term(FieldType, type));
            return filter;
        }

    }
}
