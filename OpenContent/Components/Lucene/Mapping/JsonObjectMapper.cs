using System;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using System.Web;
using Lucene.Net.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Lucene.Net.QueryParsers;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene.Mapping
{
    /// <summary>
    /// Implements the IObjectMapper interface using a JSON serialization to identify all the properties and nested
    /// objects to map/add to a Lucene.Net Document. This mapper doesn't store any of the fields on the document but
    /// instead just adds them to be indexed as appropriate.
    /// </summary>
    public sealed class JsonObjectMapper
    {
        #region Fields

        /// <summary>
        /// The JsonSerializer to use.
        /// </summary>
        private static readonly JsonSerializer Serializer = new JsonSerializer()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        #endregion

        #region Implementation

        /// <summary>
        /// Adds the given source object to the specified Document.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="doc">
        /// The Document to add the object to.
        /// </param>
        /// <param name="config"></param>
        public void AddJsonToDocument(JToken json, Document doc, FieldConfig config)
        {
            if (json == null || json.IsEmpty()) return;

            Add(doc, null, json, config);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the given JToken to the specified Document.
        /// </summary>
        /// <param name="doc">
        /// The Document to add to.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use for field names.
        /// </param>
        /// <param name="token">
        /// The JToken to add.
        /// </param>
        /// <param name="fieldconfig"></param>
        private static void Add(Document doc, string prefix, JToken token, FieldConfig fieldconfig)
        {
            if (token is JObject)
            {
                AddProperties(doc, prefix, token as JObject, fieldconfig);
            }
            else if (token is JArray)
            {
                AddArray(doc, prefix, token as JArray, fieldconfig?.Items);
            }
            else if (token is JValue)
            {
                JValue value = token as JValue;
                bool index = false;
                bool sort = false;
                if (fieldconfig != null)
                {
                    index = fieldconfig.Index;
                    sort = fieldconfig.Sort;
                }

                switch (value.Type) //todo: simple date gets detected as string 
                {
                    case JTokenType.Boolean:
                        if (index || sort)
                        {
                            doc.Add(new NumericField(prefix, Field.Store.NO, true).SetIntValue((bool)value.Value ? 1 : 0));
                        }
                        break;

                    case JTokenType.Date:
                        if (index || sort)
                        {
                            doc.Add(new NumericField(prefix, Field.Store.NO, true).SetLongValue(((DateTime)value.Value).Ticks));

                            //doc.Add(new Field(prefix, DateTools.DateToString((DateTime)value.Value, DateTools.Resolution.SECOND), Field.Store.NO, Field.Index.NOT_ANALYZED));

                            /*
                            if (field != null ){
                                if (field.IndexType == "datetime")
                                {
                                    doc.Add(new Field(prefix, DateTools.DateToString((DateTime)value.Value, DateTools.Resolution.SECOND), Field.Store.NO, Field.Index.NOT_ANALYZED));
                                }
                                else if (field.IndexType == "date")
                                {
                                    doc.Add(new Field(prefix, DateTools.DateToString((DateTime)value.Value, DateTools.Resolution.DAY), Field.Store.NO, Field.Index.NOT_ANALYZED));
                                }
                                else if (field.IndexType == "time")
                                {
                                    doc.Add(new Field(prefix, DateTools.DateToString((DateTime)value.Value, DateTools.Resolution.SECOND).Substring(8), Field.Store.NO, Field.Index.NOT_ANALYZED));
                                }
                            }
                            else
                            {
                                doc.Add(new Field(prefix, DateTools.DateToString((DateTime)value.Value, DateTools.Resolution.SECOND), Field.Store.NO, Field.Index.NOT_ANALYZED));
                            }
                            */
                        }
                        break;

                    case JTokenType.Float:
                        if (index || sort)
                        {
                            if (value.Value is float)
                            {
                                doc.Add(new NumericField(prefix, Field.Store.NO, true).SetFloatValue((float)value.Value));
                            }
                            else
                            {
                                doc.Add(new NumericField(prefix, Field.Store.NO, true).SetFloatValue((float)Convert.ToDouble(value.Value)));
                                //doc.Add(new NumericField(prefix, Field.Store.NO, true).SetDoubleValue(Convert.ToDouble(value.Value)));
                            }
                        }
                        break;

                    case JTokenType.Guid:
                        if (index || sort)
                        {
                            doc.Add(new Field(prefix, value.Value.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
                        }
                        break;

                    case JTokenType.Integer:
                        if (index || sort)
                        {
                            doc.Add(new NumericField(prefix, Field.Store.NO, true).SetFloatValue((float)Convert.ToInt64(value.Value)));
                            //doc.Add(new NumericField(prefix, Field.Store.NO, true).SetLongValue(Convert.ToInt64(value.Value)));
                        }
                        break;

                    case JTokenType.Null:
                        break;

                    case JTokenType.String:

                        if (fieldconfig != null && fieldconfig.IndexType == "key")
                        {
                            doc.Add(new Field(prefix, QueryParser.Escape(value.Value.ToString()), Field.Store.NO, Field.Index.NOT_ANALYZED));
                        }
                        else if (fieldconfig != null && fieldconfig.IndexType == "html")
                        {
                            if (index)
                            {
                                doc.Add(new Field(prefix, CleanHtml(value.Value.ToString(), true), Field.Store.NO, Field.Index.ANALYZED));
                            }
                            if (sort)
                            {
                                doc.Add(new Field("@" + prefix, CleanHtml(Truncate(value.Value.ToString(), 100), true), Field.Store.NO, Field.Index.NOT_ANALYZED));
                            }
                        }
                        else
                        {
                            if (index)
                            {
                                doc.Add(new Field(prefix, value.Value.ToString(), Field.Store.NO, Field.Index.ANALYZED));
                            }
                            if (sort)
                            {
                                doc.Add(new Field("@" + prefix, Truncate(value.Value.ToString(), 100), Field.Store.NO, Field.Index.NOT_ANALYZED));
                            }
                        }
                        break;

                    case JTokenType.TimeSpan:
                        if (index || sort)
                        {
                            doc.Add(new NumericField(prefix, Field.Store.NO, true).SetLongValue(((TimeSpan)value.Value).Ticks));
                        }
                        break;

                    case JTokenType.Uri:
                        if (index || sort)
                        {
                            doc.Add(new Field(prefix, value.Value.ToString(), Field.Store.NO, Field.Index.ANALYZED));
                        }
                        break;

                    default:
                        Debug.Fail("Unsupported JValue type: " + value.Type);
                        break;
                }
            }
            else
            {
                Debug.Fail("Unsupported JToken: " + token);
            }
        }

        public static string Truncate(string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        /// <summary>
        /// Adds the properties of the given JObject to the specified Document.
        /// </summary>
        /// <param name="doc">
        /// The Document to add the properties to.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use for field names.
        /// </param>
        /// <param name="obj">
        /// The JObject to add.
        /// </param>
        private static void AddProperties(Document doc, string prefix, JObject obj, FieldConfig field)
        {
            foreach (JProperty property in obj.Properties())
            {
                FieldConfig f = null;
                if (field?.Fields != null && field.Fields.ContainsKey(property.Name))
                {
                    f = field.Fields[property.Name];
                }
                else if (field != null && field.MultiLanguage)
                {
                    f = field;
                }
                Add(doc, MakePrefix(prefix, property.Name), property.Value, f);
            }
        }

        /// <summary>
        /// Adds the elements of the given JArray to the specified Document.
        /// </summary>
        /// <param name="doc">
        /// The Document to add the elements to.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use for field names.
        /// </param>
        /// <param name="array">
        /// The JArray to add.
        /// </param>
        private static void AddArray(Document doc, string prefix, JArray array, FieldConfig item)
        {
            for (int i = 0; i < array.Count; i++)
            {
                Add(doc, prefix, array[i], item);
            }
        }

        /// <summary>
        /// Makes a prefix for field names.
        /// </summary>
        /// <typeparam name="TAdd">
        /// The Type of the last part to add to the prefix.
        /// </typeparam>
        /// <param name="prefix">
        /// The existing prefix to extend.
        /// </param>
        /// <param name="add">
        /// The part to add to the prefix.
        /// </param>
        /// <returns>
        /// A string that can be used as prefix for field names.
        /// </returns>
        private static string MakePrefix<TAdd>(string prefix, TAdd add)
        {
            return !string.IsNullOrEmpty(prefix) ? $"{prefix}.{add}" : add.ToString();
        }

        #region CleanHtml helpers

        private static readonly Regex StripWhiteSpaceRegex = new Regex("\\s+", RegexOptions.Compiled);
        private static readonly Regex StripTagsRegex = new Regex("<[^>]*>", RegexOptions.Compiled);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clean removes any HTML Tags, Entities (and optionally any punctuation) from
        /// a string
        /// </summary>
        /// <remarks>
        /// Encoded Tags are getting decoded, as they are part of the content!
        /// </remarks>
        /// <param name="html">The Html to clean</param>
        /// <param name="removePunctuation">A flag indicating whether to remove punctuation</param>
        /// <returns>The cleaned up string</returns>
        /// <history>
        ///		[cnurse]	11/16/2004	created
        ///     [galatrash] 05/31/2013  added fix for double html-encoding
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string CleanHtml(string html, bool removePunctuation)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            if (html.Contains("&lt;"))
            {
                // Fix when it is a double-encoded document
                html = HttpUtility.HtmlDecode(html);
            }

            //First remove any HTML Tags ("<....>")
            html = StripTags(html, true);

            //Second replace any HTML entities (&nbsp; &lt; etc) through their char symbol
            html = HttpUtility.HtmlDecode(html);

            //Thirdly remove any punctuation
            if (removePunctuation)
            {
                html = StripPunctuation(html, true);
                // When RemovePunctuation is false, HtmlDecode() would have already had removed these
                //Finally remove extra whitespace
                html = StripWhiteSpace(html, true);
            }

            return html;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StripTags removes the HTML Tags from the content
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="html">The HTML content to clean up</param>
        /// <param name="retainSpace">Indicates whether to replace the Tag by a space (true) or nothing (false)</param>
        /// <returns>The cleaned up string</returns>
        /// <history>
        ///		[cnurse]	11/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string StripTags(string html, bool retainSpace)
        {
            string repString = retainSpace ? " " : "";
            return StripTagsRegex.Replace(html, repString);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StripWhiteSpace removes the WhiteSpace from the content
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="html">The HTML content to clean up</param>
        /// <param name="retainSpace">Indicates whether to replace the WhiteSpace by a space (true) or nothing (false)</param>
        /// <returns>The cleaned up string</returns>
        /// <history>
        ///		[cnurse]	12/13/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string StripWhiteSpace(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            return StripWhiteSpaceRegex.Replace(html, retainSpace ? " " : "");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StripPunctuation removes the Punctuation from the content
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="html">The HTML content to clean up</param>
        /// <param name="retainSpace">Indicates whether to replace the Punctuation by a space (true) or nothing (false)</param>
        /// <returns>The cleaned up string</returns>
        /// <history>
        ///		[cnurse]	11/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        private static string StripPunctuation(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            //Create Regular Expression objects
            const string PUNCTUATION_MATCH = "[~!#\\$%\\^&*\\(\\)-+=\\{\\[\\}\\]\\|;:\\x22'<,>\\.\\?\\\\\\t\\r\\v\\f\\n]";
            var afterRegEx = new Regex(PUNCTUATION_MATCH + "\\s");
            var beforeRegEx = new Regex("\\s" + PUNCTUATION_MATCH);

            //Define return string
            string retHtml = html + " "; //Make sure any punctuation at the end of the String is removed

            //Set up Replacement String
            var repString = retainSpace ? " " : "";
            while (beforeRegEx.IsMatch(retHtml))
            {
                retHtml = beforeRegEx.Replace(retHtml, repString);
            }
            while (afterRegEx.IsMatch(retHtml))
            {
                retHtml = afterRegEx.Replace(retHtml, repString);
            }
            // Return modified string after trimming leading and ending quotation marks
            return retHtml.Trim('"');
        }

        #endregion

        #endregion

    }
}
