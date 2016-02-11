using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Satrabel.OpenContent.Components.Manifest;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Solution
{
    public class Solution
    {
        public Manifest.Manifest Manifest { get; set; }
        public JObject Schema { get; set; }
        public JObject Options { get; set; }

        public List<TemplateSolution> Templates { get; set; }

    }
}