using System;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataItem : IDataItem
    {
        public DefaultDataItem(string id)
        {
            Id = id;
            Key = id;
        }

        public DefaultDataItem(string id, string key)
        {
            Id = id;
            Key = key;
        }

        public DefaultDataItem(JToken json)
        {
            Id = null;
            Key = null;
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