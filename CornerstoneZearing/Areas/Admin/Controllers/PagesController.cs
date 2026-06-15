using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class PagesController : Controller
{
    private readonly ApplicationDbContext _DbContext;

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="context"></param>
    public PagesController(ApplicationDbContext context)
    {
        _DbContext = context;
    }

    /// <summary>
    /// List page.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var pages = await _DbContext.Pages
            .OrderByDescending(p => p.DateModified)
            .ToListAsync();
        return View(pages);
    }

    /// <summary>
    /// Create page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new PageFormModel());
    }

    /// <summary>
    /// Creates a new page.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        if (await _DbContext.Pages.AnyAsync(p => p.UrlSlug == model.UrlSlug))
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

        _DbContext.Pages.Add(page);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Page \"{page.Name}\" created successfully.";
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
        var page = await _DbContext.Pages.FindAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        return View("Form", new PageFormModel
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

    /// <summary>
    /// Updates a page.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PageFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        if (await _DbContext.Pages.AnyAsync(p => p.UrlSlug == model.UrlSlug && p.PageID != model.PageID))
        {
            ModelState.AddModelError("UrlSlug", "This URL slug is already in use.");
            return View("Form", model);
        }

        var page = await _DbContext.Pages.FindAsync(model.PageID);
        if (page == null)
        {
            return NotFound();
        }

        page.Name = model.Name;
        page.Content = model.Content;
        page.TemplateName = model.TemplateName;
        page.UrlSlug = model.UrlSlug;
        page.MetaTitle = model.MetaTitle;
        page.MetaDescription = model.MetaDescription;
        page.Status = model.Status;
        page.DateModified = DateTime.UtcNow;
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Page \"{page.Name}\" updated successfully.";
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
        var page = await _DbContext.Pages.FindAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        return View(page);
    }

    /// <summary>
    /// Deletes a page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var page = await _DbContext.Pages.FindAsync(id);
        if (page == null)
        {
            return NotFound();
        }

        _DbContext.Pages.Remove(page);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = "Page deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}