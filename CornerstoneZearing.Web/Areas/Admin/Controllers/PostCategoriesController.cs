using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

[HasPermission(Permissions.Categories.Manage)]
public class PostCategoriesController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly SlugService _slugs;

    public PostCategoriesController(CornerstoneDbContext db, SlugService slugs)
    {
        _db = db;
        _slugs = slugs;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Post categories";
        var categories = await _db.PostCategories.OrderBy(c => c.Name).ToListAsync();
        return View("CategoryIndex", categories);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "New post category";
        return View("CategoryEdit", new CategoryEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryEditViewModel vm)
    {
        if (!ModelState.IsValid) return View("CategoryEdit", vm);

        var now = DateTime.UtcNow;
        _db.PostCategories.Add(new PostCategory
        {
            Name = vm.Name.Trim(),
            Slug = await _slugs.UniqueSlugAsync(string.IsNullOrWhiteSpace(vm.Slug) ? vm.Name : vm.Slug!,
                async s => !await _db.PostCategories.AnyAsync(c => c.Slug == s)),
            Description = vm.Description,
            DateCreated = now,
            DateModified = now,
        });
        await _db.SaveChangesAsync();
        Success("Category created.");
        return RedirectToIndex();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _db.PostCategories.FindAsync(id);
        if (c is null) return NotFound();
        ViewData["Title"] = "Edit post category";
        return View("CategoryEdit", new CategoryEditViewModel { Id = c.PostCategoryID, Name = c.Name, Slug = c.Slug, Description = c.Description });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryEditViewModel vm)
    {
        var c = await _db.PostCategories.FindAsync(id);
        if (c is null) return NotFound();
        if (!ModelState.IsValid) return View("CategoryEdit", vm);

        c.Name = vm.Name.Trim();
        c.Slug = await _slugs.UniqueSlugAsync(string.IsNullOrWhiteSpace(vm.Slug) ? vm.Name : vm.Slug!,
            async s => !await _db.PostCategories.AnyAsync(x => x.Slug == s && x.PostCategoryID != id), c.Slug);
        c.Description = vm.Description;
        c.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        Success("Category saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.PostCategories.FindAsync(id);
        if (c is null) return NotFound();
        _db.PostCategories.Remove(c);
        await _db.SaveChangesAsync();
        Success("Category deleted.");
        return RedirectToIndex();
    }
}
