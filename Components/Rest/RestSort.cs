using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Satrabel.OpenContent.Components.Datasource.search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestSort
    {
        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }
       
        [JsonProperty(PropertyName = "desc")]
        public bool Descending { get; set; }
    }
}
