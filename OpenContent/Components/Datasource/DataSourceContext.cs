using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DataSourceContext
    {
        public DataSourceContext()
        {
        }

        public string TemplateFolder { get; set; }
        public int PortalId { get; set; }
        public int UserId { get; set; } // Only Used for Add and Update commands
        public string CurrentCultureCode { get; set; }
        public bool Index { get; set; }
        /// <summary>
        /// Datasource Config coming from the manifest. To initialize the DataSource.
        /// </summary>
        public JObject Config { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DataSourceContext"/> is a single item.
        /// </summary>
        public bool Single { get; set; }

        /// <summary>
        /// Options comming from the requester (data on the webapi call). Command or Query specific config.
        /// </summary>
        public JObject Options { get; set; }

        /// <summary>
        /// Gets or sets the active module identifier  of the View Module.
        /// </summary>
        /// <value>
        /// The active module identifier.
        /// </value>
        public int ActiveModuleId { get; set; }
        /// <summary>
        /// Gets or sets the module identifier of the Data Module.
        /// </summary>
        /// <value>
        /// The module identifier.
        /// </value>
        public int ModuleId { get; set; }
        /// <summary>
        /// Gets or sets the tab identifier of the View Module.
        /// </summary>
        /// <value>
        /// The tab identifier.
        /// </value>
        public int TabId { get; set; }
        public int TabModuleId { get; set; }

        /// <summary>
        /// Gets or sets the agent, which is a string uniquely identifying the calling agent.
        /// </summary>
        public string Agent { get; set; }
        public string Collection { get; set; }

        /// <summary>
        /// Return value of Add method of Data Source Provider (workaroud because interface dont have return value)
        /// </summary>
        public string Id { get; set; }
    }
}