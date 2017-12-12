using System.Collections;

namespace Satrabel.OpenContent.Components
{
    public class ComponentSettingsInfo
    {
        public static ComponentSettingsInfo Create(IDictionary moduleSettings)
        {
            var retval = new ComponentSettingsInfo()
            {
                Template = moduleSettings["template"] as string,    //templatepath+file  or  //manifestpath+key
                Data = moduleSettings["data"] as string,
                Query = moduleSettings["query"] as string,
            };

            //normalize TabId & ModuleId
            var sTabId = moduleSettings["tabid"] as string;
            var sModuleId = moduleSettings["moduleid"] as string;
            retval.TabId = -1;
            retval.ModuleId = -1;
            if (sTabId != null && sModuleId != null)
            {
                retval.TabId = int.Parse(sTabId);
                retval.ModuleId = int.Parse(sModuleId);
            }

            //normalize DetailTabId
            var sDetailTabId = moduleSettings["detailtabid"] as string;
            retval.DetailTabId = -1;
            if (!string.IsNullOrEmpty(sDetailTabId))
            {
                retval.DetailTabId = int.Parse(sDetailTabId);
            }
            return retval;
        }

        public int DetailTabId { get; set; }

        public string Query { get; set; }

        public string Data { get; set; }

        public int ModuleId { get; set; }

        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets the template.
        /// Format:  templatepath+file  or  manifestpath+key
        /// </summary>
        public string Template { get; set; }
    }
}