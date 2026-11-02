using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class EventEditView
{
    public int EventID { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required, DataType(DataType.DateTime)]
    public DateTime StartDateTime { get; set; } = DateTime.Today.AddHours(9);

    [Required, DataType(DataType.DateTime)]
    public DateTime EndDateTime { get; set; } = DateTime.Today.AddHours(10);

    public bool AllDay { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public string? Description { get; set; }

    public bool Private { get; set; }

    [StringLength(500)]
    public string? RecurrenceRule { get; set; }

    [DataType(DataType.Date)]
    public DateTime? RecurrenceEndDate { get; set; }

    public string? RecurrenceExceptions { get; set; }
}