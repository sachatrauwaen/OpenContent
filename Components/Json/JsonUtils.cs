using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using DotNetNuke.Instrumentation;

namespace Satrabel.OpenContent.Components.Json
{
    public static class JsonUtils
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JsonUtils));

        public static JObject ToJObject(this FileUri file)
        {
            try
            {
                if (!file.FileExists) return null;
                string fileContent = File.ReadAllText(file.PhysicalFilePath);
                if (string.IsNullOrWhiteSpace(fileContent)) return null;
                return JObject.Parse(fileContent);
            }
            catch (Exception ex)
            {
                string mess = string.Format("Error while parsing file [{0}]", file.FilePath);
                Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }
        public static JObject ToJObject(this string text, string desc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text)) return null;
                return JObject.Parse(text);
            }
            catch (Exception ex)
            {
                string mess = string.Format("Error while parsing [{0}]", desc);
                Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
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
            foreach (var child in o.Children<JProperty>())
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
    }
}