#region Using

using System.Collections.Generic;

#endregion

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    ///     Allows the definition of a security scheme that can be used by the operations. Supported schemes are basic
    ///     authentication, an API key (either as a header or as a query parameter) and OAuth2's common flows (implicit,
    ///     password, application and access code).
    /// </summary>
    public class SecurityDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityDefinition"/> class.
        /// </summary>
        public SecurityDefinition()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityDefinition"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        public SecurityDefinition(string name, SecuritySchemeType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        ///     Required. The type of the security scheme. Valid values are "basic", "apiKey" or "oauth2".
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public SecuritySchemeType Type { get; set; }

        /// <summary>
        ///     Required. The name of the header or query parameter to be used.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Required The location of the API key. Valid values are "query" or "header".
        /// </summary>
        /// <value>
        ///     The in.
        /// </value>
        public Location In { get; set; }

        /// <summary>
        ///     A short description for security scheme.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Required. The flow used by the OAuth2 security scheme. Valid values are "implicit", "password", "application" or
        ///     "accessCode".
        /// </summary>
        /// <value>
        ///     The flow.
        /// </value>
        public string Flow { get; set; }

        /// <summary>
        ///     Required. The authorization URL to be used for this flow. This SHOULD be in the form of a URL.
        /// </summary>
        /// <value>
        ///     The authorization URL.
        /// </value>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        ///     Required. The token URL to be used for this flow. This SHOULD be in the form of a URL.
        /// </summary>
        /// <value>
        ///     The token URL.
        /// </value>
        public string TokenUrl { get; set; }

        /// <summary>
        ///     Required. The available scopes for the OAuth2 security scheme.
        /// </summary>
        /// <value>
        ///     Maps between a name of a scope to a short description of it (as the value of the property).
        /// </value>
        public Dictionary<string, string> Scopes { get; set; }
    }
}