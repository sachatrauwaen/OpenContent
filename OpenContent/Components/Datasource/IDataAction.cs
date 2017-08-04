using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Datasource
{
    public interface IDataAction
    {
        string Name { get; }
        string AfterExecute { get; }
    }
}
