using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class Normalize
    {
        #region NormalizeDynamic
        /// <summary>
        /// Normalizes a setting from a Alpaca form field
        /// </summary>
        /// <para>
        /// An Alpaca field that has been defined as Number and is not filled in (has no value)
        /// will return a dynamic null value that is seen as a int with value null (not a int? but a real int with value null). 
        /// C# manual and Resharper says an int can never be Null. But alpaca forms manages to give us a int that is null
        /// That is very strange and akward and needs to be normalized.
        /// </para>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int DynamicValue(dynamic value, int defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == 0.GetType()) return value ?? defaultValue; //Resharper says value is never Null. 

            int retVal = 0;
            if (!int.TryParse(value, out retVal))
            {
                retVal = defaultValue;
            }
            return retVal;
        }
        public static bool DynamicValue(dynamic value, bool defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == true.GetType()) return value ?? defaultValue; //Resharper says value is never Null. 

            bool retVal;
            if (!bool.TryParse(value, out retVal))
            {
                retVal = defaultValue;
            }
            return retVal;
        }
        public static string DynamicValue(dynamic value, string defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == "".GetType()) return value ?? defaultValue; //Resharper says value is never Null. 
            return value.ToString();
        }
        public static DateTime? DynamicValue(dynamic value, DateTime defaultValue)
        {
            if (value == null && defaultValue == DateTime.MinValue) return null;
            if (value == null) return defaultValue as DateTime?;
            if (value.GetType() == DateTime.MinValue.GetType()) return value as DateTime?;

            DateTime? retval = null;
            if (value.GetType() == "".GetType())
            {
                retval = DateTimeExtensions.ToDateTime(value.ToString());
            }
            return retval as DateTime?;
        }
        public static string DynamicValue(dynamic value, DateTime defaultValue, string format)
        {
            if (value == null && defaultValue == DateTime.MinValue) return string.Empty;
            if (value == null) return string.Empty;

            DateTime? retval = null;
            if (value.GetType() == DateTime.MinValue.GetType())
                retval = value as DateTime?;
            else if (value.GetType() == "".GetType())
            {
                retval = DateTimeExtensions.ToDateTime(value);
            }
            if (retval == null) return string.Empty;

            if (string.IsNullOrEmpty(format))
                format = "yyyy-MM-dd hh:mm:ss";

            return retval.ToStringOrDefault(format);
        }


        /// <summary>
        /// Normalise an string Array
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <example>model.values = Normalize.DynamicValue(model.values, new string[]{});</example>
        /// <returns></returns>
        public static string[] DynamicValue(dynamic value, string[] defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == defaultValue.GetType()) return value ?? defaultValue; //Resharper says value is never Null. 

            object[] source = ((System.Web.Helpers.DynamicJsonArray)value).ToArray();
            string[] retval = Array.ConvertAll(source, i => i.ToString());

            return retval;
        }
        /// <summary>
        /// Normalise an int Array
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <example>model.values = Normalize.DynamicValue(model.values, new int[]{});</example>
        /// <returns></returns>
        public static int[] DynamicValue(dynamic value, int[] defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == defaultValue.GetType()) return value ?? defaultValue; //Resharper says value is never Null. 

            object[] source = ((System.Web.Helpers.DynamicJsonArray)value).ToArray();
            int[] retval = Array.ConvertAll(source, x =>
                {
                    int r;
                    if (int.TryParse(x.ToString(), out r))
                        return r;
                    else
                        return -1;
                }
            );
            return retval;
        }
        #endregion

        #region NormalizeDynamic

        public static JObject JsonObject(JObject value, string key, JObject defaultValue)
        {
            if (value == null) return defaultValue;
            if (string.IsNullOrEmpty(key)) return defaultValue;

            var extract = value[key] as JObject;
            if (extract == null) return defaultValue;

            return extract;
        }

        public static int JsonValue(JToken value, int defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.IsEmpty()) return defaultValue;
            if ((value as JValue) == null) return defaultValue;

            return Normalize.DynamicValue(value.ToString(), defaultValue);
        }

        public static bool JsonValue(JToken value, bool defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.IsEmpty()) return defaultValue;
            if ((value as JValue) == null) return defaultValue;

            return Normalize.DynamicValue(value.ToString(), defaultValue);
        }
        #endregion
    }
}