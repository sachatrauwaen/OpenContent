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
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name of the Datasource is a unique identifier.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the specified item of a list datasource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        IDataItem Get(DataSourceContext context, string id);
        /// <summary>
        /// Gets the item of a non-list datasource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IDataItem GetFirst(DataSourceContext context);
        /// <summary>
        /// Gets all items of a list datasource
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IDataItems GetAll(DataSourceContext context);
        /// <summary>
        /// Gets items of a list datasource based on a query, 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="select">The select.</param>
        /// <returns></returns>
        IDataItems GetAll(DataSourceContext context, Select select);

        IDataItem GetEdit(DataSourceContext context, string id);
        IDataItem GetFirstEdit(DataSourceContext context);
        IDataItem GetVersion(DataSourceContext context, string id, DateTime datetime);
        void Add(DataSourceContext context, JToken data);
        void Update(DataSourceContext context, IDataItem item, JToken data);
        void Delete(DataSourceContext context, IDataItem item);

    }
}