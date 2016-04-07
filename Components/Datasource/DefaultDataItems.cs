using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DefaultDataItems : IDataItems
    {

        public IEnumerable<IDataItem> Items
        {
            get;
            set;
        }

        public int Total
        {
            get;
            set;
        }
    }
}