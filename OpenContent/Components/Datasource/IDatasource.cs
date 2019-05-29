using Newtonsoft.Json.Linq;
using System;
using Satrabel.OpenContent.Components.Datasource.Search;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataSource
    {
        /// <summary>
        /// Gets the name of the resource that this dataprovider is handling.
        /// </summary>
        /// <value>
        /// The name of the Datasource is a unique identifier.
        /// </value>
        string Name { get; }

        #region Queries

        /// <summary>
        /// It there any data present.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        bool Any(DataSourceContext context);

        /// <summary>
        /// Gets the specified item of a list datasource.
        /// Needed to go to the detail page.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="id">The identifier. (null for single item template)</param>
        /// <returns></returns>
        IDataItem Get(DataSourceContext context, string id);

        /// <summary>
        /// Gets items of a list datasource based on a query, 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="selectQuery">The select. (if null must return all data)</param>
        /// <returns></returns>
        IDataItems GetAll(DataSourceContext context, Select selectQuery);

        /// <summary>
        /// Gets Alpaca Schema, Options and View json, 
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="schema">Must return schema information</param>
        /// <param name="options">Must return options information</param>
        /// <param name="view">Must return view information</param>
        /// <returns>Alpaca json object { schema: ..., options : ..., view: ...}  </returns>
        JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view);

        JArray GetVersions(DataSourceContext context, IDataItem item);
        JToken GetVersion(DataSourceContext context, IDataItem item, DateTime datetime);

        #region AdditionalData

        /// <summary>
        /// Gets additional data of a datasource.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scope">The Scope. (portal, tab, module, tabmodule)</param>
        /// <param name="key">The unique keyin the scope</param>
        /// <returns></returns>
        IDataItem GetData(DataSourceContext context, string scope, string key);

        /// <summary>
        /// Gets Alpaca Schema, Options and View json for Additional data
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="schema">Must return schema information</param>
        /// <param name="options">Must return options information</param>
        /// <param name="view">Must return view information</param>
        /// <param name="key"></param>
        /// <returns>Alpaca json object { schema: ..., options : ..., view: ...}  </returns>
        JObject GetDataAlpaca(DataSourceContext context, bool schema, bool options, bool view, string key);

        JToken GetDataVersions(DataSourceContext context, IDataItem item);

        JToken GetDataVersion(DataSourceContext context, IDataItem item, DateTime datetime);
        #endregion

        #endregion

        #region Commands

        void Add(DataSourceContext context, JToken data);
        void Update(DataSourceContext context, IDataItem item, JToken data);
        void Delete(DataSourceContext context, IDataItem item);
        JToken Action(DataSourceContext context, string action, IDataItem item, JToken data);

        #region AdditionalData
        void AddData(DataSourceContext context, string scope, string key, JToken data);
        void UpdateData(DataSourceContext context, IDataItem item, JToken data);
        #endregion

        #endregion







    }
}