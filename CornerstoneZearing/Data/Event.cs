using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Data;

public class Event
{
    public Guid EventID { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsAllDay { get; set; }

    public bool IsPrivate { get; set; } = false;

    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    public int RecurrenceInterval { get; set; } = 1;

    public bool RecurSunday { get; set; }

    public bool RecurMonday { get; set; }

    public bool RecurTuesday { get; set; }

    public bool RecurWednesday { get; set; }

    public bool RecurThursday { get; set; }

    public bool RecurFriday { get; set; }

    public bool RecurSaturday { get; set; }

    public MonthlyYearlyPattern MonthlyYearlyPattern { get; set; } = MonthlyYearlyPattern.SpecificDate;

    public DateTime? RecurrenceEndDate { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateModified { get; set; }
}