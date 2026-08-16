using Microsoft.AspNetCore.Identity;

namespace CornerstoneZearing.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
            {
                return UserName ?? Email ?? "Unknown";
            }

            return $"{FirstName} {LastName}".Trim();
        }
    }
}