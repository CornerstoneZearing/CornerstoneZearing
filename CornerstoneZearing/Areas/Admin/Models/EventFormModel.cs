using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Areas.Admin.Models;

public class EventFormModel : IValidatableObject
{
    public Guid EventID { get; set; }

    [Required]
    [MaxLength(200)]
    [Display(Name = "Event Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Start")]
    public DateTime StartDateTime { get; set; } = DateTime.Today.AddHours(9);

    [Required]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "End")]
    public DateTime EndDateTime { get; set; } = DateTime.Today.AddHours(10);

    [Display(Name = "All-Day Event")]
    public bool IsAllDay { get; set; }

    [Display(Name = "Private Event")]
    public bool IsPrivate { get; set; } = true;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Recurrence")]
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    [Range(1, 999)]
    [Display(Name = "Every")]
    public int RecurrenceInterval { get; set; } = 1;

    [Display(Name = "Sun")]
    public bool RecurSunday { get; set; }

    [Display(Name = "Mon")]
    public bool RecurMonday { get; set; }

    [Display(Name = "Tue")]
    public bool RecurTuesday { get; set; }

    [Display(Name = "Wed")]
    public bool RecurWednesday { get; set; }

    [Display(Name = "Thu")]
    public bool RecurThursday { get; set; }

    [Display(Name = "Fri")]
    public bool RecurFriday { get; set; }

    [Display(Name = "Sat")]
    public bool RecurSaturday { get; set; }

    [Display(Name = "Pattern")]
    public MonthlyYearlyPattern MonthlyYearlyPattern { get; set; } = MonthlyYearlyPattern.SpecificDate;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "End Recurrence On")]
    public DateTime? RecurrenceEndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsAllDay)
        {
            if (EndDateTime.Date < StartDateTime.Date)
            {
                yield return new ValidationResult("End date must be on or after the start date.", [nameof(EndDateTime)]);
            }
        }
        else
        {
            if (EndDateTime <= StartDateTime)
            {
                yield return new ValidationResult("End date/time must be after the start date/time.", [nameof(EndDateTime)]);
            }
        }

        if (RecurrenceType == RecurrenceType.Weekly && !RecurSunday && !RecurMonday && !RecurTuesday && !RecurWednesday && !RecurThursday && !RecurFriday && !RecurSaturday)
        {
            yield return new ValidationResult("Weekly recurrence requires at least one day of the week to be selected.", [nameof(RecurSunday)]);
        }

        if (RecurrenceEndDate.HasValue && RecurrenceEndDate.Value.Date < StartDateTime.Date)
        {
            yield return new ValidationResult("Recurrence end date must be on or after the event start date.", [nameof(RecurrenceEndDate)]);
        }
    }
}