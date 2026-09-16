using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class PagesController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly EditorJsRenderer _renderer;
    private readonly SlugService _slugs;
    private readonly PageTemplateService _templates;
    private readonly NavigationService _navigation;

    public PagesController(CornerstoneDbContext db, EditorJsRenderer renderer, SlugService slugs,
        PageTemplateService templates, NavigationService navigation)
    {
        _db = db;
        _renderer = renderer;
        _slugs = slugs;
        _templates = templates;
        _navigation = navigation;
    }

    [HasPermission(Permissions.Pages.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Pages";
        var pages = await _db.Pages
            .Include(p => p.ParentPage)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Title)
            .ToListAsync();
        return View(pages);
    }

    [HasPermission(Permissions.Pages.Create)]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Page";
        var vm = new PageEditViewModel();
        await PopulateOptionsAsync(vm, null);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Pages.Create)]
    public async Task<IActionResult> Create(PageEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(vm, null);
            return View("Edit", vm);
        }

        var page = new Page();
        await ApplyAsync(vm, page, isNew: true);
        _db.Pages.Add(page);
        await _db.SaveChangesAsync();
        _navigation.Invalidate();
        Success("Page created.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.Pages.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page is null) return NotFound();

        ViewData["Title"] = "Edit Page";
        var vm = new PageEditViewModel
        {
            PageID = page.PageID,
            Title = page.Title,
            Slug = page.Slug,
            ParentPageID = page.ParentPageID,
            SidebarID = page.SidebarID,
            FeaturedMediaID = page.FeaturedMediaID,
            Template = page.Template,
            ContentJson = page.ContentJson,
            Status = page.Status,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            SortOrder = page.SortOrder,
            ShowInNavigation = page.ShowInNavigation,
        };
        await PopulateOptionsAsync(vm, page.PageID);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Pages.Edit)]
    public async Task<IActionResult> Edit(int id, PageEditViewModel vm)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page is null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(vm, id);
            return View(vm);
        }

        await ApplyAsync(vm, page, isNew: false);
        await _db.SaveChangesAsync();
        _navigation.Invalidate();
        Success("Page saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Pages.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var page = await _db.Pages.FindAsync(id);
        if (page is null) return NotFound();

        if (await _db.Pages.AnyAsync(p => p.ParentPageID == id))
        {
            Error("Remove or reassign child pages first.");
            return RedirectToIndex();
        }

        _db.Pages.Remove(page);
        await _db.SaveChangesAsync();
        _navigation.Invalidate();
        Success("Page deleted.");
        return RedirectToIndex();
    }

    private async Task ApplyAsync(PageEditViewModel vm, Page page, bool isNew)
    {
        var now = DateTime.UtcNow;
        page.Title = vm.Title.Trim();
        page.Slug = await _slugs.UniqueSlugAsync(
            string.IsNullOrWhiteSpace(vm.Slug) ? vm.Title : vm.Slug!,
            async slug => !await _db.Pages.AnyAsync(p => p.Slug == slug && p.PageID != page.PageID),
            isNew ? null : page.Slug);
        page.ParentPageID = vm.ParentPageID;
        page.SidebarID = vm.SidebarID;
        page.FeaturedMediaID = vm.FeaturedMediaID;
        page.Template = vm.Template;
        page.ContentJson = vm.ContentJson;
        page.ContentHtml = _renderer.Render(vm.ContentJson);
        page.Status = vm.Status;
        page.MetaTitle = vm.MetaTitle;
        page.MetaDescription = vm.MetaDescription;
        page.SortOrder = vm.SortOrder;
        page.ShowInNavigation = vm.ShowInNavigation;
        page.DateModified = now;
        if (isNew) page.DateCreated = now;
    }

    private async Task PopulateOptionsAsync(PageEditViewModel vm, int? excludeId)
    {
        var pages = await _db.Pages
            .Where(p => excludeId == null || p.PageID != excludeId)
            .OrderBy(p => p.Title)
            .Select(p => new { p.PageID, p.Title })
            .ToListAsync();
        vm.ParentOptions = new SelectList(pages, "PageID", "Title", vm.ParentPageID);

        var sidebars = await _db.Sidebars.OrderBy(s => s.Title)
            .Select(s => new { s.SidebarID, s.Title }).ToListAsync();
        vm.SidebarOptions = new SelectList(sidebars, "SidebarID", "Title", vm.SidebarID);

        vm.TemplateOptions = new SelectList(_templates.GetTemplateNames(), vm.Template);

        if (vm.FeaturedMediaID is { } mediaId)
        {
            var media = await _db.Media.FindAsync(mediaId);
            if (media != null)
                vm.FeaturedMediaUrl = Url.Action("File", "MediaFiles", new { area = "", id = media.MediaID });
        }
    }
}
