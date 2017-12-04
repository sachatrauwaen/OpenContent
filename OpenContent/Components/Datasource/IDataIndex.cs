using DotNetNuke.Services.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataIndex
    {
        void Reindex(DataSourceContext context);
       
    }
}
