using System;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class OtherExtentions
    {
        public static string ToStringOrDefault(this DateTime? source, string format = "yyyy-MM-dd hh:mm:ss", string defaultValue = null)
        {
            if (source != null)
            {
                return source.Value.ToString(format);
            }
            return string.IsNullOrEmpty(defaultValue) ? string.Empty : defaultValue;
        }
    }
}