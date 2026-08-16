using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;

namespace CornerstoneZearing.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _Context;

    public HomeController(ApplicationDbContext context)
    {
        _Context = context;
    }

    public async Task<IActionResult> Index()
    {
        var homePage = await _Context.Pages
            .FirstOrDefaultAsync(p => p.UrlSlug == "" && p.Status == PageStatus.Published);

        if (homePage == null)
        {
            return View("Index");
        }

        return await RenderPage(homePage);
    }

    public async Task<IActionResult> Render(string slug)
    {
        var segments = (slug ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return NotFound();
        }

        Page? page = null;
        foreach (var segment in segments)
        {
            var parentID = page?.PageID;
            page = await _Context.Pages.FirstOrDefaultAsync(p =>
                p.UrlSlug == segment &&
                p.ParentPageID == parentID &&
                p.Status == PageStatus.Published);

            if (page == null)
            {
                return NotFound();
            }
        }

        return await RenderPage(page!);
    }

    private Task<IActionResult> RenderPage(Page page)
    {
        var templatePath = $"~/Views/Templates/{page.TemplateName}.cshtml";
        return Task.FromResult<IActionResult>(View(templatePath, page));
    }
}
