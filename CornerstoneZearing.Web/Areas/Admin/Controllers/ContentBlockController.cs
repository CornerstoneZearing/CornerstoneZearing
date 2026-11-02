using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class ContentBlockController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;

    public ContentBlockController(CornerstoneDbContext db)
    {
        _db = db;
    }

    [HasPermission(Permissions.ContentBlocks.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "ContentBlocks";
        return View(await _db.ContentBlocks.OrderBy(s => s.Title).ToListAsync());
    }

    [HasPermission(Permissions.ContentBlocks.Create)]
    public IActionResult Create()
    {
        ViewData["Title"] = "New ContentBlock";
        return View("Edit", new ContentBlockEditView());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.ContentBlocks.Create)]
    public async Task<IActionResult> Create(ContentBlockEditView vm)
    {
        if (!ModelState.IsValid) return View("Edit", vm);
        var now = DateTime.UtcNow;
        _db.ContentBlocks.Add(new ContentBlock
        {
            Title = vm.Title.Trim(),
            ContentJson = vm.ContentJson,
            //ContentHtml = _renderer.Render(vm.ContentJson),
            DateCreated = now,
            DateModified = now,
        });
        await _db.SaveChangesAsync();
        Success("Content block created.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.ContentBlocks.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var s = await _db.ContentBlocks.FindAsync(id);
        if (s is null) return NotFound();
        ViewData["Title"] = "Edit Content Block";
        return View(new ContentBlockEditView { ContentBlockID = s.ContentBlockID, Title = s.Title, ContentJson = s.ContentJson });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.ContentBlocks.Edit)]
    public async Task<IActionResult> Edit(int id, ContentBlockEditView vm)
    {
        var s = await _db.ContentBlocks.FindAsync(id);
        if (s is null) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        s.Title = vm.Title.Trim();
        s.ContentJson = vm.ContentJson;
        //s.ContentHtml = _renderer.Render(vm.ContentJson);
        s.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        Success("Content block saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.ContentBlocks.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.ContentBlocks.FindAsync(id);
        if (s is null) return NotFound();
        _db.ContentBlocks.Remove(s);
        await _db.SaveChangesAsync();
        Success("Content block deleted.");
        return RedirectToIndex();
    }
}