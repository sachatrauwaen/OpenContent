using System;

namespace Satrabel.OpenContent.Components
{
    internal static class DateTimeExtensions
    {
        public static DateTime? ToDateTime(this string s)
        {
            DateTime dtr;
            var tryDtr = DateTime.TryParse(s, out dtr);
            return (tryDtr) ? dtr : new DateTime?();
        }
    }
}