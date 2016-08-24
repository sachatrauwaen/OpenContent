using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataItems : IDataItems
    {

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