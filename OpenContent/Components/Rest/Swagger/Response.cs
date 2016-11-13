#region Using

using System.Collections.Generic;

#endregion

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    ///     Describes a single response from an API Operation.
    /// </summary>
    /// <example>
    ///     {
    ///     "description": "A complex object array response",
    ///     "schema": {
    ///     "type": "array",
    ///     "items": {
    ///     "$ref": "#/definitions/VeryComplexType"
    ///     }
    ///     }
    ///     }
    /// </example>
    public class Response
    {
        /// <summary>
        ///     Required. A short description of the response. GFM syntax can be used for rich text representation.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     A definition of the response structure.
        ///     It can be a primitive, an array or an object. If this field does not exist, it means no content is returned as part
        ///     of the response.
        ///     As an extension to the Schema Object, its root type value may also be "file".
        ///     This SHOULD be accompanied by a relevant produces mime-type.
        /// </summary>
        /// <value>
        ///     The schema.
        /// </value>
        public SchemaObject Schema { get; set; }

        /// <summary>
        ///     A list of headers that are sent with the response.
        /// </summary>
        /// <value>
        ///     The headers.
        /// </value>
        public Dictionary<string, SchemaObject> Headers { get; set; }
    }
}