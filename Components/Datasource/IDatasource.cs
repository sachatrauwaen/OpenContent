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
        
        IDataItem Get(DataSourceContext context, string id);
        IDataItem GetFirst(DataSourceContext context);
        IDataItems GetAll(DataSourceContext context);
        IDataItems GetAll(DataSourceContext context, Select select);
        IDataItem GetEdit(DataSourceContext context, string id);
        IDataItem GetFirstEdit(DataSourceContext context);
        IDataItem GetVersion(DataSourceContext context, string id, DateTime datetime);
        void Add(DataSourceContext context, JToken data);
        void Update(DataSourceContext context, IDataItem item, JToken data);
        void Delete(DataSourceContext context, IDataItem item);

    }
}