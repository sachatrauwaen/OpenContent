using Newtonsoft.Json.Linq;
using System;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentVersion
    {
        public JToken Json { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}
