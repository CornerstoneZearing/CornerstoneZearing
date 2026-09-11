using System.Security.Claims;
using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CornerstoneZearing.Web.Authorization;

/// <summary>
/// Adds every permission claim held by the user's roles to the signed-in principal,
/// so <see cref="PermissionAuthorizationHandler"/> can check them without a DB hit.
/// Permission/role changes take effect on the user's next sign-in (or when the
/// security-stamp validation interval elapses).
/// </summary>
public class PermissionClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly CornerstoneDbContext _db;

    public PermissionClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options,
        CornerstoneDbContext db)
        : base(userManager, roleManager, options)
    {
        _db = db;
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var permissions = await _db.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == Permissions.ClaimType)
            .Select(rc => rc.ClaimValue!)
            .Distinct()
            .ToListAsync();

        foreach (var permission in permissions)
        {
            if (!identity.HasClaim(Permissions.ClaimType, permission))
                identity.AddClaim(new Claim(Permissions.ClaimType, permission));
        }

        return principal;
    }
}
