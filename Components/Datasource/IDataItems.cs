using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataItems
    {
        IEnumerable<IDataItem> Items { get; set; }
        int Total { get; set; }
        string DebugInfo { get; set; }
    }
}
