using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource.search
{
    public class SortRule
    {
        public string Field { get; set; }
        public FieldTypeEnum FieldType { get; set; }
        public bool Descending { get; set; }
        
    }
}