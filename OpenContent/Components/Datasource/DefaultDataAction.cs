using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataAction : IDataAction
    {
        public string Name {get; set;}
        public string AfterExecute { get; set; }

    }
}