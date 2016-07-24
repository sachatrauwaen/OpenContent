using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    public class SecurityScheme
    {
        public SecurityScheme(){
            Scopes = new Dictionary<string, string>();
        }

        public SecuritySchemeType Type { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public Location In { get; set; }

        public OAuth2Flow Flow { get; set; }

        public string AuthorizationUrl { get; set; }

        public string TokenUrl { get; set; }

        public Dictionary<string, string> Scopes { get; set; } 
    }
}