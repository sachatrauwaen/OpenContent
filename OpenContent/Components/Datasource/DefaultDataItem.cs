using System;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataItem : IDataItem
    {
        public DefaultDataItem()
        {
        }

        public DefaultDataItem(JToken json)
        {
            Data = json;
        }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Collection { get; set; }
        public string Title { get; set; }
        public JToken Data { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public object Item { get; set; }
        
    }
}