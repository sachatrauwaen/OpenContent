using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataSource
    {
        string Name { get; }
        IDataItem GetEdit(DataSourceContext context, string id);
        IDataItem Get(DataSourceContext context, string id);
        IDataItem GetFirst(DataSourceContext context);
        IEnumerable<IDataItem> GetAll(DataSourceContext context);
        void AddContent(DataSourceContext context, JObject data);
        void UpdateContent(DataSourceContext context, IDataItem item, JObject data);
    }
}