using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Data;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalPages = await _context.Pages.CountAsync();
        ViewBag.PublishedPages = await _context.Pages.CountAsync(p => p.Status == PageStatus.Published);
        ViewBag.DraftPages = await _context.Pages.CountAsync(p => p.Status == PageStatus.Draft);
        ViewBag.WithdrawnPages = await _context.Pages.CountAsync(p => p.Status == PageStatus.Withdrawn);
        return View();
    }
}
