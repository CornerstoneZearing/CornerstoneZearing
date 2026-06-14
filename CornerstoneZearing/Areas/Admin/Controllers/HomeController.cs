using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _DbContext;

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="context"></param>
    public HomeController(ApplicationDbContext context)
    {
        _DbContext = context;
    }

    /// <summary>
    /// Admin dashboard page.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalPages = await _DbContext.Pages.CountAsync();
        ViewBag.PublishedPages = await _DbContext.Pages.CountAsync(p => p.Status == PageStatus.Published);
        ViewBag.DraftPages = await _DbContext.Pages.CountAsync(p => p.Status == PageStatus.Draft);
        ViewBag.WithdrawnPages = await _DbContext.Pages.CountAsync(p => p.Status == PageStatus.Withdrawn);
        return View();
    }
}