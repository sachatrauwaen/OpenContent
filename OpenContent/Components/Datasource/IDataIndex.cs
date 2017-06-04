using System.Collections.Generic;
using Satrabel.OpenContent.Components.Indexing;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataIndex
    {
        IEnumerable<IIndexableItem> GetIndexableData(DataSourceContext context);
    }
}
