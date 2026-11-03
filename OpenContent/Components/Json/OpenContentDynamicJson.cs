using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Satrabel.OpenContent.Components.Json
{
    /// <summary>
    /// Case-insensitive dynamic wrapper compatible with Razor/Handlebars model access
    /// (replaces System.Web.Helpers.DynamicJsonObject for large JSON models).
    /// </summary>
    internal sealed class OpenContentDynamicJson : DynamicObject, IEnumerable
    {
        private readonly IDictionary<string, object> _values;

        public OpenContentDynamicJson(IDictionary<string, object> values)
        {
            _values = values ?? new Dictionary<string, object>();
        }

        public static object Wrap(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is OpenContentDynamicJson)
            {
                return value;
            }

            if (value is IDictionary<string, object> dict)
            {
                return new OpenContentDynamicJson(dict);
            }

            if (value is object[] array)
            {
                var wrapped = new object[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    wrapped[i] = Wrap(array[i]);
                }
                return wrapped;
            }

            return value;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_values.TryGetValue(binder.Name, out result))
            {
                result = Wrap(result);
                return true;
            }

            result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = value;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes != null && indexes.Length == 1 && indexes[0] != null)
            {
                var key = indexes[0].ToString();
                if (_values.TryGetValue(key, out result))
                {
                    result = Wrap(result);
                    return true;
                }
            }

            result = null;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _values.Keys;
        }

        public IEnumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        internal IDictionary<string, object> AsDictionary()
        {
            return _values;
        }
    }
}
