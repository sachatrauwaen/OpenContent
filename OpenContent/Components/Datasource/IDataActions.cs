using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataActions
    {
        List<IDataAction> GetActions(DataSourceContext context, IDataItem item);
    }

    
}
