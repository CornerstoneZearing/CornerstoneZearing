using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Data;

namespace CornerstoneZearing.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var homePage = await _context.Pages
            .FirstOrDefaultAsync(p => p.UrlSlug == "" && p.Status == PageStatus.Published);

        if (homePage == null)
        {
            return View("Index");
        }

        return await RenderPage(homePage);
    }

    public async Task<IActionResult> Render(string slug)
    {
        var page = await _context.Pages
            .FirstOrDefaultAsync(p => p.UrlSlug == slug && p.Status == PageStatus.Published);

        if (page == null)
        {
            return NotFound();
        }

        return await RenderPage(page);
    }

    private Task<IActionResult> RenderPage(Page page)
    {
        var templatePath = $"~/Views/Templates/{page.TemplateName}.cshtml";
        return Task.FromResult<IActionResult>(View(templatePath, page));
    }
}
