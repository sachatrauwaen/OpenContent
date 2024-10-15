using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.Nl;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Lucene.Config;
using System.IO;

namespace Satrabel.OpenContent.Components.Lucene.Mapping
{
    public static class JsonMappingUtils
    {
        #region Consts

        /// <summary>
        /// The name of the field which holds the type.
        /// </summary>
        public const string FIELD_TYPE = "$type";

        /// <summary>
        /// The name of the field which holds the JSON-serialized source of the object.
        /// </summary>
        public const string FIELD_SOURCE = "$source";

        /// <summary>
        /// The name of the field which holds the timestamp when the document was created.
        /// </summary>
        public const string FIELD_TIMESTAMP = "$timestamp";

        public const string FIELD_ID = "$id";
        public const string FIELD_USER_ID = "$userid";
        public const string FIELD_CREATED_ON_DATE = "$createdondate";

        #endregion

        public static Document JsonToDocument(string type, string id, string userId, DateTime createdOnDate, JToken json, string source, FieldConfig config, bool storeSource = false)
        {
            var objectMapper = new JsonObjectMapper();
            Document doc = new Document();

            doc.Add(new Field(FIELD_TYPE, type, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FIELD_ID, id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(FIELD_USER_ID, userId, Field.Store.NO, Field.Index.NOT_ANALYZED));
            if (storeSource)
            {
                doc.Add(new Field(FIELD_SOURCE, source, Field.Store.YES, Field.Index.NO));
            }
            doc.Add(new NumericField(FIELD_TIMESTAMP, Field.Store.YES, true).SetLongValue(DateTime.UtcNow.Ticks));
            doc.Add(new NumericField(FIELD_CREATED_ON_DATE, Field.Store.NO, true).SetLongValue(createdOnDate.Ticks));
            CorrectSortIndexData(json);
            objectMapper.AddJsonToDocument(json, doc, config);
            return doc;
        }

        public static Filter GetTypeFilter(string type)
        {
            var typeTermQuery = CreateTypeQuery(type);
            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            Filter filter = new QueryWrapperFilter(query);
            return filter;
        }
        public static Filter GetTypeFilter(string type, Query filter)
        {
            var typeTermQuery = CreateTypeQuery(type);
            BooleanQuery query = new BooleanQuery();
            query.Add(typeTermQuery, Occur.MUST);
            query.Add(filter, Occur.MUST);
            Filter resultFilter = new QueryWrapperFilter(query);
            return resultFilter;
        }

        public static Analyzer GetAnalyser(string cultureCode = "")
        {
            if (!string.IsNullOrEmpty(cultureCode))
            {
                if (cultureCode.StartsWith("fr"))
                    return new FrenchAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30);
                else if (cultureCode.StartsWith("nl"))
                    return new DutchAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30);
            }
            return new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30);
        }

        public static TermQuery CreateTypeQuery(string type)
        {
            return new TermQuery(new Term(FIELD_TYPE, type));
        }

        /// <summary>
        /// Corrects the SortIndex data in the JSON object.
        /// This method was originally written to address issues with the SortIndex,
        /// where non-integer values were being stored, leading to indexing problems.
        /// 
        /// The method attempts to convert string values to integers and removes invalid values.
        /// 
        /// Currently, this method is disabled (return statement at the beginning) because the underlying
        /// cause has been resolved elsewhere in the code. However, we retain this method for future
        /// debugging purposes, should similar issues arise again.
        /// </summary>
        /// <param name="json">The JToken object containing the JSON data to be corrected.</param>
        private static void CorrectSortIndexData(JToken json)
        {
            return; 
            if (json["SortIndex"].Type == JTokenType.Object)
            {
                foreach (var prop in json["SortIndex"].Children<JProperty>())
                {
                    if (prop.Value.Type != JTokenType.Integer)
                    {
                        int intValue;
                        if (int.TryParse(prop.Value.ToString(), out intValue))
                        {
                            prop.Value = JToken.FromObject(intValue);
                        }
                        else
                        {
                            // Als we de waarde niet kunnen parsen, verwijderen we het property
                            prop.Remove();
                        }
                    }
                }
            }
            else
            {
                if (json["SortIndex"].Type != JTokenType.Integer)
                {
                    int intValue;
                    if (int.TryParse(json["SortIndex"].ToString(), out intValue))
                    {
                        json["SortIndex"] = JToken.FromObject(intValue);
                    }
                    else
                    {
                        // Als we de waarde niet kunnen parsen, verwijderen we het hele SortIndex veld
                        json["SortIndex"].Parent.Remove();
                    }
                }
            }
        }
    }

    public class ASCIIFoldingAnalyzer : Analyzer
    {
        private readonly Analyzer subAnalyzer;

        public ASCIIFoldingAnalyzer(Analyzer subAnalyzer)
        {
            this.subAnalyzer = subAnalyzer;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var result = subAnalyzer.TokenStream(fieldName, reader);
            result = new ASCIIFoldingFilter(result);
            return result;
        }
    }
}
