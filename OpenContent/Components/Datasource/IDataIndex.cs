using System.Collections.Generic;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataIndex
    {
        IEnumerable<IIndexableItem> GetIndexableData(DataSourceContext context);
    }
}
