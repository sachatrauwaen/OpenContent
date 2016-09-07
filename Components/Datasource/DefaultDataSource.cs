using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using System;
using System.Linq;

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

        public virtual IDataItem GetData(DataSourceContext context, string scope, string key)
        {
            throw new NotImplementedException();
        }

        public virtual JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            return fb.BuildForm();
        }
        public virtual JObject GetDataAlpaca(DataSourceContext context, bool schema, bool options, bool view, string key)
        {
            throw new NotImplementedException();
        }

        public virtual JArray GetVersions(DataSourceContext context, IDataItem item)
        {
            return null;
        }

        public virtual JToken GetVersion(DataSourceContext context, IDataItem item, DateTime datetime)
        {
            return null;
        }
        public virtual JToken GetDataVersions(DataSourceContext context, IDataItem item)
        {
            throw new NotImplementedException();
        }

        public virtual JToken GetDataVersion(DataSourceContext context, IDataItem item, DateTime datetime)
        {
            throw new NotImplementedException();
        }
        public abstract void Add(DataSourceContext context, JToken data);

        public abstract void Update(DataSourceContext context, IDataItem item, JToken data);

        public abstract void Delete(DataSourceContext context, IDataItem item);
        public virtual JToken Action(DataSourceContext context, string action, IDataItem item, JToken data)
        {
            throw new NotImplementedException();
        }
        public virtual void AddData(DataSourceContext context, string scope, string key, JToken data)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateData(DataSourceContext context, IDataItem item, JToken data)
        {
            throw new NotImplementedException();
        }

        public abstract string Name { get; }






        

       

       
    }
}