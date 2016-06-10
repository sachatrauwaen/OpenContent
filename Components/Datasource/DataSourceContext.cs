using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DataSourceContext
    {
        public int ModuleId { get; set; }
        public string TemplateFolder { get; set; }
        public int PortalId { get; set; }
        public int UserId { get; set; }
        public bool Index { get; set; }
        public JObject Config { get; set; }

        public bool Single { get; set; }

        public JObject Options { get; set; }
    }
}