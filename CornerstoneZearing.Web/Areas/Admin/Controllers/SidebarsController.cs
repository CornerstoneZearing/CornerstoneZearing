using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class SidebarsController : Controller
{
    private readonly ApplicationDbContext _DbContext;

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="context"></param>
    public SidebarsController(ApplicationDbContext context)
    {
        _DbContext = context;
    }

    /// <summary>
    /// List sidebars.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var sidebars = await _DbContext.Sidebars
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(sidebars);
    }

    /// <summary>
    /// Create sidebar.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View("Form", new SidebarFormModel());
    }

    /// <summary>
    /// Creates a new sidebar.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SidebarFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var sidebar = new Sidebar
        {
            SidebarID = Guid.NewGuid(),
            Name = model.Name,
            ContentHtml = model.ContentHtml,
            ContentJson = model.ContentJson,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        _DbContext.Sidebars.Add(sidebar);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"The sidebar \"{sidebar.Name}\" was created successfully.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Edit sidebar.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var sidebar = await _DbContext.Sidebars.FindAsync(id);
        if (sidebar == null)
        {
            return NotFound();
        }

        return View("Form", new SidebarFormModel
        {
            SidebarID = sidebar.SidebarID,
            Name = sidebar.Name,
            ContentHtml = sidebar.ContentHtml,
            ContentJson = sidebar.ContentJson
        });
    }

    /// <summary>
    /// Updates a sidebar.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SidebarFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }
        
        var sidebar = await _DbContext.Sidebars.FindAsync(model.SidebarID);
        if (sidebar == null)
        {
            return NotFound();
        }

        sidebar.Name = model.Name;
        sidebar.ContentHtml = model.ContentHtml;
        sidebar.ContentJson = model.ContentJson;
        sidebar.DateModified = DateTime.UtcNow;
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"The sidebar \"{sidebar.Name}\" was updated successfully.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Delete confirmation page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sidebar = await _DbContext.Sidebars.FindAsync(id);
        if (sidebar == null)
        {
            return NotFound();
        }

        return View(sidebar);
    }

    /// <summary>
    /// Deletes a sidebar.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var sidebar = await _DbContext.Sidebars.FindAsync(id);
        if (sidebar == null)
        {
            return NotFound();
        }

        _DbContext.Sidebars.Remove(sidebar);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"The sidebar \"{sidebar.Name}\" was deleted successfully.";
        return RedirectToAction("Index");
    }
}