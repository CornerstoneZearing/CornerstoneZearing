using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public abstract class BaseAdminController : Controller
{
    protected IActionResult RedirectToIndex() => RedirectToAction("Index");

    protected void Success(string message) => TempData["Success"] = message;

    protected void Error(string message) => TempData["Error"] = message;
}
