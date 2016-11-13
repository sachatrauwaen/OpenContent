using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataItems : IDataItems
    {
        public DefaultDataItems()
        {

        }
        public DefaultDataItems(IEnumerable<IDataItem> items, int total)
        {
            Items = items;
            Total = total;
        }

        public IEnumerable<IDataItem> Items
        {
            get;
            set;
        }

        public int Total
        {
            get;
            set;
        }
        public string DebugInfo { get; set; }
    }
}