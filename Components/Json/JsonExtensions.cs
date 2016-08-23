using DotNetNuke.Instrumentation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using DotNetNuke.Common.Utilities;


namespace Satrabel.OpenContent.Components.Json
{
    public static class JsonExtensions
    {
        public static bool IsEmpty(this JObject json)
        {
            if (json == null) return true;
            return !json.HasValues;
        }
        public static bool IsEmpty(this JToken jtoken)
        {
            //tried using HasValues, but string value are not detected that way.
            if (jtoken == null) return true;
            string json = jtoken.ToString();
            if (json == "[]") return true;
            return string.IsNullOrEmpty(json);
        }
        public static bool Exists(this JObject json)
        {
            return !json.IsEmpty();
        }
        public static bool Exists(this JToken json)
        {
            return !json.IsEmpty();
        }
        public static JToken ToJObject(this FileUri file)
        {
            try
            {
                if (!file.FileExists) return null;
                string fileContent = File.ReadAllText(file.PhysicalFilePath);
                if (string.IsNullOrWhiteSpace(fileContent)) return null;
                return JToken.Parse(fileContent);
            }
            catch (Exception ex)
            {
                string mess = string.Format("Error while parsing file [{0}]", file.FilePath);
                Log.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }
        public static JToken ToJObject(this string text, string desc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text)) return null;
                return JToken.Parse(text);
            }
            catch (Exception ex)
            {
                string mess = string.Format("Error while parsing text [{0}]", desc);
                Log.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }
        public static JToken ToJObject(this object obj, string desc)
        {
            try
            {
                if (obj == null) return null;
                return JToken.Parse(obj.ToJson());
            }
            catch (Exception ex)
            {
                string mess = string.Format("Error while parsing object [{0}]", desc);
                Log.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }


        /// <summary>
        ///   Serializes a type to Json. Note the type must be marked Serializable 
        ///   or include a DataContract attribute.
        /// </summary>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static string ToJsonString(object value)
        {
            var ser = SerializerFactory();
            string json = ser.Serialize(value);
            return json;
        }
        /// <summary>
        ///   Extension method on object that serializes the value to Json. 
        ///   Note the type must be marked Serializable or include a DataContract attribute.
        /// </summary>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static string ToJson(this object value)
        {
            return ToJsonString(value);
        }
        /// <summary>
        ///   Deserializes a json string into a specific type. 
        ///   Note that the type specified must be serializable.
        /// </summary>
        /// <param name = "json"></param>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static object FromJsonString(string json, Type type)
        {
            // *** Have to use Reflection with a 'dynamic' non constant type instance 
            var ser = SerializerFactory();

            object result = ser.GetType().GetMethod("Deserialize").MakeGenericMethod(type).Invoke(ser, new object[1] { json });
            return result;
        }

        /// <summary>
        ///   Extension method to string that deserializes a json string 
        ///   into a specific type. 
        ///   Note that the type specified must be serializable.
        /// </summary>
        /// <param name = "json"></param>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static object FromJson(this string json, Type type)
        {
            return FromJsonString(json, type);
        }

        public static TType FromJson<TType>(this string json)
        {
            var ser = SerializerFactory();

            var result = ser.Deserialize<TType>(json);
            return result;
        }

        private static JavaScriptSerializer SerializerFactory()
        {
            // Allow large JSON strings to be serialized and deserialized.
            return new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
        }
    }
}