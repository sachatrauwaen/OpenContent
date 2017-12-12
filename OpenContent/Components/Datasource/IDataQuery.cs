using Satrabel.OpenContent.Components.Datasource.Search;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataQuery
    {
        string Name { get; }
        IDataItems GetAll(DataSourceContext context, Select selectQuery);
    }
}
