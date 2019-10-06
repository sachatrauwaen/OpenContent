using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataActions
    {
        List<IDataAction> GetActions(DataSourceContext context, IDataItem item);
    }

    
}
