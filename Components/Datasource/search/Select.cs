using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.Datasource.Search
{
    public class Select
    {
        public Select()
        {
            Filter = new FilterGroup();
            Query = new FilterGroup();
            Sort = new List<SortRule>();
        }
        public FilterGroup Filter { get; private set; }
        public FilterGroup Query { get; private set; }
        public List<SortRule> Sort { get; private set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public bool IsQueryEmpty
        {
            get
            {
                return !Query.FilterRules.Any() && !Query.FilterGroups.Any();
            }
        }

    }
}