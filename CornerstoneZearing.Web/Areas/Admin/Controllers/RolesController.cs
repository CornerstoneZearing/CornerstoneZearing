using System.Security.Claims;
using CornerstoneZearing.Data.Identity;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

[HasPermission(Permissions.Roles.Manage)]
public class RolesController : BaseAdminController
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RolesController(RoleManager<ApplicationRole> roleManager) => _roleManager = roleManager;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Roles";
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        return View(roles);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "New role";
        return View("Edit", new RoleEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleEditViewModel vm)
    {
        if (!ModelState.IsValid) return View("Edit", vm);

        var role = new ApplicationRole(vm.Name.Trim()) { Description = vm.Description };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View("Edit", vm);
        }

        await SyncPermissionsAsync(role, vm.Permissions);
        Success("Role created.");
        return RedirectToIndex();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();

        ViewData["Title"] = "Edit role";
        var claims = await _roleManager.GetClaimsAsync(role);
        return View(new RoleEditViewModel
        {
            Id = role.Id,
            Name = role.Name ?? "",
            Description = role.Description,
            Permissions = claims.Where(c => c.Type == Web.Authorization.Permissions.ClaimType).Select(c => c.Value).ToList(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoleEditViewModel vm)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        role.Name = vm.Name.Trim();
        role.Description = vm.Description;
        var update = await _roleManager.UpdateAsync(role);
        if (!update.Succeeded)
        {
            foreach (var e in update.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }

        await SyncPermissionsAsync(role, vm.Permissions);
        Success("Role saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();
        //if (string.Equals(role.Name, Services.DbSeeder.AdministratorRole, StringComparison.OrdinalIgnoreCase))
        //{
        //    Error("The Administrator role cannot be deleted.");
        //    return RedirectToIndex();
        //}
        await _roleManager.DeleteAsync(role);
        Success("Role deleted.");
        return RedirectToIndex();
    }

    private async Task SyncPermissionsAsync(ApplicationRole role, ICollection<string> desired)
    {
        var valid = desired.Where(p => Web.Authorization.Permissions.All.Contains(p)).ToHashSet();
        var current = (await _roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == Web.Authorization.Permissions.ClaimType).ToList();

        foreach (var claim in current.Where(c => !valid.Contains(c.Value)))
            await _roleManager.RemoveClaimAsync(role, claim);

        var currentValues = current.Select(c => c.Value).ToHashSet();
        foreach (var permission in valid.Where(p => !currentValues.Contains(p)))
            await _roleManager.AddClaimAsync(role, new Claim(Web.Authorization.Permissions.ClaimType, permission));
    }
}
