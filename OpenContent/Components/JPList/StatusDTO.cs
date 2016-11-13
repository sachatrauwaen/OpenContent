namespace Satrabel.OpenContent.Components.JPList
{
    public class StatusDTO
    {
        /// <summary>
        /// jplist action: paging, filter, sort, etc.
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// jplist control name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// jplist control type: drop-down, textbox, etc.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// status related data
        /// </summary>
        public StatusDataDTO data { get; set; }
    }
}
