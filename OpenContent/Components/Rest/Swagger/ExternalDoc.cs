namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    ///     Allows referencing an external resource for extended documentation.
    /// </summary>
    /// <example>
    ///     {
    ///     "description": "Find more info here",
    ///     "url": "https://swagger.io"
    ///     }
    /// </example>
    public class ExternalDoc
    {
        /// <summary>
        ///     short description of the target documentation. GFM syntax can be used for rich text representation.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Required. The URL for the target documentation. Value MUST be in the format of a URL.
        /// </summary>
        /// <value>
        ///     The URL.
        /// </value>
        public string Url { get; set; }
    }
}