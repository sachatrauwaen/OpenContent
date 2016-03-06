using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Lucene
{
    public class SearchResults
    {
        public SearchResults()
        {
            ids = new string[0];
        }
        public int TotalResults { get; set; }
        public string[] ids { get; set; }
    }
}