using DotNetNuke.Instrumentation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace Satrabel.OpenContent.Components.Json
{
    public static class JsonExtensions
    {


        public static bool IsEmpty(this JObject json)
        {
            return !json.HasValues;
        }

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
                Log.Logger.Error(mess, ex);
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
                Log.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }
    }
}