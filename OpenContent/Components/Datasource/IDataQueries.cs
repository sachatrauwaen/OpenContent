using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataQueries
    {
        List<IDataQuery> GetQueries(DataSourceContext context);
    }

    
}
