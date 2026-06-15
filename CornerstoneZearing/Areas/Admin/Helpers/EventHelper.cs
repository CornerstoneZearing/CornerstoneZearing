using CornerstoneZearing.Data;

namespace CornerstoneZearing.Areas.Admin.Helpers
{
    public static class EventHelper
    {
        /// <summary>
        /// Generates a human-readable summary of recurrence settings for the specified event.
        /// </summary>
        /// <param name="evnt"></param>
        /// <returns></returns>
        public static string RecurrenceSummary(Event evnt)
        {
            if (evnt.RecurrenceType == RecurrenceType.None)
            {
                return "";
            }

            int interval = evnt.RecurrenceInterval;
            DateTime start = evnt.StartDateTime.ToLocalTime();

            if (evnt.RecurrenceType == RecurrenceType.Daily)
            {
                return interval == 1 ? "Daily" : $"Every {interval} days";
            }

            if (evnt.RecurrenceType == RecurrenceType.Weekly)
            {
                var days = new List<string>();
                if (evnt.RecurSunday)
                {
                    days.Add("Sun");
                }
                if (evnt.RecurMonday)
                {
                    days.Add("Mon");
                }
                if (evnt.RecurTuesday)
                {
                    days.Add("Tue");
                }
                if (evnt.RecurWednesday)
                {
                    days.Add("Wed");
                }
                if (evnt.RecurThursday)
                {
                    days.Add("Thu");
                }
                if (evnt.RecurFriday)
                {
                    days.Add("Fri");
                }
                if (evnt.RecurSaturday)
                {
                    days.Add("Sat");
                }

                string dayStr = days.Count != 0 ? string.Join(", ", days) : "";
                return interval == 1 ? $"Weekly on {dayStr}" : $"Every {interval} weeks on {dayStr}";
            }

            if (evnt.RecurrenceType == RecurrenceType.Monthly)
            {
                string prefix = interval == 1 ? "Monthly" : $"Every {interval} months";
                if (evnt.MonthlyYearlyPattern == MonthlyYearlyPattern.SpecificDate)
                {
                    return $"{prefix}, day {start.Day}";
                }
                return $"{prefix}, {WeekPosition(start)} {start.DayOfWeek}";
            }

            if (evnt.RecurrenceType == RecurrenceType.Yearly)
            {
                string prefix = interval == 1 ? "Yearly" : $"Every {interval} years";
                if (evnt.MonthlyYearlyPattern == MonthlyYearlyPattern.SpecificDate)
                {
                    return $"{prefix}, {start:MMM d}";
                }
                return $"{prefix}, {WeekPosition(start)} {start.DayOfWeek} of {start:MMMM}";
            }

            return "";
        }


        /// <summary>
        /// Returns the week position of the specified date for its month.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string WeekPosition(DateTime date)
        {
            int day = date.Day;
            int days = DateTime.DaysInMonth(date.Year, date.Month);

            if (day + 7 > days)
            {
                return "last";
            }

            return Math.Ceiling((double)day / 7) switch
            {
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                _ => "4th"
            };
        }
    }
}