namespace CornerstoneZearing.Data.Entities;

public class Event
{
    public int EventID { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool AllDay { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool Private { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public string? RecurrenceExceptions { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}