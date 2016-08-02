using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    /// <summary>
    /// Describes the operations available on a single path. A Path Item may be empty, due to ACL constraints. 
    /// The path itself is still exposed to the documentation viewer but they will not know which operations and parameters are available.
    /// </summary>
    /// <example>{
    ///  "get": {
    ///    "description": "Returns pets based on ID",
    ///    "summary": "Find pets by ID",
    ///    "operationId": "getPetsById",
    ///    "produces": [
    ///      "application/json",
    ///      "text/html"
    ///    ],
    ///    "responses": {
    ///      "200": {
    ///        "description": "pet response",
    ///        "schema": {
    ///          "type": "array",
    ///          "items": {
    ///            "$ref": "#/definitions/Pet"
    ///          }
    ///        }
    ///      },
    ///      "default": {
    ///        "description": "error payload",
    ///        "schema": {
    ///          "$ref": "#/definitions/ErrorModel"
    ///        }
    ///      }
    ///    }
    ///  },
    ///  "parameters": [
    ///    {
    ///      "name": "id",
    ///      "in": "path",
    ///      "description": "ID of pet to use",
    ///      "required": true,
    ///      "type": "array",
    ///      "items": {
    ///        "type": "string"
    ///      },
    ///      "collectionFormat": "csv"
    ///    }
    ///  ]
    ///}</example>
    public class PathItem
    {
        public Operation Get { get; set; }
        public Operation Put { get; set; }
        public Operation Post { get; set; }
        public Operation Delete { get; set; }
        public Operation Options { get; set; }
        public Operation Head { get; set; }
        public Operation Patch { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}