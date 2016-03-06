using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.JPList
{
    public class FilterDTO
    {
        public FilterDTO()
        {
            names = new List<string>();
            pathGroup = new List<string>();
        }
        public string name { 
            get
            {
                return string.Join(",", names.ToArray());
            }
            set{
                names = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            } 
        }
        public List<string> names { get; set; }
        public string value { get; set; }
        
        public string path { get; set; }

        public List<string> pathGroup { get; set; }
    }
}
