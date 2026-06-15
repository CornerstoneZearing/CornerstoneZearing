using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _UserManager;
    private readonly RoleManager<ApplicationRole> _RoleManager;

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="roleManager"></param>
    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _UserManager = userManager;
        _RoleManager = roleManager;
    }

    /// <summary>
    /// List page.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var models = new List<UserListModel>();
        foreach (var user in await _UserManager.Users.ToListAsync())
        {
            models.Add(new UserListModel
            {
                UserID = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = await _UserManager.GetRolesAsync(user)
            });
        }
        return View(models);
    }

    /// <summary>
    /// Create page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new UserFormModel
        {
            AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync()
        };
        return View("Form", model);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View("Form", model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _UserManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View("Form", model);
        }

        if (model.SelectedRoles.Count > 0)
        {
            await _UserManager.AddToRolesAsync(user, model.SelectedRoles);
        }

        TempData["Success"] = $"User {user.Email} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Edit page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _UserManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _UserManager.GetRolesAsync(user);
        var model = new UserFormModel
        {
            UserID = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            SelectedRoles = [.. currentRoles],
            AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync()
        };

        return View("Form", model);
    }

    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View("Form", model);
        }

        var user = await _UserManager.FindByIdAsync(model.UserID.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.Email = model.Email;
        user.UserName = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var updateResult = await _UserManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View("Form", model);
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _UserManager.GeneratePasswordResetTokenAsync(user);
            var pwResult = await _UserManager.ResetPasswordAsync(user, token, model.Password);
            if (!pwResult.Succeeded)
            {
                foreach (var error in pwResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.AvailableRoles = await _RoleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View("Form", model);
            }
        }

        var currentRoles = await _UserManager.GetRolesAsync(user);
        await _UserManager.RemoveFromRolesAsync(user, currentRoles);
        if (model.SelectedRoles.Count > 0)
        {
            await _UserManager.AddToRolesAsync(user, model.SelectedRoles);
        }

        TempData["Success"] = $"User {user.Email} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Delete confirmation page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _UserManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var currentUserId = _UserManager.GetUserId(User);
        if (user.Id.ToString() == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        return View(new UserListModel
        {
            UserID = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = await _UserManager.GetRolesAsync(user)
        });
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var user = await _UserManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var currentUserId = _UserManager.GetUserId(User);
        if (user.Id.ToString() == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _UserManager.DeleteAsync(user);
        TempData["Success"] = $"User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}