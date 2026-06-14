using Microsoft.AspNetCore.Identity;

namespace CornerstoneZearing.Data;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}