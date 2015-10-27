using System;

namespace Satrabel.OpenContent.Components
{
    public static class DateTimeExtensions
    {

        public static string ToStringOrDefault(this DateTime? source, string format = "yyyy-MM-dd hh:mm:ss", string defaultValue = null)
        {
            if (source != null)
            {
                return source.Value.ToString(format);
            }
            return string.IsNullOrEmpty(defaultValue) ? string.Empty : defaultValue;
        }
        public static DateTime? ToDateTime(this string s)
        {
            DateTime dtr;
            var tryDtr = DateTime.TryParse(s, out dtr);
            return (tryDtr) ? dtr : new DateTime?();
        }

        public static bool IsInRange(this DateTime currentDate, DateTime beginDate, DateTime endDate)
        {
            return (currentDate >= beginDate && currentDate < endDate);
        }
        /// <summary>
        /// Determines whether the specified date is between two other dates.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="compareTime">if set to <c>true</c> [compare time].</param>
        /// <returns></returns>
        public static bool IsBetween(this DateTime dt, DateTime? startDate, DateTime? endDate, bool compareTime = false)
        {
            if (compareTime)
            {
                if (startDate == null)
                {
                    if (endDate == null) return true;
                    return endDate.Value < dt;
                }
                if (startDate.Value > dt)
                    return false;
                if (endDate == null) return true;
                return endDate.Value >= dt;
            }

            if (startDate == null)
            {
                if (endDate == null) return true;
                return endDate.Value.Date < dt.Date;
            }
            if (startDate.Value.Date > dt.Date)
                return false;
            if (endDate == null) return true;
            return endDate.Value.Date >= dt.Date;
        }

        #region Public Methods
        /// <summary>
        /// Returns the first day of the month
        /// </summary>
        /// <example>
        /// DateTime firstOfThisMonth = DateTime.Now.FirstOfMonth;
        /// </example>
        /// <param name="dt">Start date</param>
        /// <returns></returns>
        public static DateTime FirstOfMonth(this DateTime dt)
        {
            return (dt.AddDays(1 - dt.Day)).AtMidnight();
        }
        /// <summary>
        /// Returns the first specified day of the week in the current month
        /// </summary>
        /// <example>
        /// DateTime firstTuesday = DateTime.Now.FirstDayOfMonth(DayOfWeek.Tuesday);
        /// </example>
        /// <param name="dt">Start date</param>
        /// <param name="dayOfWeek">The required day of week</param>
        /// <returns></returns>
        public static DateTime FirstOfMonth(this DateTime dt, DayOfWeek dayOfWeek)
        {
            DateTime firstDayOfMonth = dt.FirstOfMonth();
            return (firstDayOfMonth.DayOfWeek == dayOfWeek ? firstDayOfMonth :
                    firstDayOfMonth.NextDayOfWeek(dayOfWeek)).AtMidnight();
        }
        /// <summary>
        /// Returns the last day in the current month
        /// </summary>
        /// <example>
        /// DateTime endOfMonth = DateTime.Now.LastDayOfMonth();
        /// </example>
        /// <param name="dt" />Start date
        /// <returns />
        public static DateTime LastOfMonth(this DateTime dt)
        {
            int daysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);
            return dt.FirstOfMonth().AddDays(daysInMonth - 1).AtMidnight();
        }
        /// <summary>
        /// Returns the last specified day of the week in the current month
        /// </summary>
        /// <example>
        /// DateTime finalTuesday = DateTime.Now.LastDayOfMonth(DayOfWeek.Tuesday);
        /// </example>
        /// <param name="dt" />Start date
        /// <param name="dayOfWeek" />The required day of week
        /// <returns />
        public static DateTime LastOfMonth(this DateTime dt, DayOfWeek dayOfWeek)
        {
            DateTime lastDayOfMonth = dt.LastOfMonth();
            return lastDayOfMonth.AddDays(lastDayOfMonth.DayOfWeek < dayOfWeek ?
                    dayOfWeek - lastDayOfMonth.DayOfWeek - 7 :
                    dayOfWeek - lastDayOfMonth.DayOfWeek);
        }
        /// <summary>
        /// Returns the next date which falls on the given day of the week
        /// </summary>
        /// <example>
        /// DateTime nextTuesday = DateTime.Now.NextDayOfWeek(DayOfWeek.Tuesday);
        /// </example>
        /// <param name="dt">Start date</param>
        /// <param name="dayOfWeek">The required day of week</param>
        public static DateTime NextDayOfWeek(this DateTime dt, DayOfWeek dayOfWeek)
        {
            int offsetDays = dayOfWeek - dt.DayOfWeek;
            return dt.AddDays(offsetDays > 0 ? offsetDays : offsetDays + 7).AtMidnight();
        }
        /// <summary>
        /// Returns the same day, at midnight
        /// </summary>
        /// <example>
        /// DateTime startOfDay = DateTime.Now.AtMidnight();
        /// </example>
        /// <param name="dt">Start date</param>
        public static DateTime AtMidnight(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
        }
        /// <summary>
        /// Returns the same day, at midday
        /// </summary>
        /// <example>
        /// DateTime startOfAfternoon = DateTime.Now.AtMidday();
        /// </example>
        /// <param name="dt">Start date</param>
        public static DateTime AtMidday(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 12, 0, 0);
        }
        #endregion

    }
}