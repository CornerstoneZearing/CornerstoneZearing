using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class PagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var pages = await _context.Pages
            .OrderByDescending(p => p.DateModified)
            .ToListAsync();
        return View(pages);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new PageFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        if (await _context.Pages.AnyAsync(p => p.UrlSlug == model.UrlSlug))
        {
            ModelState.AddModelError("UrlSlug", "This URL slug is already in use.");
            return View("Form", model);
        }

        var page = new Page
        {
            PageID = Guid.NewGuid(),
            Name = model.Name,
            Content = model.Content,
            TemplateName = model.TemplateName,
            UrlSlug = model.UrlSlug,
            MetaTitle = model.MetaTitle,
            MetaDescription = model.MetaDescription,
            Status = model.Status,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        _context.Pages.Add(page);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Page \"{page.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var page = await _context.Pages.FindAsync(id);
        if (page == null) return NotFound();

        return View("Form", new PageFormViewModel
        {
            PageID = page.PageID,
            Name = page.Name,
            Content = page.Content,
            TemplateName = page.TemplateName,
            UrlSlug = page.UrlSlug,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            Status = page.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PageFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        if (await _context.Pages.AnyAsync(p => p.UrlSlug == model.UrlSlug && p.PageID != model.PageID))
        {
            ModelState.AddModelError("UrlSlug", "This URL slug is already in use.");
            return View("Form", model);
        }

        var page = await _context.Pages.FindAsync(model.PageID);
        if (page == null) return NotFound();

        page.Name = model.Name;
        page.Content = model.Content;
        page.TemplateName = model.TemplateName;
        page.UrlSlug = model.UrlSlug;
        page.MetaTitle = model.MetaTitle;
        page.MetaDescription = model.MetaDescription;
        page.Status = model.Status;
        page.DateModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Page \"{page.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var page = await _context.Pages.FindAsync(id);
        if (page == null) return NotFound();
        return View(page);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var page = await _context.Pages.FindAsync(id);
        if (page == null) return NotFound();

        _context.Pages.Remove(page);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Page deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
