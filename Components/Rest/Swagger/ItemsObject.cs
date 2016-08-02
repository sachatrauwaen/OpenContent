#region Using

using System.Collections.Generic;

#endregion

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    /// A limited subset of JSON-Schema's items object. It is used by parameter definitions that are not located in "body".
    /// </summary>
    public class ItemsObject
    {
        private CollectionFormat? _collectionFormat;

        /// <summary>
        ///     Determines the format of the array if type array is used. Possible values are:
        ///     csv - comma separated values foo, bar.
        ///     ssv - space separated values foo bar.
        ///     tsv - tab separated values foo\tbar.
        ///     pipes - pipe separated values foo|bar.
        ///     Default value is csv.
        /// </summary>
        /// <value>
        ///     The collection format.
        /// </value>
        public CollectionFormat? CollectionFormat
        {
            get
            {
                if(Type == SchemaType.Array && !_collectionFormat.HasValue)
                    _collectionFormat= Satrabel.OpenContent.Components.Rest.Swagger.CollectionFormat.Csv;
                return _collectionFormat;
            }
            set { _collectionFormat = value; }
        }

        /// <summary>
        ///     Required. The type of the parameter. Since the parameter is not located at the request body, it is limited to
        ///     simple types (that is, not an object).
        ///     The value MUST be one of "string", "number", "integer", "boolean", "array" or "file".
        ///     If type is "file", the consumes MUST be either "multipart/form-data", " application/x-www-form-urlencoded" or both
        ///     and the parameter MUST be in "formData".
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public SchemaType? Type { get; set; }

        /// <summary>
        ///     The extending format for the previously mentioned type. See Data Type Formats for further details.
        /// </summary>
        /// <value>
        ///     The format.
        /// </value>
        public string Format { get; set; }

        /// <summary>
        ///     The value of this keyword MUST be an array. This array MUST have at least one element. Elements in the array MUST
        ///     be unique.
        ///     Elements in the array MAY be of any type, including null.
        ///     An instance validates successfully against this keyword if its value is equal to one of the elements in this
        ///     keyword's array value.
        /// </summary>
        /// <value>
        ///     The enum.
        /// </value>
        public List<object> Enum { get; set; }

        /// <summary>
        ///     Required if type is "array". Describes the type of items in the array.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public ItemsObject Items { get; set; }

        /// <summary>
        ///     Declares the value of the item that the server will use if none is provided. (Note: "default" has no meaning for
        ///     required items.)
        ///     See http://json-schema.org/latest/json-schema-validation.html#anchor101.
        ///     Unlike JSON Schema this value MUST conform to the defined type for the data type.
        /// </summary>
        /// <value>
        ///     The default.
        /// </value>
        public object Default { get; set; }

        /// <summary>
        ///     Gets or sets the maximum numeric value.
        /// </summary>
        /// <value>
        ///     The maximum.
        /// </value>
        public double? Maximum { get; set; }

        /// <summary>
        ///     Gets or sets the exclusive maximum.
        /// </summary>
        /// <value>
        ///     The exclusive maximum.
        /// </value>
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>
        ///     Gets or sets the minimum numeric value.
        /// </summary>
        /// <value>
        ///     The minimum.
        /// </value>
        public double? Minimum { get; set; }

        /// <summary>
        ///     Gets or sets the exclusive minimum.
        /// </summary>
        /// <value>
        ///     The exclusive minimum.
        /// </value>
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>
        ///     Gets or sets the maximum length.
        /// </summary>
        /// <value>
        ///     The maximum length.
        /// </value>
        public long? MaxLength { get; set; }

        /// <summary>
        ///     Gets or sets the minimum length.
        /// </summary>
        /// <value>
        ///     The minimum length.
        /// </value>
        public long? MinLength { get; set; }

        /// <summary>
        ///     The value of this keyword MUST be a string.
        ///     This string SHOULD be a valid regular expression, according to the ECMA 262 regular expression dialect.
        ///     A string instance is considered valid if the regular expression matches the instance successfully.
        ///     Recall: regular expressions are not implicitly anchored.
        /// </summary>
        /// <value>
        ///     The pattern.
        /// </value>
        public string Pattern { get; set; }

        /// <summary>
        ///     The value of this keyword MUST be an integer. This integer MUST be greater than, or equal to, 0.
        ///     An array instance is valid against "maxItems" if its size is less than, or equal to, the value of this keyword.
        /// </summary>
        /// <value>
        ///     The maximum items.
        /// </value>
        public long? MaxItems { get; set; }

        /// <summary>
        ///     The value of this keyword MUST be an integer. This integer MUST be greater than, or equal to, 0.
        ///     An array instance is valid against "minItems" if its size is greater than, or equal to, the value of this keyword.
        /// </summary>
        /// <value>
        ///     The minimum items.
        /// </value>
        public long? MinItems { get; set; }

        /// <summary>
        ///     The value of this keyword MUST be a boolean.
        ///     If this keyword has boolean value false, the instance validates successfully.
        ///     If it has boolean value true, the instance validates successfully if all of its elements are unique.
        /// </summary>
        /// <value>
        ///     The true if items should be unique.
        /// </value>
        public bool? UniqueItems { get; set; }
    }
}