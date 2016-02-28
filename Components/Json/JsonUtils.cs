using System.Linq;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.TemplateHelpers;
using DotNetNuke.Services.FileSystem;


namespace Satrabel.OpenContent.Components.Json
{
    public static class JsonUtils
    {
        public static bool IsJson(this string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData)) return false;
            return jsonData.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }

        public static JToken LoadJsonFromFile(string filename)
        {
            return new FileUri(filename).ToJObject();
        }

        public static dynamic JsonToDynamic(string json)
        {
            var dynamicObject = System.Web.Helpers.Json.Decode(json);
            return dynamicObject;
        }

        public static string SimplifyJson(string json, string culture)
        {
            JObject obj = JObject.Parse(json);
            SimplifyJson(obj, culture);
            return obj.ToString();
        }
        public static void SimplifyJson(JObject o, string culture)
        {
            foreach (var child in o.Children<JProperty>().ToList())
            {
                var childProperty = child;
                var array = childProperty.Value as JArray;
                if (array != null)
                {

                    foreach (var value in array)
                    {
                        var obj = value as JObject;
                        if (obj != null)
                        {
                            SimplifyJson(obj, culture);
                        }
                    }
                }
                else
                {
                    var obj = childProperty.Value as JObject;
                    if (obj != null)
                    {
                        bool languages = obj.Children<JProperty>().Any(v => v.Name.Length == 5 && v.Name.Substring(2, 1) == "-");
                        if (languages)
                        {
                            var cultureToken = obj[culture];
                            if (cultureToken != null)
                            {
                                childProperty.Value = cultureToken;
                            }
                            else
                            {
                                childProperty.Value = new JValue("");
                            }
                        }
                        else
                        {
                            SimplifyJson(obj, culture);
                        }
                    }
                }
            }
        }
        public static void LookupJson(JObject o, JObject additionalData, JObject options)
        {
            foreach (var child in o.Children<JProperty>().ToList())
            {
                JObject opt = null;
                if (options != null && options["fields"] != null)
                {
                    opt = options["fields"][child.Name] as JObject;
                }
                bool lookup = opt != null && 
                    opt["type"] != null &&
                    opt["type"].ToString() == "select2" &&
                    opt["dataService"] != null &&
                    opt["dataService"]["data"] != null &&
                    opt["dataService"]["data"]["dataKey"] != null;

                string dataKey = "";
                string dataMember = "";
                string valueField = "Id";
                if (lookup)
                {
                    dataKey = opt["dataService"]["data"]["dataKey"].ToString();
                    dataMember = opt["dataService"]["data"]["dataMember"] == null ? "" : opt["dataService"]["data"]["dataMember"].ToString();
                    valueField = opt["dataService"]["data"]["valueField"] == null ? "Id" : opt["dataService"]["data"]["valueField"].ToString();
                }
                              
                var childProperty = child;

                if (childProperty.Value is JArray)
                {
                    var array = childProperty.Value as JArray;
                    JArray newArray = new JArray();
                    foreach (var value in array)
                    {
                        var obj = value as JObject;
                        if (obj != null)
                        {
                            LookupJson(obj, additionalData, opt["items"] as JObject);
                        }
                        else if (lookup)
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    newArray.Add(GenerateObject(additionalData, dataKey, val.ToString(), dataMember, valueField));
                                }
                                catch (System.Exception)
                                {
                                }
                            }
                        }
                    }
                    if (lookup)
                    {
                        childProperty.Value = newArray;
                    }
                }
                else if (childProperty.Value is JObject)
                {
                    var obj = childProperty.Value as JObject;
                    LookupJson(obj, additionalData, opt);
                }
                else if (childProperty.Value is JValue)
                {
                    if (lookup)
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {
                            o[childProperty.Name] = GenerateObject(additionalData, dataKey, val, dataMember, valueField);
                        }
                        catch (System.Exception)
                        {
                        }
                    }
                }
            }
        }

        public static void ImagesJson(JObject o, JObject requestOptions, JObject options)
        {
            foreach (var child in o.Children<JProperty>().ToList())
            {
                JObject opt = null;
                if (options != null && options["fields"] != null)
                {
                    opt = options["fields"][child.Name] as JObject;
                }
                JObject reqOpt = null;
                if (requestOptions != null && requestOptions["fields"] != null)
                {
                    reqOpt = requestOptions["fields"][child.Name] as JObject;
                }

                bool image = opt != null &&
                    opt["type"] != null && opt["type"].ToString() == "image2";

                
                if (image && reqOpt != null)
                {
                                       
                    
                }

                var childProperty = child;

                if (childProperty.Value is JArray)
                {
                    var array = childProperty.Value as JArray;
                    JArray newArray = new JArray();
                    foreach (var value in array)
                    {
                        var obj = value as JObject;
                        if (obj != null)
                        {
                            LookupJson(obj, reqOpt, opt["items"] as JObject);
                        }
                        else if (image)
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    newArray.Add(GenerateImage(reqOpt, val.ToString()));
                                }
                                catch (System.Exception)
                                {
                                }
                            }
                        }
                    }
                    if (image)
                    {
                        childProperty.Value = newArray;
                    }
                }
                else if (childProperty.Value is JObject)
                {
                    var obj = childProperty.Value as JObject;
                    LookupJson(obj, reqOpt, opt);
                }
                else if (childProperty.Value is JValue)
                {
                    if (image)
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {
                            //o[childProperty.Name] = GenerateObject(additionalData, dataKey, val, dataMember, valueField);
                            o[childProperty.Name] = GenerateImage(reqOpt, val);
                        }
                        catch (System.Exception)
                        {
                        }
                    }
                }
            }
        }

        private static JToken GenerateImage(JObject reqOpt, string p)
        {
            var ratio = new Ratio(100, 100);
            if (reqOpt != null && reqOpt["ratio"] != null)
            {
                ratio = new Ratio(reqOpt["ratio"].ToString());
            }
            int fileId = int.Parse(p);
            var file = FileManager.Instance.GetFile(fileId);
            var imageUrl = ImageHelper.GetImageUrl(file, ratio);
            return new JValue(imageUrl);
        }

        private static JObject GenerateObject(JObject additionalData, string key, string id, string dataMember, string valueField)
        {
            var json = additionalData[key];
            if (!string.IsNullOrEmpty(dataMember))
            {
                json = json[dataMember];
            }
            JArray array = json as JArray;
            if (array != null)
            {
                foreach (var obj in array)
                {
                    var objid = obj[valueField].ToString();
                    if (id.Equals(objid))
                    {
                        return obj as JObject;
                    }
                }
            }
            JObject res = new JObject();
            res["Id"] = id;
            res["Title"] = "unknow";
            return res;
        }

        public static void SimplifyJson(JToken o, string culture)
        {

            var array = o as JArray;
            if (array != null)
            {
                foreach (var value in array)
                {
                    var obj = value as JObject;
                    if (obj != null)
                    {
                        SimplifyJson(obj, culture);
                    }
                }
            }
            else
            {
                var obj = o as JObject;
                if (obj != null)
                {
                    SimplifyJson(obj, culture);
                }
            }
        }

        public static void Merge(JObject model, JObject completeModel)
        {
            foreach (var prop in completeModel.Properties())
            {
                model[prop.Name] = prop.Value;
            }
        }
    }
}