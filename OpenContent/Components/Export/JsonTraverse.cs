using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Export
{
    public class JsonTraverse
    {
        public static JToken Traverse(JToken data, JObject schema, JObject options, Func<JToken, JObject, JObject, JToken> callback)
        {
            var json = callback(data, schema, options);
            if (json is JArray)
            {
                JObject sch = schema?["items"] as JObject;
                JObject opt = options?["items"] as JObject;
                var array = json as JArray;
                var newArray = new JArray();
                foreach (var arrayItem in array)
                {
                    var res = Traverse(arrayItem, sch, opt, callback);
                    newArray.Add(res);
                }
                json = newArray;
            }
            else if (json is JObject)
            {
                var obj = json as JObject;
                foreach (var child in json.Children<JProperty>().ToList())
                {
                    var sch = schema?["properties"]?[child.Name] as JObject;
                    var opt = options?["fields"]?[child.Name] as JObject;
                    child.Value = Traverse(child.Value, sch, opt, callback);
                }
            }
            else if (json is JValue)
            {
            }
            return json;
        }


        public static void Traverse(JToken data, JObject schema, JObject options, Action<JToken, JObject, JObject> callback)
        {
            callback(data, schema, options);
            if (data is JArray)
            {
                JObject sch = schema?["items"] as JObject;
                JObject opt = options?["items"] as JObject;
                var array = data as JArray;
                foreach (var arrayItem in array)
                {
                    Traverse(arrayItem, sch, opt, callback);
                }
            }
            else if (data is JObject)
            {
                var obj = data as JObject;
                foreach (var child in data.Children<JProperty>().ToList())
                {
                    var sch = schema?["properties"]?[child.Name] as JObject;
                    var opt = options?["fields"]?[child.Name] as JObject;
                    Traverse(child.Value, sch, opt, callback);
                }
            }
            else if (data is JValue)
            {
            }
        }
    }
}