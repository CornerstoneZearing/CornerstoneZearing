using Microsoft.AspNetCore.Identity;

namespace CornerstoneZearing.Data.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }

    public string? Description { get; set; }
}
