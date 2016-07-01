using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public abstract class DefaultDataSource : IDataSource
    {

        public virtual bool Any(DataSourceContext context)
        {
            return GetAll(context, null).Items.Any();
        }

        public virtual IDataItem Get(DataSourceContext context, string id)
        {
            return GetAll(context, null).Items.SingleOrDefault(i => i.Id == id);
        }
        public abstract IDataItems GetAll(DataSourceContext context, search.Select select);

        public virtual JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm();
        }

        public virtual JArray GetVersions(DataSourceContext context, IDataItem item)
        {
            return null;
        }

        public virtual JToken GetVersion(DataSourceContext context, IDataItem item, DateTime datetime)
        {
            return null;
        }

        public abstract void Add(DataSourceContext context, JToken data);

        public abstract void Update(DataSourceContext context, IDataItem item, JToken data);

        public abstract void Delete(DataSourceContext context, IDataItem item);
        public virtual void Action(DataSourceContext context, string action, IDataItem item, JToken data)
        {
            throw new NotImplementedException();
        }

        public abstract string Name { get; }

    }
}