#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Reflection;

namespace Satrabel.OpenContent.Components
{
    /// <summary>
    /// Assert Class.
    /// </summary>
    public static class Requires
    {
        /// <summary>
        /// Determines whether argValue is type of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argValue">The arg value.</param>
        /// <param name="argName">Name of the arg.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void IsTypeOf<T>(object argValue, string argName)
        {
            if (!((argValue) is T))
            {
                throw new ArgumentException($"The argument '{argName}' must be of type '{typeof(T).FullName}'.", argName);
            }
        }

        /// <summary>
        /// Determines whether argValue is less than zero.
        /// </summary>
        /// <param name="argValue">The arg value.</param>
        /// <param name="argName">Name of the arg.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void NotNegative(int argValue, string argName)
        {
            if (argValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName, $"The argument '{argName}' cannot be negative.");
            }
        }

        /// <summary>
        /// Determines whether the argument is null.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(typeof(T).Name);
            }
        }

        /// <summary>
        /// Determines whether the argValue is null.
        /// </summary>
        /// <param name="argValue">The arg value.</param>
        /// <param name="argName">Name of the arg.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull(object argValue, string argName)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// Determines whether the argValue is null or empty.
        /// </summary>
        /// <param name="argValue">The arg value.</param>
        /// <param name="argName">Name of the arg.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void NotNullOrEmpty(string argValue, string argName)
        {
            if (string.IsNullOrEmpty(argValue))
            {
                throw new ArgumentException($"The argument '{argName}' cannot be null or empty.", argName);
            }
        }

        /// <summary>
        /// Determines whether a property is negative.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void PropertyNotNegative<T>(T item, string propertyName)
        {
            //Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            PropertyInfo property = type.GetProperty(propertyName);
            var propertyValue = property.GetValue(item, null);

            var intValue = (int)propertyValue;

            if (intValue < 0)
            {
                throw new ArgumentOutOfRangeException($"The property '{propertyName}' in object '{typeof(T).Name}' cannot be negative.", propertyName);
            }
        }

        /// <summary>
        /// Determines whether propertyValue is less than zero.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void PropertyNotNegative(string argName, string argProperty, int propertyValue)
        {
            if (propertyValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName, string.Format("The property '{1}' in object '{0}' cannot be negative.", argName, argProperty));
            }
        }

        /// <summary>
        /// Determines whether the argValue is null.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void PropertyNotNull(string argValue, string argName)
        {
            NotNull(argName, argValue);
        }

        /// <summary>
        /// Determines whether a property is null.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void PropertyNotNull<T>(T item, string propertyName) where T : class
        {
            //Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var propertyValue = property.GetValue(item, null);

            if (propertyValue == null)
            {
                throw new ArgumentNullException(propertyName);
            }
        }

        /// <summary>
        /// Determines whether a property is null or empty.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNullOrEmpty<T>(T item, string propertyName)
        {
            //Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var propertyValue = property.GetValue(item, null);
            var stringValue = propertyValue as string;

            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentException(propertyName, string.Format("The property '{1}' in object '{0}' cannot be null or empty.", typeof(T).Name, propertyName));
            }
        }

        /// <summary>
        /// Determins whether propertyValye is not null or empty.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNullOrEmpty(string argName, string argProperty, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ArgumentException(argName,
                    string.Format("The property '{1}' in object '{0}' cannot be null or empty.", argName, argProperty));
            }
        }

        /// <summary>
        /// Determines whether propertyValue equal to testValue.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="testValue">The test value.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void PropertyNotEqualTo<TValue>(string argName, string argProperty, TValue propertyValue, TValue testValue) where TValue : IEquatable<TValue>
        {
            if (propertyValue.Equals(testValue))
            {
                throw new ArgumentException(argName, string.Format("The property '{1}' in object '{0}' is invalid.", argName, argProperty));
            }
        }
    }
}
