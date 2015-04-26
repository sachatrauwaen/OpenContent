using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Json
{
    public class JsonUtils
    {
        public static dynamic JsonToDynamic(string json){
            var dynamicObject = System.Web.Helpers.Json.Decode(json);
            return dynamicObject;
        }
    }
}