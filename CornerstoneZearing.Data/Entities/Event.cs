namespace CornerstoneZearing.Data.Entities;

public class Event
{
    public int EventID { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool Private { get; set; }

    /// <summary>RFC 5545 RRULE text (without the "RRULE:" prefix). Null = single occurrence.</summary>
    public string? RecurrenceRule { get; set; }

    /// <summary>Optional cap on how far the recurrence extends.</summary>
    public DateTime? RecurrenceEndDate { get; set; }

    /// <summary>Newline-separated EXDATE values (occurrence start times to skip).</summary>
    public string? RecurrenceExceptions { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}
