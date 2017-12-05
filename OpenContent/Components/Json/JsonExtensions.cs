using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Web.Script.Serialization;


namespace Satrabel.OpenContent.Components.Json
{
    public static class JsonExtensions
    {
        public static bool EqualsAny(this JToken json, params string[] valueList)
        {
            if (json.IsEmpty()) return false;
            return ((IList)valueList).Contains(json.ToString());
        }

        public static bool IsEmpty(this JObject json)
        {
            if (json == null) return true;
            return !json.HasValues;
        }

        public static bool IsEmpty(this JToken jtoken)
        {
            //tried using HasValues, but string value are not detected that way.
            if (jtoken == null) return true;
            if (jtoken.Type == JTokenType.Object)
                return (jtoken as JObject).IsEmpty();

            if (jtoken.Type == JTokenType.Array)
                return (jtoken as JArray).HasValues;

            string json = jtoken.ToString();
            if (json == "{}") return true;
            return string.IsNullOrEmpty(json);
        }

        public static bool IsNotEmpty(this JToken jtoken)
        {
            if (jtoken == null) return false;
            return !jtoken.IsEmpty();
        }

        public static bool Exists(this JObject json)
        {
            return !json.IsEmpty();
        }
        public static bool Exists(this JToken json)
        {
            return !json.IsEmpty();
        }

        public static bool HasField(this JToken json, string fieldname)
        {
            return !json.IsEmpty() && json[fieldname] != null;
        }

        public static int GetValue(this JObject json, string fieldname, int defaultvalue)
        {
            return json?[fieldname]?.Value<int>() ?? defaultvalue;
        }
        public static bool GetValue(this JObject json, string fieldname, bool defaultvalue)
        {
            return json?[fieldname]?.Value<bool>() ?? defaultvalue;
        }


        public static void MakeSureFieldExists(this JToken jToken, string fieldname, JTokenType jTokenType)
        {
            JToken defaultvalue;
            switch (jTokenType)
            {
                case JTokenType.Object:
                    defaultvalue = new JObject();
                    break;
                default:
                    throw new NotImplementedException("unknown json type in JsonExtentions");
            }
            if (!jToken.HasField(fieldname))
            {
                jToken[fieldname] = defaultvalue;
            }

            if (jToken[fieldname].Type != jTokenType)
                jToken[fieldname] = defaultvalue;
        }

        public static JToken ToJObject(this string text, string descRemark)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text)) return null;
                return JToken.Parse(text);
            }
            catch (Exception ex)
            {
                string mess = $"Error while parsing text [{descRemark}]";
                App.Services.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }
        public static JToken ToJObject(this object obj, string descRemark)
        {
            try
            {
                if (obj == null) return null;
                return JToken.Parse(obj.ToJsonString());
            }
            catch (Exception ex)
            {
                string mess = $"Error while parsing object [{descRemark}]";
                App.Services.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
        }



        /// <summary>
        ///   Serializes a type to Json. Note the type must be marked Serializable 
        ///   or include a DataContract attribute.
        /// </summary>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static string ToJsonString(this object value)
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

        /// <summary>
        /// Serializers the factory.
        /// </summary>
        /// <remarks>
        /// Needs reference to System.Web.Extentions
        /// </remarks>
        private static JavaScriptSerializer SerializerFactory()
        {
            // Allow large JSON strings to be serialized and deserialized.
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }
    }
}