using System.Collections.Generic;
using Satrabel.OpenContent.Components.Lucene.Config;
﻿using DotNetNuke.Services.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataIndex
    {
        IEnumerable<IIndexableItem> GetIndexableData(DataSourceContext context);
       // void Reindex(DataSourceContext context);
    }
}
