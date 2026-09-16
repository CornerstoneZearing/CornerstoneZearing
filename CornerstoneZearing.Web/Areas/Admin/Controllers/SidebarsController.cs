using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class SidebarsController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly EditorJsRenderer _renderer;

    public SidebarsController(CornerstoneDbContext db, EditorJsRenderer renderer)
    {
        _db = db;
        _renderer = renderer;
    }

    [HasPermission(Permissions.Sidebars.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Sidebars";
        return View(await _db.Sidebars.OrderBy(s => s.Title).ToListAsync());
    }

    [HasPermission(Permissions.Sidebars.Create)]
    public IActionResult Create()
    {
        ViewData["Title"] = "New Sidebar";
        return View("Edit", new SidebarEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Sidebars.Create)]
    public async Task<IActionResult> Create(SidebarEditViewModel vm)
    {
        if (!ModelState.IsValid) return View("Edit", vm);
        var now = DateTime.UtcNow;
        _db.Sidebars.Add(new Sidebar
        {
            Title = vm.Title.Trim(),
            ContentJson = vm.ContentJson,
            ContentHtml = _renderer.Render(vm.ContentJson),
            DateCreated = now,
            DateModified = now,
        });
        await _db.SaveChangesAsync();
        Success("Sidebar created.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.Sidebars.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var s = await _db.Sidebars.FindAsync(id);
        if (s is null) return NotFound();
        ViewData["Title"] = "Edit Sidebar";
        return View(new SidebarEditViewModel { SidebarID = s.SidebarID, Title = s.Title, ContentJson = s.ContentJson });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Sidebars.Edit)]
    public async Task<IActionResult> Edit(int id, SidebarEditViewModel vm)
    {
        var s = await _db.Sidebars.FindAsync(id);
        if (s is null) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        s.Title = vm.Title.Trim();
        s.ContentJson = vm.ContentJson;
        s.ContentHtml = _renderer.Render(vm.ContentJson);
        s.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        Success("Sidebar saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Sidebars.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Sidebars.FindAsync(id);
        if (s is null) return NotFound();
        if (await _db.Pages.AnyAsync(p => p.SidebarID == id))
        {
            Error("This sidebar is attached to one or more pages.");
            return RedirectToIndex();
        }
        _db.Sidebars.Remove(s);
        await _db.SaveChangesAsync();
        Success("Sidebar deleted.");
        return RedirectToIndex();
    }
}
