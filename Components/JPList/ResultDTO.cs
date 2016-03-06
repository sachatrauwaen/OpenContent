using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Satrabel.OpenContent.Components.JPList
{
    class ResultDTO
    {
        public JToken data { get; set; }

        public int count { get; set; }

    }
}
