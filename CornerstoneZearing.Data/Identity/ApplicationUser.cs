using Microsoft.AspNetCore.Identity;

namespace CornerstoneZearing.Data.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? (UserName ?? Email ?? $"User {Id}")
            : $"{FirstName} {LastName}".Trim();
}
