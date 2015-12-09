using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public class FieldDTO
    {
        public string type { get; set; }
        public bool index { get; set; }
        public bool sort { get; set; }

        public Dictionary<string, FieldDTO> fields { get; set; }
        public ItemDTO item { get; set; }

    }
}
