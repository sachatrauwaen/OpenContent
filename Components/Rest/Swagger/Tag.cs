namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    ///     Allows adding meta data to a single tag that is used by the Operation Object. It is not mandatory to have a Tag
    ///     Object per tag used there.
    /// </summary>
    /// <example>
    ///     {
    ///     "name": "pet",
    ///     "description": "Pets operations"
    ///     }
    /// </example>
    public class Tag
    {
        /// <summary>
        ///     Required. The name of the tag.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        ///     A short description for the tag. GFM syntax can be used for rich text representation.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public virtual string Description { get; set; }

        /// <summary>
        ///     Additional external documentation for this tag.
        /// </summary>
        /// <value>
        ///     The external docs.
        /// </value>
        public ExternalDoc ExternalDocs { get; set; }
    }
}