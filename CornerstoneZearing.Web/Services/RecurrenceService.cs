using System.Globalization;
using System.Text;
using CornerstoneZearing.Data.Entities;
using Calendar = Ical.Net.Calendar;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace CornerstoneZearing.Web.Services;

public record EventOccurrence(int EventID, string Title, string? Location, string? Description, bool Private, DateTime Start, DateTime End);

public class RecurrenceService
{
    private const int SafetyCap = 750;

    private static DateTime Unspecified(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

    /// <summary>Expand an event into concrete occurrences that intersect [rangeStart, rangeEnd).</summary>
    public IEnumerable<EventOccurrence> Expand(Event ev, DateTime rangeStart, DateTime rangeEnd)
    {
        rangeStart = Unspecified(rangeStart);
        rangeEnd = Unspecified(rangeEnd);
        var duration = ev.EndDateTime - ev.StartDateTime;
        if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(ev.RecurrenceRule))
        {
            if (ev.StartDateTime < rangeEnd && ev.EndDateTime > rangeStart)
                yield return ToOccurrence(ev, ev.StartDateTime, ev.EndDateTime);
            yield break;
        }

        var calEvent = new CalendarEvent
        {
            Start = new CalDateTime(Unspecified(ev.StartDateTime), true),
            End = new CalDateTime(Unspecified(ev.StartDateTime.Add(duration)), true),
            RecurrenceRule = new RecurrencePattern(BuildRule(ev)),
        };
        var calendar = new Calendar();
        calendar.Events.Add(calEvent);

        var exceptions = ParseExceptions(ev.RecurrenceExceptions);
        var count = 0;

        foreach (var occurrence in calendar.GetOccurrences(new CalDateTime(rangeStart, true)))
        {
            if (++count > SafetyCap) yield break;

            var start = occurrence.Period.StartTime.Value;
            if (start >= rangeEnd) yield break;

            var end = start.Add(duration);
            if (end <= rangeStart) continue;
            if (exceptions.Contains(start)) continue;

            yield return ToOccurrence(ev, start, end);
        }
    }

    public IEnumerable<EventOccurrence> ExpandAll(IEnumerable<Event> events, DateTime rangeStart, DateTime rangeEnd) =>
        events.SelectMany(e => Expand(e, rangeStart, rangeEnd)).OrderBy(o => o.Start);

    public string ToIcs(IEnumerable<Event> events)
    {
        var calendar = new Calendar { ProductId = "-//Cornerstone Church of Christ//Events//EN" };
        foreach (var ev in events)
        {
            var duration = ev.EndDateTime - ev.StartDateTime;
            var calEvent = new CalendarEvent
            {
                Uid = $"event-{ev.EventID}@cornerstonezearing",
                Summary = ev.Title,
                Location = ev.Location,
                Description = ev.Description,
                Start = new CalDateTime(Unspecified(ev.StartDateTime), true),
                End = new CalDateTime(Unspecified(ev.StartDateTime.Add(duration < TimeSpan.Zero ? TimeSpan.Zero : duration)), true),
            };
            if (!string.IsNullOrWhiteSpace(ev.RecurrenceRule))
                calEvent.RecurrenceRule = new RecurrencePattern(BuildRule(ev));
            calendar.Events.Add(calEvent);
        }

        return new CalendarSerializer().SerializeToString(calendar) ?? string.Empty;
    }

    private static string BuildRule(Event ev)
    {
        var rule = ev.RecurrenceRule!.Trim();
        if (rule.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            rule = rule[6..];

        var hasBound = rule.Contains("UNTIL=", StringComparison.OrdinalIgnoreCase) ||
                       rule.Contains("COUNT=", StringComparison.OrdinalIgnoreCase);
        if (!hasBound && ev.RecurrenceEndDate is { } until)
        {
            var sep = rule.EndsWith(';') ? "" : ";";
            rule += $"{sep}UNTIL={until.Date.AddDays(1).AddSeconds(-1):yyyyMMddTHHmmss}";
        }

        return rule;
    }

    private static HashSet<DateTime> ParseExceptions(string? raw)
    {
        var set = new HashSet<DateTime>();
        if (string.IsNullOrWhiteSpace(raw)) return set;

        foreach (var line in raw.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (DateTime.TryParse(line, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
                DateTime.TryParseExact(line, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                set.Add(dt);
            }
        }

        return set;
    }

    private static EventOccurrence ToOccurrence(Event ev, DateTime start, DateTime end) =>
        new(ev.EventID, ev.Title, ev.Location, ev.Description, ev.Private, start, end);
}
