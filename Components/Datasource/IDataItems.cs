using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataItems
    {
        IEnumerable<IDataItem> Items { get; set; }
        int Total { get; set; }
        string DebugInfo { get; set; }
    }
}
