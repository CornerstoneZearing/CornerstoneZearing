using Microsoft.AspNetCore.Identity;

namespace CornerstoneZearing.Data.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}