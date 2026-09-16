using CornerstoneZearing.Data.Identity;
using Microsoft.AspNetCore.Identity;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

[HasPermission(Permissions.Users.Manage)]
public class UsersController : BaseAdminController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Users";
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var list = new List<UserListItem>();
        foreach (var u in users)
        {
            list.Add(new UserListItem
            {
                Id = u.Id,
                Email = u.Email ?? "",
                DisplayName = u.DisplayName,
                IsActive = u.IsActive,
                Roles = (await _userManager.GetRolesAsync(u)).ToList(),
            });
        }
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New User";
        return View("Edit", new UserEditViewModel { Roles = await RoleCheckboxesAsync(Array.Empty<string>()) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserEditViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.NewPassword))
            ModelState.AddModelError(nameof(vm.NewPassword), "A password is required for a new user.");

        if (!ModelState.IsValid)
        {
            vm.Roles = await RoleCheckboxesAsync(SelectedRoleNames(vm));
            return View("Edit", vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            EmailConfirmed = true,
            IsActive = vm.IsActive,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
        };
        var result = await _userManager.CreateAsync(user, vm.NewPassword!);
        if (!result.Succeeded)
        {
            AddErrors(result);
            vm.Roles = await RoleCheckboxesAsync(SelectedRoleNames(vm));
            return View("Edit", vm);
        }

        await _userManager.AddToRolesAsync(user, SelectedRoleNames(vm));
        Success("User created.");
        return RedirectToIndex();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        ViewData["Title"] = "Edit User";
        var roles = await _userManager.GetRolesAsync(user);
        return View(new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = await RoleCheckboxesAsync(roles),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserEditViewModel vm)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.Roles = await RoleCheckboxesAsync(SelectedRoleNames(vm));
            return View(vm);
        }

        user.Email = vm.Email;
        user.UserName = vm.Email;
        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.IsActive = vm.IsActive;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded) { AddErrors(update); vm.Roles = await RoleCheckboxesAsync(SelectedRoleNames(vm)); return View(vm); }

        if (!string.IsNullOrWhiteSpace(vm.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);
            if (!reset.Succeeded) { AddErrors(reset); vm.Roles = await RoleCheckboxesAsync(SelectedRoleNames(vm)); return View(vm); }
        }

        var current = await _userManager.GetRolesAsync(user);
        var desired = SelectedRoleNames(vm);
        await _userManager.RemoveFromRolesAsync(user, current.Except(desired));
        await _userManager.AddToRolesAsync(user, desired.Except(current));

        await _userManager.UpdateSecurityStampAsync(user);
        Success("User saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        Success(user.IsActive ? "User activated." : "User deactivated.");
        return RedirectToIndex();
    }

    private static string[] SelectedRoleNames(UserEditViewModel vm) =>
        vm.Roles.Where(r => r.Assigned).Select(r => r.Name).ToArray();

    private async Task<List<RoleCheckbox>> RoleCheckboxesAsync(IEnumerable<string> assigned)
    {
        var set = assigned.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var roles = await _roleManager.Roles.OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();
        return roles
            .Select(r => new RoleCheckbox { RoleId = r.Id, Name = r.Name!, Assigned = set.Contains(r.Name!) })
            .ToList();
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e.Description);
    }
}
