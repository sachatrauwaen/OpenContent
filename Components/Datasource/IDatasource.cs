using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource.search;
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
        IDataItems GetAll(DataSourceContext context);
        IDataItems GetAll(DataSourceContext context, Select select);
        void AddContent(DataSourceContext context, JObject data);
        void UpdateContent(DataSourceContext context, IDataItem item, JObject data);
    }
}