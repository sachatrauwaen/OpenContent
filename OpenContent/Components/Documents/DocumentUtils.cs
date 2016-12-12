using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Documents
{
    public class DocumentUtils
    {
        public static CollectionKey GetCollectionKey(string colkey)
        {
            var items = colkey.Split('/');
            return new CollectionKey()
            {
                Collection = items.Length > 0 ? items[0] : "",
                Key = items.Length > 1 ? items[1] : "",
            };
        }

    }

    public class CollectionKey
    {
        public string Collection { get; set; }
        public string Key { get; set; }
    }

}