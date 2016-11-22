namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    ///     The license information.
    /// </summary>
    /// <example>
    ///     {
    ///     "name": "Apache 2.0",
    ///     "url": "http://www.apache.org/licenses/LICENSE-2.0.html"
    ///     }
    /// </example>
    public class License
    {
        /// <summary>
        /// Required. The license name used for the API.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// A URL to the license used for the API. MUST be in the format of a URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }
    }
}