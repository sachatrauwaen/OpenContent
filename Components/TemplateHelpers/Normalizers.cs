using System;

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
        #endregion

    }
}