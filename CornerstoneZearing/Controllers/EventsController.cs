using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Data;

namespace CornerstoneZearing.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
    {
        // FullCalendar sends local-time range params (DateTimeKind.Unspecified from model binding)
        var utcStart = DateTime.SpecifyKind(start, DateTimeKind.Local).ToUniversalTime();
        var utcEnd = DateTime.SpecifyKind(end, DateTimeKind.Local).ToUniversalTime();

        var events = await _context.Events
            .Where(e => !e.IsPrivate)
            .ToListAsync();

        var calendarEvents = new List<object>();
        foreach (var ev in events)
            calendarEvents.AddRange(ExpandOccurrences(ev, utcStart, utcEnd));

        return Json(calendarEvents);
    }

    private static IEnumerable<object> ExpandOccurrences(Event ev, DateTime utcRangeStart, DateTime utcRangeEnd)
    {
        // Work in local time so day-of-week and recurrence logic matches what was entered
        var localStart = ev.StartDateTime.ToLocalTime();
        var localEnd = ev.EndDateTime.ToLocalTime();
        var duration = localEnd - localStart;

        var localRangeStart = utcRangeStart.ToLocalTime();
        var localRangeEnd = utcRangeEnd.ToLocalTime();

        var effectiveEnd = ev.RecurrenceEndDate.HasValue
            ? Min(ev.RecurrenceEndDate.Value.ToLocalTime().Date.AddDays(1), localRangeEnd)
            : localRangeEnd;

        if (ev.RecurrenceType == RecurrenceType.None)
        {
            if (localStart < localRangeEnd && localEnd > localRangeStart)
                yield return MakeCalendarEvent(ev, localStart, localEnd);
            yield break;
        }

        if (ev.RecurrenceType == RecurrenceType.Weekly)
        {
            foreach (var item in ExpandWeekly(ev, localStart, duration, localRangeStart, effectiveEnd))
                yield return item;
            yield break;
        }

        var cursor = localStart;

        // Skip ahead for large gaps (daily events with many past occurrences)
        if (ev.RecurrenceType == RecurrenceType.Daily && cursor < localRangeStart)
        {
            int days = (int)(localRangeStart - cursor).TotalDays;
            int skip = Math.Max(0, days / ev.RecurrenceInterval - 1);
            cursor = cursor.AddDays(skip * ev.RecurrenceInterval);
        }

        while (cursor < effectiveEnd)
        {
            if (cursor + duration > localRangeStart)
                yield return MakeCalendarEvent(ev, cursor, cursor + duration);

            cursor = ev.RecurrenceType switch
            {
                RecurrenceType.Daily => cursor.AddDays(ev.RecurrenceInterval),
                RecurrenceType.Monthly => AdvanceMonthly(ev, cursor),
                RecurrenceType.Yearly => AdvanceYearly(ev, cursor),
                _ => effectiveEnd
            };
        }
    }

    private static IEnumerable<object> ExpandWeekly(Event ev, DateTime localEventStart, TimeSpan duration, DateTime localRangeStart, DateTime effectiveEnd)
    {
        var selectedDays = GetSelectedDays(ev).ToList();
        if (selectedDays.Count == 0) yield break;

        var timeOfDay = localEventStart.TimeOfDay;

        // Anchor to the Sunday of the week containing the event's start date
        var anchorWeekStart = localEventStart.Date.AddDays(-(int)localEventStart.DayOfWeek);

        // Jump close to the range start to avoid iterating from the distant past
        if (localRangeStart > anchorWeekStart)
        {
            int weeksBehind = (int)((localRangeStart - anchorWeekStart).TotalDays / 7);
            int skippableIntervals = Math.Max(0, weeksBehind / ev.RecurrenceInterval - 1);
            anchorWeekStart = anchorWeekStart.AddDays(skippableIntervals * ev.RecurrenceInterval * 7);
        }

        var weekStart = anchorWeekStart;
        while (weekStart < effectiveEnd)
        {
            foreach (var dow in selectedDays)
            {
                var occurrence = weekStart.AddDays((int)dow).Add(timeOfDay);
                if (occurrence >= localEventStart
                    && occurrence < effectiveEnd
                    && occurrence + duration > localRangeStart)
                {
                    yield return MakeCalendarEvent(ev, occurrence, occurrence + duration);
                }
            }
            weekStart = weekStart.AddDays(ev.RecurrenceInterval * 7);
        }
    }

    private static DateTime AdvanceMonthly(Event ev, DateTime current)
    {
        if (ev.MonthlyYearlyPattern == MonthlyYearlyPattern.SpecificDate)
            return current.AddMonths(ev.RecurrenceInterval);

        var target = current.AddMonths(ev.RecurrenceInterval);
        return FindDayOfWeekOccurrence(ev.StartDateTime.ToLocalTime(), target.Year, target.Month);
    }

    private static DateTime AdvanceYearly(Event ev, DateTime current)
    {
        if (ev.MonthlyYearlyPattern == MonthlyYearlyPattern.SpecificDate)
            return current.AddYears(ev.RecurrenceInterval);

        var localStart = ev.StartDateTime.ToLocalTime();
        int targetYear = current.Year + ev.RecurrenceInterval;
        return FindDayOfWeekOccurrence(localStart, targetYear, localStart.Month);
    }

    // Finds the Nth occurrence of a day-of-week in a given month, matching the position of the reference date.
    private static DateTime FindDayOfWeekOccurrence(DateTime reference, int year, int month)
    {
        int position = (reference.Day - 1) / 7; // 0-based position within month
        var dow = reference.DayOfWeek;

        var firstOfMonth = new DateTime(year, month, 1);
        int daysUntil = ((int)dow - (int)firstOfMonth.DayOfWeek + 7) % 7;
        var candidate = firstOfMonth.AddDays(daysUntil + position * 7);

        // If overflow to next month, fall back to last occurrence
        if (candidate.Month != month)
            candidate = candidate.AddDays(-7);

        return candidate.Add(reference.TimeOfDay);
    }

    private static IEnumerable<DayOfWeek> GetSelectedDays(Event ev)
    {
        if (ev.RecurSunday) yield return DayOfWeek.Sunday;
        if (ev.RecurMonday) yield return DayOfWeek.Monday;
        if (ev.RecurTuesday) yield return DayOfWeek.Tuesday;
        if (ev.RecurWednesday) yield return DayOfWeek.Wednesday;
        if (ev.RecurThursday) yield return DayOfWeek.Thursday;
        if (ev.RecurFriday) yield return DayOfWeek.Friday;
        if (ev.RecurSaturday) yield return DayOfWeek.Saturday;
    }

    private static object MakeCalendarEvent(Event ev, DateTime localStart, DateTime localEnd)
    {
        // Return as Unspecified so JSON serializer omits the timezone suffix;
        // FullCalendar then treats them as calendar-local times.
        return new
        {
            id = ev.EventID,
            title = ev.Name,
            start = DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified),
            end = DateTime.SpecifyKind(localEnd, DateTimeKind.Unspecified),
            allDay = ev.IsAllDay,
            extendedProps = new
            {
                location = ev.Location,
                description = ev.Description
            }
        };
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
