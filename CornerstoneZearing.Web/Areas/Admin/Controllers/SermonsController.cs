using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class SermonsController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly EditorJsRenderer _renderer;
    private readonly SlugService _slugs;

    public SermonsController(CornerstoneDbContext db, EditorJsRenderer renderer, SlugService slugs)
    {
        _db = db;
        _renderer = renderer;
        _slugs = slugs;
    }

    [HasPermission(Permissions.Sermons.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Sermons";
        var sermons = await _db.Sermons.Include(s => s.SermonCategory)
            .OrderByDescending(s => s.SermonDate).ToListAsync();
        return View(sermons);
    }

    [HasPermission(Permissions.Sermons.Create)]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Sermon";
        var vm = new SermonEditViewModel();
        await PopulateAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Sermons.Create)]
    public async Task<IActionResult> Create(SermonEditViewModel vm)
    {
        if (!ModelState.IsValid) { await PopulateAsync(vm); return View("Edit", vm); }

        var sermon = new Sermon();
        Apply(vm, sermon, true);
        _db.Sermons.Add(sermon);
        await _db.SaveChangesAsync();
        Success("Sermon created.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.Sermons.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var s = await _db.Sermons.FindAsync(id);
        if (s is null) return NotFound();

        ViewData["Title"] = "Edit Sermon";
        var vm = new SermonEditViewModel
        {
            SermonID = s.SermonID,
            Title = s.Title,
            SermonCategoryID = s.SermonCategoryID,
            FeaturedDocumentID = s.FeaturedDocumentID,
            FeaturedMediaID = s.FeaturedMediaID,
            SermonDate = s.SermonDate,
            Speaker = s.Speaker,
            ContentJson = s.ContentJson,
            Status = s.Status,
        };
        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Sermons.Edit)]
    public async Task<IActionResult> Edit(int id, SermonEditViewModel vm)
    {
        var s = await _db.Sermons.FindAsync(id);
        if (s is null) return NotFound();
        if (!ModelState.IsValid) { await PopulateAsync(vm); return View(vm); }

        Apply(vm, s, false);
        await _db.SaveChangesAsync();
        Success("Sermon saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Sermons.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Sermons.FindAsync(id);
        if (s is null) return NotFound();
        _db.Sermons.Remove(s);
        await _db.SaveChangesAsync();
        Success("Sermon deleted.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.Categories.Manage)]
    public async Task<IActionResult> Categories()
    {
        var categories = await _db.SermonCategories.OrderBy(c => c.Name).ToListAsync();
        return View(categories);
    }

    [HasPermission(Permissions.Categories.Manage)]
    public IActionResult CategoryCreate()
    {
        ViewData["Title"] = "New Sermon Category";
        return View("CategoryEdit", new CategoryEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Categories.Manage)]
    public async Task<IActionResult> CategoryCreate(CategoryEditViewModel vm)
    {
        if (!ModelState.IsValid) return View("CategoryEdit", vm);

        var now = DateTime.UtcNow;
        _db.SermonCategories.Add(new SermonCategory
        {
            Name = vm.Name.Trim(),
            Slug = await _slugs.UniqueSlugAsync(string.IsNullOrWhiteSpace(vm.Slug) ? vm.Name : vm.Slug!,
                async s => !await _db.SermonCategories.AnyAsync(c => c.Slug == s)),
            Description = vm.Description,
            DateCreated = now,
            DateModified = now,
        });
        await _db.SaveChangesAsync();
        Success("Category created.");
        return RedirectToAction(nameof(Categories));
    }

    [HasPermission(Permissions.Categories.Manage)]
    public async Task<IActionResult> CategoryEdit(int id)
    {
        var c = await _db.SermonCategories.FindAsync(id);
        if (c is null) return NotFound();
        ViewData["Title"] = "Edit Sermon Category";
        return View("CategoryEdit", new CategoryEditViewModel { Id = c.SermonCategoryID, Name = c.Name, Slug = c.Slug, Description = c.Description });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Categories.Manage)]
    public async Task<IActionResult> CategoryEdit(int id, CategoryEditViewModel vm)
    {
        var c = await _db.SermonCategories.FindAsync(id);
        if (c is null) return NotFound();
        if (!ModelState.IsValid) return View("CategoryEdit", vm);

        c.Name = vm.Name.Trim();
        c.Slug = await _slugs.UniqueSlugAsync(string.IsNullOrWhiteSpace(vm.Slug) ? vm.Name : vm.Slug!,
            async s => !await _db.SermonCategories.AnyAsync(x => x.Slug == s && x.SermonCategoryID != id), c.Slug);
        c.Description = vm.Description;
        c.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        Success("Category saved.");
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Categories.Manage)]
    public async Task<IActionResult> CategoryDelete(int id)
    {
        var c = await _db.SermonCategories.FindAsync(id);
        if (c is null) return NotFound();
        if (await _db.Sermons.AnyAsync(s => s.SermonCategoryID == id))
        {
            Error("Reassign sermons in this category first.");
            return RedirectToAction(nameof(Categories));
        }
        _db.SermonCategories.Remove(c);
        await _db.SaveChangesAsync();
        Success("Category deleted.");
        return RedirectToAction(nameof(Categories));
    }

    private void Apply(SermonEditViewModel vm, Sermon sermon, bool isNew)
    {
        var now = DateTime.UtcNow;
        sermon.Title = vm.Title.Trim();
        sermon.SermonCategoryID = vm.SermonCategoryID;
        sermon.FeaturedDocumentID = vm.FeaturedDocumentID;
        sermon.FeaturedMediaID = vm.FeaturedMediaID;
        sermon.SermonDate = vm.SermonDate;
        sermon.Speaker = vm.Speaker;
        sermon.ContentJson = vm.ContentJson;
        sermon.ContentHtml = _renderer.Render(vm.ContentJson);
        sermon.Status = vm.Status;
        sermon.DateModified = now;
        if (isNew) sermon.DateCreated = now;
    }

    private async Task PopulateAsync(SermonEditViewModel vm)
    {
        var categories = await _db.SermonCategories.OrderBy(c => c.Name)
            .Select(c => new { c.SermonCategoryID, c.Name }).ToListAsync();
        vm.CategoryOptions = new SelectList(categories, "SermonCategoryID", "Name", vm.SermonCategoryID);

        var docs = await _db.Documents.OrderByDescending(d => d.DateCreated)
            .Select(d => new { d.DocumentID, d.OriginalFileName }).ToListAsync();
        vm.DocumentOptions = new SelectList(docs, "DocumentID", "OriginalFileName", vm.FeaturedDocumentID);

        if (vm.FeaturedMediaID is { } mediaId && await _db.Media.AnyAsync(m => m.MediaID == mediaId))
            vm.FeaturedMediaUrl = Url.Action("File", "MediaFiles", new { area = "", id = mediaId });
    }
}
