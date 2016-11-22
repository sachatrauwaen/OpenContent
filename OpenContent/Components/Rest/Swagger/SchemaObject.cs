using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    public class SchemaObject
    {
        [JsonProperty("$ref")]
        public string Ref { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public object Default { get; set; }
        public double? MultipleOf { get; set; }
        public double? Maximum { get; set; }
        public bool? ExclusiveMaximum { get; set; }
        public double? Minimum { get; set; }
        public bool? ExclusiveMinimum { get; set; }
        public long? MaxLength { get; set; }
        public long? MinLength { get; set; }
        public string Pattern { get; set; }
        public long? MaxItems { get; set; }
        public long? MinItems { get; set; }
        public bool? UniqueItems { get; set; }
        public long? MaxProperties { get; set; }
        public long? MinProperties { get; set; }
        public List<string> Required { get; set; }
        public List<object> Enum { get; set; }
        public SchemaObject Items { get; set; }
        //public List<SchemaObject> Items { get; set; }
        public List<SchemaObject> AllOf { get; set; }
        public Dictionary<string, SchemaObject> Properties { get; set; }
        //public JToken Properties { get; set; }
        public SchemaObject AdditionalProperties { get; set; }
        [JsonIgnore]
        public Uri Id { get; set; }
        public SchemaType? Type { get; set; }
    }
}