using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

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

        var byId = pages.ToDictionary(p => p.PageID);
        ViewBag.FullPaths = pages.ToDictionary(p => p.PageID, p => BuildFullPath(p, byId));

        return View(pages);
    }

    /// <summary>
    /// Create page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateParentOptionsAsync(excludePageId: null);
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
            await PopulateParentOptionsAsync(excludePageId: null);
            return View("Form", model);
        }

        if (await _DbContext.Pages.AnyAsync(p => p.UrlSlug == model.UrlSlug && p.ParentPageID == model.ParentPageID))
        {
            ModelState.AddModelError("UrlSlug", "This URL slug is already in use under the selected parent page.");
            await PopulateParentOptionsAsync(excludePageId: null);
            return View("Form", model);
        }

        var page = new Page
        {
            PageID = Guid.NewGuid(),
            Name = model.Name,
            ContentHtml = model.ContentHtml,
            ContentJson = model.ContentJson,
            TemplateName = model.TemplateName,
            UrlSlug = model.UrlSlug,
            ParentPageID = model.ParentPageID,
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

        await PopulateParentOptionsAsync(excludePageId: page.PageID);

        return View("Form", new PageFormModel
        {
            PageID = page.PageID,
            Name = page.Name,
            ContentHtml = page.ContentHtml,
            ContentJson = page.ContentJson,
            TemplateName = page.TemplateName,
            UrlSlug = page.UrlSlug,
            ParentPageID = page.ParentPageID,
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
            await PopulateParentOptionsAsync(excludePageId: model.PageID);
            return View("Form", model);
        }

        if (await _DbContext.Pages.AnyAsync(p => p.UrlSlug == model.UrlSlug && p.ParentPageID == model.ParentPageID && p.PageID != model.PageID))
        {
            ModelState.AddModelError("UrlSlug", "This URL slug is already in use under the selected parent page.");
            await PopulateParentOptionsAsync(excludePageId: model.PageID);
            return View("Form", model);
        }

        if (model.ParentPageID.HasValue)
        {
            var allPages = await _DbContext.Pages.ToListAsync();
            var byId = allPages.ToDictionary(p => p.PageID);
            if (model.ParentPageID == model.PageID || IsDescendant(model.ParentPageID.Value, model.PageID, byId))
            {
                ModelState.AddModelError("ParentPageID", "A page cannot be nested under itself or one of its own child pages.");
                await PopulateParentOptionsAsync(excludePageId: model.PageID);
                return View("Form", model);
            }
        }

        var page = await _DbContext.Pages.FindAsync(model.PageID);
        if (page == null)
        {
            return NotFound();
        }

        page.Name = model.Name;
        page.ContentHtml = model.ContentHtml;
        page.ContentJson = model.ContentJson;
        page.TemplateName = model.TemplateName;
        page.UrlSlug = model.UrlSlug;
        page.ParentPageID = model.ParentPageID;
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

        if (await _DbContext.Pages.AnyAsync(p => p.ParentPageID == id))
        {
            TempData["Error"] = $"\"{page.Name}\" has child pages. Move or delete them first.";
            return RedirectToAction(nameof(Index));
        }

        var allPages = await _DbContext.Pages.ToListAsync();
        ViewBag.FullPath = BuildFullPath(page, allPages.ToDictionary(p => p.PageID));

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

        if (await _DbContext.Pages.AnyAsync(p => p.ParentPageID == id))
        {
            TempData["Error"] = $"\"{page.Name}\" has child pages. Move or delete them first.";
            return RedirectToAction(nameof(Index));
        }

        _DbContext.Pages.Remove(page);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = "Page deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Builds the "/"-joined URL path for a page by walking its parent chain.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="byId"></param>
    /// <returns></returns>
    private static string BuildFullPath(Page page, Dictionary<Guid, Page> byId)
    {
        var segments = new List<string>();
        Page? current = page;
        var guard = 0;

        while (current != null && guard++ < 50)
        {
            if (!string.IsNullOrEmpty(current.UrlSlug))
            {
                segments.Insert(0, current.UrlSlug);
            }
            current = current.ParentPageID.HasValue && byId.TryGetValue(current.ParentPageID.Value, out var parent) ? parent : null;
        }

        return string.Join("/", segments);
    }

    /// <summary>
    /// Determines whether the page identified by <paramref name="candidateId"/> is <paramref name="ancestorId"/> itself
    /// or nested (at any depth) under it.
    /// </summary>
    /// <param name="candidateId"></param>
    /// <param name="ancestorId"></param>
    /// <param name="byId"></param>
    /// <returns></returns>
    private static bool IsDescendant(Guid candidateId, Guid ancestorId, Dictionary<Guid, Page> byId)
    {
        Guid? current = candidateId;
        var guard = 0;

        while (current.HasValue && guard++ < 50)
        {
            if (current.Value == ancestorId)
            {
                return true;
            }
            current = byId.TryGetValue(current.Value, out var page) ? page.ParentPageID : null;
        }

        return false;
    }

    /// <summary>
    /// Populates <c>ViewBag.ParentPages</c> with the pages eligible to be selected as a parent,
    /// excluding the page being edited and any of its own descendants (which would create a cycle).
    /// </summary>
    /// <param name="excludePageId"></param>
    /// <returns></returns>
    private async Task PopulateParentOptionsAsync(Guid? excludePageId)
    {
        var pages = await _DbContext.Pages.OrderBy(p => p.Name).ToListAsync();
        var byId = pages.ToDictionary(p => p.PageID);

        var candidates = excludePageId.HasValue
            ? pages.Where(p => p.PageID != excludePageId.Value && !IsDescendant(p.PageID, excludePageId.Value, byId))
            : pages;

        var items = candidates
            .Select(p => new { p.PageID, Path = "/" + BuildFullPath(p, byId) })
            .OrderBy(p => p.Path)
            .ToList();

        ViewBag.ParentPages = new SelectList(items, "PageID", "Path");
    }
}