using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace Satrabel.OpenContent.Components.Json
{
    /// <summary>
    /// Case-insensitive dynamic wrapper compatible with Razor/Handlebars model access
    /// (parity with System.Web.Helpers.DynamicJsonObject for large JSON models).
    /// </summary>
    internal sealed class OpenContentDynamicJson : DynamicObject
    {
        private readonly IDictionary<string, object> _values;

        public OpenContentDynamicJson(IDictionary<string, object> values)
        {
            // Mirror DynamicJsonObject: wrap children up front, case-insensitive keys.
            _values = (values ?? new Dictionary<string, object>())
                .ToDictionary(p => p.Key, p => Wrap(p.Value), StringComparer.OrdinalIgnoreCase);
        }

        public static object Wrap(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is OpenContentDynamicJson || value is OpenContentDynamicJsonArray)
            {
                return value;
            }

            var dict = value as IDictionary<string, object>;
            if (dict != null)
            {
                return new OpenContentDynamicJson(dict);
            }

            var array = value as object[];
            if (array != null)
            {
                return new OpenContentDynamicJsonArray(array);
            }

            return value;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // Mirror DynamicJsonObject.TryConvert
            result = null;
            if (binder.Type.IsAssignableFrom(_values.GetType()))
            {
                result = _values;
                return true;
            }

            throw new InvalidOperationException(
                string.Format(CultureInfo.CurrentCulture, "Unable to convert to type '{0}'", binder.Type));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetValue(binder.Name);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = Wrap(value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var key = GetKey(indexes);
            result = null;
            if (!string.IsNullOrEmpty(key))
            {
                result = GetValue(key);
            }
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            // Needed for templates/helpers that assign via indexer, e.g. HolonCom.OcUtils.RegisterDynamic: source[name] = ...
            var key = GetKey(indexes);
            if (!string.IsNullOrEmpty(key))
            {
                _values[key] = Wrap(value);
            }
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        internal IDictionary<string, object> AsDictionary()
        {
            return _values;
        }

        private object GetValue(string name)
        {
            object result;
            if (_values.TryGetValue(name, out result))
            {
                return result;
            }
            return null;
        }

        private static string GetKey(object[] indexes)
        {
            if (indexes != null && indexes.Length == 1 && indexes[0] != null)
            {
                return indexes[0].ToString();
            }
            return null;
        }
    }

    /// <summary>
    /// Array wrapper with System.Web.Helpers.DynamicJsonArray parity:
    /// missing named members resolve to null; Length/Count work under Razor dynamic binding.
    /// </summary>
    internal sealed class OpenContentDynamicJsonArray : DynamicObject, IEnumerable<object>, IEnumerable
    {
        private readonly object[] _values;

        public OpenContentDynamicJsonArray(object[] values)
        {
            if (values == null)
            {
                _values = new object[0];
                return;
            }

            _values = new object[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                _values[i] = OpenContentDynamicJson.Wrap(values[i]);
            }
        }

        public int Length
        {
            get { return _values.Length; }
        }

        public int Count
        {
            get { return _values.Length; }
        }

        public dynamic this[int index]
        {
            get { return _values[index]; }
            set { _values[index] = OpenContentDynamicJson.Wrap(value); }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // Razor's dynamic binder uses TryGetMember even when a CLR Length property exists
            // (unlike the C# runtime binder). Return Length/Count explicitly.
            if (string.Equals(binder.Name, "Length", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(binder.Name, "Count", StringComparison.OrdinalIgnoreCase))
            {
                result = _values.Length;
                return true;
            }

            // Mirror DynamicJsonArray: other named members resolve to null (no RuntimeBinderException).
            result = null;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int index;
            if (TryGetIntIndex(indexes, out index) && index >= 0 && index < _values.Length)
            {
                result = _values[index];
                return true;
            }

            result = null;
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            int index;
            if (TryGetIntIndex(indexes, out index) && index >= 0 && index < _values.Length)
            {
                _values[index] = OpenContentDynamicJson.Wrap(value);
                return true;
            }

            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // Mirror DynamicJsonArray.TryConvert (+ allow Array/object[])
            if (_values.GetType().IsAssignableFrom(binder.Type) ||
                binder.Type == typeof(object[]) ||
                binder.Type == typeof(Array))
            {
                result = _values;
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public IEnumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return ((IEnumerable<object>)_values).GetEnumerator();
        }

        public static implicit operator object[](OpenContentDynamicJsonArray obj)
        {
            return obj == null ? null : obj._values;
        }

        public static implicit operator Array(OpenContentDynamicJsonArray obj)
        {
            return obj == null ? null : obj._values;
        }

        internal object[] AsArray()
        {
            return _values;
        }

        private static bool TryGetIntIndex(object[] indexes, out int index)
        {
            index = -1;
            if (indexes == null || indexes.Length != 1 || indexes[0] == null)
            {
                return false;
            }

            if (indexes[0] is int)
            {
                index = (int)indexes[0];
                return true;
            }

            return int.TryParse(indexes[0].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out index);
        }
    }
}
