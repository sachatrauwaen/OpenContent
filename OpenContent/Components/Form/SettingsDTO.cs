using System.Collections.Generic;

namespace Satrabel.OpenContent.Components.Form
{
    public class SettingsDTO
    {
        public List<NotificationDTO> Notifications { get; set; }
        public GenSettingsDTO Settings { get; set; }
    }
}