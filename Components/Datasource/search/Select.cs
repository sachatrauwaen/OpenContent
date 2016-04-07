using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class Select
    {
        public Select()
        {
            Filter = new FilterGroup();
            Query = new FilterGroup();
            Sort = new List<SortRule>();
        }
        public FilterGroup Filter { get; set; }
        public FilterGroup Query { get; set; }
        public List<SortRule> Sort { get; private set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        
    }
}