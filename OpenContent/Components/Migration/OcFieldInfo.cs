using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Migration
{
    public class OcFieldInfo
    {
        public OcFieldInfo(JObject schema, JObject options)
        {
            Schema = schema;
            Options = options;
        }

        public JObject Schema { get; }
        public JObject Options { get; }

        public string Type
        {
            get
            {
                return Options["type"].ToString();
            }
        }
    }
}