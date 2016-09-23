using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    public class SwaggerRoot
    {
        public SwaggerRoot(){
            Swagger  = "2.0";
            Info = new Info();
            Paths = new Dictionary<string, PathItem>();
            //Definitions = new Dictionary<string, SchemaObject>();
            Definitions = new Dictionary<string, JToken>();
            Consumes = new List<string>();
            Produces = new List<string>();
            Tags = new List<Tag>();
        }
        public string Swagger { get; set; }

        public Info Info { get; set; } 

        public string Host { get; set; }

        public string BasePath { get; set; }

        public List<Protocol> Schemes { get; set; }

        public List<string> Consumes { get; set; }

        public List<string> Produces { get; set; }

        public Dictionary<string, PathItem> Paths { get; set; }

        //public Dictionary<string, SchemaObject> Definitions { get; set; }
        public Dictionary<string, JToken> Definitions { get; set; }

        public List<Parameter> Parameters { get; set; }

        public Dictionary<string, Response> Responses { get; set; }

        public Dictionary<string, SecurityDefinition> SecurityDefinitions { get; set; }

       public List<SecurityRequirement> Security { get; set; }

        public List<Tag> Tags { get; set; }

        public string BaseUrl { get; set; }

        public ExternalDoc ExternalDocs { get; set; }
   
    }
}