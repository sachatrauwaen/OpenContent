using System;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.JPList
{
    public class FilterDTO
    {
        public FilterDTO()
        {
            Names = new List<string>();
            ExactSearchMultiValue = new List<string>();
        }

        public string Name
        {
            get
            {
                return string.Join(",", Names.ToArray());
            }
            set
            {
                Names = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            } 
        }
        public List<string> Names { get; set; }

        public string WildCardSearchValue { get; set; }
        
        public string ExactSearchValue { get; set; }

        public List<string> ExactSearchMultiValue { get; set; }
    }
}
