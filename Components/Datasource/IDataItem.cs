using System;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataItem
    {
        string Id { get; set; }
        string Title { get; set; }
        JToken Data { get; set; }
        int CreatedByUserId { get; set; }
        DateTime CreatedOnDate { get; set; }
        object Item { get; set; }

    }
}
