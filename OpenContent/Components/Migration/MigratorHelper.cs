using System.Web.Services.Description;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Migration
{
    public static class MigratorHelper
    {
        public static JToken Image2ToImageX(JToken input)
        {
            // create ImageX JToken, but also copy the original image to the new image location
            JToken output = input;
            return output;
        }
    }
}