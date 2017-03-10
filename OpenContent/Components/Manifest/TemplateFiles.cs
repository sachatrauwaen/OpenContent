using System.Collections.Generic;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class TemplateFiles
    {
        public TemplateFiles()
        {
            DataInTemplate = true;
        }
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }

        [JsonProperty(PropertyName = "partialTemplates")]
        public Dictionary<string, PartialTemplate> PartialTemplates { get; set; }

        [JsonProperty(PropertyName = "schemaInTemplate")]
        public bool SchemaInTemplate { get; set; }

        [JsonProperty(PropertyName = "optionsInTemplate")]
        public bool OptionsInTemplate { get; set; }
        [JsonProperty(PropertyName = "additionalDataInTemplate")]
        public bool AdditionalDataInTemplate { get; set; }
        [JsonProperty(PropertyName = "dataInTemplate")]
        public bool DataInTemplate { get; set; }
        [JsonProperty(PropertyName = "labelsInTemplate")]
        public bool LabelsInTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [DNN search].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [DNN search]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "dnnsearch")]
        public bool DnnSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Detail URL to an item is used in [DNN search] results.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [DNN search]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "useDetailUrlInDnnSearchResults")]
        public bool UseDetailUrlInDnnSearchResults { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the field to use for document Title in [DNN search] results.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [DNN search]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty(PropertyName = "titleFieldForDnnSearch")]
        public string TitleFieldForDnnSearch { get; set; }

        [JsonProperty(PropertyName = "model")]
        public Dictionary<string, CollectionManifest> Model { get; set; }
    }
}