using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.TemplateHelpers;
using DotNetNuke.Services.FileSystem;
using Satrabel.OpenContent.Components.Datasource;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Files;

namespace Satrabel.OpenContent.Components.Json
{
    public static class JsonUtils
    {
        public static bool IsJson(this string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData)) return false;
            return jsonData.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }

        [Obsolete("This method is obsolete since aug 2017; use LoadJsonFromCacheOrDisk() or consider using LoadJsonFileFromCacheOrDisk() instead")]
        public static JToken LoadJsonFromFile(string filename)
        {
            return LoadJsonFromCacheOrDisk(new FileUri(filename));
        }

        [Obsolete("This method is obsolete since aug 2017; use LoadJsonFileFromDisk() or consider using LoadJsonFileFromCacheOrDisk() instead")]
        public static JObject GetJsonFromFile(string filename)
        {
            return LoadJsonFileFromDisk(filename) as JObject;
        }

        public static T LoadJsonFileFromCacheOrDisk<T>(FileUri file)
        {
            try
            {
                T jsonObject = default(T);
                if (file.FileExists)
                {
                    string cacheKey = file.FilePath;
                    jsonObject = App.Services.CacheAdapter.GetCache<T>(cacheKey);
                    if (jsonObject == null)
                    {
                        string content = File.ReadAllText(file.PhysicalFilePath);
                        jsonObject = JsonConvert.DeserializeObject<T>(content);
                        App.Services.CacheAdapter.SetCache(cacheKey, jsonObject, file.PhysicalFilePath);
                    }
                }
                return jsonObject;
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Failed to load json file {file.FilePath}. Error: {ex}");
                throw new Exception($"Failed to load file {file.FilePath}. See log for more info.", ex);
            }
        }

        /// <summary>
        /// Tries to load Json file from cache.
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        public static JToken LoadJsonFromCacheOrDisk(FileUri fileUri)
        {
            string cacheKey = fileUri.FilePath;
            var json = App.Services.CacheAdapter.GetCache<JObject>(cacheKey);
            if (json == null)
            {
                var fileContent = FileUriUtils.ReadFileFromDisk(fileUri);
                json = fileContent.ToJObject($"file [{fileUri.FilePath}]") as JObject;
                if (json != null)
                {
                    App.Services.CacheAdapter.SetCache(cacheKey, json, fileUri.PhysicalFilePath);
                }
            }
            return json;
        }

        public static JToken LoadJsonFileFromDisk(string filename)
        {
            if (!File.Exists(filename)) return null;

            JToken json = null;
            string fileContent = File.ReadAllText(filename);
            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                json = fileContent.ToJObject($"file [{filename}]") as JObject;
            }
            return json;
        }

        public static dynamic JsonToDynamic(string json)
        {
            var dynamicObject = System.Web.Helpers.Json.Decode(json);
            return dynamicObject;
        }

        public static Dictionary<string, object> JsonToDictionary(string json)
        {
            var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            if(json.Length >= jsSerializer.MaxJsonLength )
            {
                //jsSerializer.MaxJsonLength = jsSerializer.MaxJsonLength + 40000; //temp fix
                throw new Exception($"Too much data to deserialize. Please use a client side template to circumvent that.");
            }
            // next line fails with large amount of data (>4MB). Use a client side template to fix that.
            Dictionary<string, object> model = (Dictionary<string, object>)jsSerializer.DeserializeObject(json); 
            return model;
            //return ToDictionaryNoCase(model);
        }

        private static DictionaryNoCase ToDictionaryNoCase(Dictionary<string, object> dic)
        {
            var newDic = new DictionaryNoCase();
            foreach (KeyValuePair<string, object> entry in dic)
            {
                newDic.Add(entry.Key, ConvertObject(entry.Value));
            }
            return newDic;
        }
        private static object ConvertObject(object obj)
        {
            if (obj is string)
            {
                return obj;
            }
            else if (obj is Dictionary<string, object>)
            {
                return ToDictionaryNoCase((Dictionary<string, object>)obj);
            }
            else if (obj is IEnumerable)
            {
                var arr = (IEnumerable)obj;
                var newArr = new List<object>();
                foreach (var item in arr)
                {
                    newArr.Add(ConvertObject(item));
                }
                return newArr.ToArray();
            }
            else
            {
                return obj;
            }
        }

        //public static string SimplifyJson(string json, string culture)
        //{
        //    JObject obj = JObject.Parse(json);
        //    SimplifyJson(obj, culture);
        //    return obj.ToString();
        //}

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

        public static void LookupJson(JObject o, JObject additionalData, JObject schema, JObject options, bool includelabels, List<string> includes, Func<string, string, JObject> objFromCollection, Func<string, JObject> alpacaForAddData, string prefix = "")
        {
            if (schema?["properties"] == null)
                return;
            if (options?["fields"] == null)
                return;

            foreach (var child in o.Children<JProperty>().ToList())
            {
                JObject sch = null;
                JObject opt = null;

                sch = schema["properties"][child.Name] as JObject;
                if (sch == null) continue;

                opt = options["fields"][child.Name] as JObject;
                if (opt == null) continue;

                // additionalData enhancement
                bool lookup =
                    opt["type"] != null &&
                    opt["type"].ToString() == "select2" &&
                    opt["dataService"]?["data"]?["dataKey"] != null;

                string dataKey = "";
                string dataMember = "";
                string valueField = "Id";
                string childrenField = "Id";
                if (lookup)
                {
                    dataKey = opt["dataService"]["data"]["dataKey"].ToString();
                    dataMember = opt["dataService"]["data"]["dataMember"]?.ToString() ?? "";
                    valueField = opt["dataService"]["data"]["valueField"]?.ToString() ?? "Id";
                    childrenField = opt["dataService"]["data"]["childrenField"]?.ToString() ?? "";
                }

                // collections enhancement
                string field = (string.IsNullOrEmpty(prefix) ? child.Name : prefix + "." + child.Name);
                bool include = includes != null && includes.Contains(field);
                string collection = opt["dataService"]?["data"]?["collection"] != null ? opt["dataService"]?["data"]?["collection"].ToString() : "";

                // enum enhancement
                var enums = (sch["enum"] as JArray)?.Select(l => l.ToString()).ToArray();
                var labels = (opt["optionLabels"] as JArray)?.Select(l => l.ToString()).ToArray();

                var childProperty = child;
                if (childProperty.Value is JArray)
                {
                    var array = childProperty.Value as JArray;
                    var newArray = new JArray();
                    foreach (var value in array)
                    {
                        var obj = value as JObject;
                        if (obj != null)
                        {
                            LookupJson(obj, additionalData, sch["items"] as JObject, opt["items"] as JObject, includelabels, includes, objFromCollection, alpacaForAddData, field);
                        }
                        else if (lookup)
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    var genObj = GenerateObject(additionalData, dataKey, val.ToString(), dataMember, valueField, childrenField);
                                    var alpaca = alpacaForAddData(dataKey);
                                    LookupJson(genObj, additionalData, alpaca["schema"]?["items"] as JObject, alpaca["options"]?["items"] as JObject, includelabels, includes, objFromCollection, alpacaForAddData, field);
                                    newArray.Add(genObj);
                                }
                                catch (System.Exception)
                                {
                                }
                            }
                        }
                        else if (include && !string.IsNullOrEmpty(collection))
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    newArray.Add(objFromCollection(collection, val.ToString()));
                                }
                                catch (System.Exception)
                                {
                                    Debugger.Break();
                                }
                            }
                        }
                        else if (includelabels && enums != null && labels != null)
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    int idx = Array.IndexOf(enums, val.ToString());
                                    var enumObj = new JObject();
                                    enumObj["value"] = val.ToString();
                                    enumObj["label"] = labels[idx];
                                    newArray.Add(enumObj);
                                }
                                catch (System.Exception)
                                {
                                    Debugger.Break();
                                }
                            }
                        }
                    }

                    if (lookup || (include && !string.IsNullOrEmpty(collection)) || (includelabels && enums != null && labels != null))
                    {
                        childProperty.Value = newArray;
                    }
                }
                else if (childProperty.Value is JObject)
                {
                    var obj = childProperty.Value as JObject;
                    LookupJson(obj, additionalData, sch, opt, includelabels, includes, objFromCollection, alpacaForAddData, field);
                }
                else if (childProperty.Value is JValue)
                {
                    if (lookup)
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {
                            var obj = GenerateObject(additionalData, dataKey, val, dataMember, valueField, childrenField);
                            var alpaca = alpacaForAddData(dataKey);
                            LookupJson(obj, additionalData, alpaca["schema"]?["items"] as JObject, alpaca["options"]?["items"] as JObject, includelabels, includes, objFromCollection, alpacaForAddData, field);

                            //LookupJson(obj, additionalData, sch, opt, includelabels, includes, objFromCollection, alpacaForAddData, field);
                            o[childProperty.Name] = obj;
                        }
                        catch (System.Exception)
                        {
                            Debugger.Break();
                        }
                    }
                    else if (include && !string.IsNullOrEmpty(collection))
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {
                            o[childProperty.Name] = objFromCollection(collection, val);
                        }
                        catch (System.Exception)
                        {
                            Debugger.Break();
                        }
                    }
                    else if (includelabels && enums != null && labels != null)
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {
                            int idx = Array.IndexOf(enums, val);
                            var enumObj = new JObject();
                            enumObj["value"] = val;
                            enumObj["label"] = labels[idx];
                            o[childProperty.Name] = enumObj;
                        }
                        catch (System.Exception)
                        {
                            Debugger.Break();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enhance data for all alpaca fields of type 'image2' and 'mlimage2'
        /// </summary>
        public static void ImagesJson(JObject o, JObject requestOptions, JObject options, bool isEditable)
        {
            foreach (var child in o.Children<JProperty>().ToList())
            {
                JObject opt = null;
                if (options?["fields"] != null)
                {
                    opt = options["fields"][child.Name] as JObject;
                }
                JObject reqOpt = null;
                if (requestOptions?["fields"] != null)
                {
                    reqOpt = requestOptions["fields"][child.Name] as JObject;
                }

                bool image = (opt?["type"]).EqualsAny("image2", "mlimage2");

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
                        if (obj != null && opt != null && opt["items"] != null)
                        {
                            ImagesJson(obj, reqOpt, opt["items"] as JObject, isEditable);
                        }
                        else if (image)
                        {
                            var val = value as JValue;
                            if (val != null)
                            {
                                try
                                {
                                    newArray.Add(GenerateImage(reqOpt, val.ToString(), isEditable));
                                }
                                catch (Exception)
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
                    if (obj != null && opt != null)
                    {
                        ImagesJson(obj, reqOpt, opt, isEditable);
                    }
                }
                else if (childProperty.Value is JValue)
                {
                    if (image)
                    {
                        string val = childProperty.Value.ToString();
                        try
                        {
                            o[childProperty.Name] = GenerateImage(reqOpt, val, isEditable);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private static JToken GenerateImage(JObject reqOpt, string p, bool isEditable)
        {
            var ratio = new Ratio(100, 100);
            if (reqOpt?["ratio"] != null)
            {
                ratio = new Ratio(reqOpt["ratio"].ToString());
            }
            int fileId = int.Parse(p);
            IFileInfo file = FileManager.Instance.GetFile(fileId);
            var imageUrl = ImageHelper.GetImageUrl(file, ratio);
            var editUrl = isEditable ? GetFileEditUrl(file) : "";

            var obj = new JObject();
            obj["ImageId"] = fileId;
            obj["ImageUrl"] = imageUrl;
            if (isEditable)
                obj["EditUrl"] = editUrl;
            return obj;
            //return new JValue(imageUrl);
        }
        private static string GetFileEditUrl(IFileInfo f)
        {
            if (f == null) return "";
            var portalFileUri = new PortalFileUri(f);
            return portalFileUri.EditUrl();
        }



        private static JObject GenerateObject(JObject additionalData, string key, string id, string dataMember, string valueField, string childerenField)
        {

            var json = additionalData[key];
            if (json == null)
            {
                json = additionalData[key.ToLowerInvariant()];
            }
            if (!string.IsNullOrEmpty(dataMember))
            {
                json = json[dataMember];
            }
            var res = GetObjectInHiearchy(json, id, valueField, childerenField);
            /*
            JArray array = json as JArray;
            if (array != null)
            {
                foreach (var obj in array)
                {
                    var objid = obj[valueField].ToString();
                    if (id.Equals(objid))
                    {
                        return obj as JObject;
                    } else if (!string.IsNullOrEmpty(childerenField))
                    {
                        var childerenjson = obj[childerenField] as JArray;
                        if(childerenjson != null)
                        {

                        }

                    }

                }
            }
            */
            if (res == null)
            {
                res = new JObject();
                res["Id"] = id;
                res["Title"] = "unknow";
            }
            return res;
        }

        private static JObject GetObjectInHiearchy(JToken json, string id, string valueField, string childrenField)
        {
            JArray array = json as JArray;
            if (array != null)
            {
                foreach (var obj in array)
                {
                    var objid = obj[valueField]?.ToString();
                    if (objid != null && id.Equals(objid))
                    {
                        return obj as JObject;
                    }
                    else if (!string.IsNullOrEmpty(childrenField))
                    {
                        var childerenjson = obj[childrenField] as JArray;
                        if (childerenjson != null)
                        {
                            var childerenRes = GetObjectInHiearchy(childerenjson, id, valueField, childrenField);
                            if (childerenRes != null) return childerenRes;
                        }
                    }
                }
            }
            return null;
        }

        public static void Merge(JObject model, JObject completeModel)
        {
            foreach (var prop in completeModel.Properties())
            {
                model[prop.Name] = prop.Value;
            }
        }

        internal static void IdJson(JToken o)
        {
            var array = o as JArray;
            if (array != null)
            {
                foreach (var value in array)
                {
                    var obj = value as JObject;
                    if (obj != null)
                    {
                        if (obj["id"] == null)
                        {
                            obj["id"] = Guid.NewGuid().ToString();
                        }
                        IdJson(obj);
                    }
                }
            }
            else
            {
                var obj = o as JObject;
                if (obj != null)
                {
                    if (obj["id"] == null)
                    {
                        obj["id"] = Guid.NewGuid().ToString();
                    }
                    foreach (var child in o.Children<JProperty>().ToList())
                    {
                        IdJson(child.Value);
                    }
                }
            }
        }

        public static void RemoveType(JToken o)
        {
            var array = o as JArray;
            if (array != null)
            {
                foreach (var value in array)
                {
                    var obj = value as JObject;
                    if (obj != null)
                    {
                        if (obj["__type"] != null)
                        {
                            obj.Remove("__type");
                        }
                        RemoveType(obj);
                    }
                }
            }
            else
            {
                var obj = o as JObject;
                if (obj != null)
                {
                    if (obj["__type"] != null)
                    {
                        obj.Remove("__type");
                    }
                    foreach (var child in o.Children<JProperty>().ToList())
                    {
                        RemoveType(child.Value);
                    }
                }
            }
        }
    }
    class DictionaryNoCase : Dictionary<string, object>
    {
        public DictionaryNoCase() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
        public DictionaryNoCase(IDictionary<string, object> dic) : base(dic, StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}