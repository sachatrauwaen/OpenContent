using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataItem : IDataItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public JToken Data { get; set; }
        public int CreatedByUserId { get; set; }
        public object Item { get; set; }
    }
}