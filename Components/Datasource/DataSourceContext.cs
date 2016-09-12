using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DataSourceContext
    {
        public int ModuleId { get; set; }
        public int ActiveModuleId { get; set; }
        public string TemplateFolder { get; set; }
        public int PortalId { get; set; }        
        public int UserId { get; set; } // Only Used for Add and Update commands
        public bool Index { get; set; }
        public JObject Config { get; set; }
        public bool Single { get; set; }
        public JObject Options { get; set; }

        public int TabId { get; set; }

        public int TabModuleId { get; set; }
    }
}