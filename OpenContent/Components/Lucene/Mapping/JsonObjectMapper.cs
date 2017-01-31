using System;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using DotNetNuke.Common.Utilities;
using Lucene.Net.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene.Config;
using Lucene.Net.QueryParsers;

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
        private static readonly JsonSerializer serializer = new JsonSerializer()
        {
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        #endregion

        #region Implementation

        /// <summary>
        /// Adds the given source object to the specified Document.
        /// </summary>
        /// <param name="source">
        /// The source object to add.
        /// </param>
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
                AddArray(doc, prefix, token as JArray, fieldconfig == null ? null : fieldconfig.Items);
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
                                doc.Add(new Field(prefix, HtmlUtils.Clean(value.Value.ToString(), true), Field.Store.NO, Field.Index.ANALYZED));
                            }
                            if (sort)
                            {
                                doc.Add(new Field("@" + prefix, HtmlUtils.Clean(Truncate(value.Value.ToString(), 100), true), Field.Store.NO, Field.Index.NOT_ANALYZED));
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
                if (field != null && field.Fields != null && field.Fields.ContainsKey(property.Name))
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
            if (!string.IsNullOrEmpty(prefix))
            {
                return string.Format("{0}.{1}", prefix, add);
            }
            else
            {
                return add.ToString();
            }
        }

        #endregion

    }
}
