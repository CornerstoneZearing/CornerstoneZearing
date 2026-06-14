using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Areas.Admin.Models;

public class UserFormViewModel : IValidatableObject
{
    public Guid UserID { get; set; }

    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [DataType(DataType.Password), Display(Name = "Password")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
    public string? Password { get; set; }

    [DataType(DataType.Password), Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }

    public List<string> SelectedRoles { get; set; } = [];
    public List<string> AvailableRoles { get; set; } = [];

    public bool IsEdit => UserID != Guid.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsEdit && string.IsNullOrWhiteSpace(Password))
            yield return new ValidationResult("The Password field is required.", [nameof(Password)]);

        if (!string.IsNullOrWhiteSpace(Password) && Password != ConfirmPassword)
            yield return new ValidationResult("The password and confirmation password do not match.", [nameof(ConfirmPassword)]);
    }
}
