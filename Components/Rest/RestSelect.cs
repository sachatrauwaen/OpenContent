using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Rest
{
    public class RestSelect
    {
        public RestSelect()
        {
            //Filter = new FilterGroup();
            Query = new RestGroup();
            Sort = new List<RestSort>();
        }
        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { get; set; }
        [JsonProperty(PropertyName = "pageIndex")]
        public int PageIndex { get; set; }
        [JsonProperty(PropertyName = "query")]
        public RestGroup Query { get; set; }
        [JsonProperty(PropertyName = "sort")]
        public List<RestSort> Sort { get; set; }
    }
}