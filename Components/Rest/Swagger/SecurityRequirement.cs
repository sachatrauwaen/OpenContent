
using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    /// Swagger security Requirement
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{string, string[]}" />
    public class SecurityRequirement : Dictionary<string, string[]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityRequirement"/> class.
        /// </summary>
        public SecurityRequirement()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityRequirement"/> class.
        /// </summary>
        /// <param name="defaultKey">The default key.</param>
        public SecurityRequirement(string defaultKey)
        {
            Add(defaultKey,new string[0]);
        }
    }
}