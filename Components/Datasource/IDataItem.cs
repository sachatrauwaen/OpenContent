using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataItem
    {
        string Id { get; set; }
        JToken Data { get; set; }
        int CreatedByUserId { get; set; }
        object Item { get; set; }
    }
}
