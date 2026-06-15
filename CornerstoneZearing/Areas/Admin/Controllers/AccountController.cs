using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _SignInManager;

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="signInManager"></param>
    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _SignInManager = signInManager;
    }

    /// <summary>
    /// Login page.
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        return View(new LoginModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Login form submission.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Please try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    /// <summary>
    /// Logout submission.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _SignInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}