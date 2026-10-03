using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Data.Enums;
using CornerstoneZearing.Web.Models;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Controllers;

public class PagesController : Controller
{
    private readonly CornerstoneDbContext _db;
    private readonly PageTemplateService _templates;

    public PagesController(CornerstoneDbContext db, PageTemplateService templates)
    {
        _db = db;
        _templates = templates;
    }

    public async Task<IActionResult> Show(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return RedirectToAction("Index", "Home");

        // Walk the slug segments down the page tree: each segment must be a child of the previous one.
        var segments = slug.Split('/', StringSplitOptions.RemoveEmptyEntries);
        int? parentId = null;
        Page? page = null;
        foreach (var segment in segments)
        {
            var id = parentId;
            page = await _db.Pages
                .Include(p => p.Sidebar)
                .Include(p => p.FeaturedMedia)
                .FirstOrDefaultAsync(p => p.ParentPageID == id && p.Slug == segment && p.Status == ContentStatus.Published);
            if (page is null)
                return NotFound();
            parentId = page.PageID;
        }

        if (page is null)
            return NotFound();

        var vm = new PageViewModel { Page = page, Sidebar = page.Sidebar, FeaturedMedia = page.FeaturedMedia };
        return base.View(_templates.ResolveViewName(page.Template), vm);
    }
}
