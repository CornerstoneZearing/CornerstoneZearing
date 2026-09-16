using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Data.Enums;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class PostsController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly EditorJsRenderer _renderer;
    private readonly SlugService _slugs;

    public PostsController(CornerstoneDbContext db, EditorJsRenderer renderer, SlugService slugs)
    {
        _db = db;
        _renderer = renderer;
        _slugs = slugs;
    }

    [HasPermission(Permissions.Posts.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Posts";
        var posts = await _db.Posts.Include(p => p.PostCategory)
            .OrderByDescending(p => p.DateCreated).ToListAsync();
        return View(posts);
    }

    [HasPermission(Permissions.Posts.Create)]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Post";
        var vm = new PostEditViewModel();
        await PopulateAsync(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Posts.Create)]
    public async Task<IActionResult> Create(PostEditViewModel vm)
    {
        if (!ModelState.IsValid) { await PopulateAsync(vm); return View("Edit", vm); }

        var post = new Post();
        await ApplyAsync(vm, post, true);
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        Success("Post created.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.Posts.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();

        ViewData["Title"] = "Edit Post";
        var vm = new PostEditViewModel
        {
            PostID = post.PostID,
            Title = post.Title,
            Slug = post.Slug,
            PostCategoryID = post.PostCategoryID,
            FeaturedMediaID = post.FeaturedMediaID,
            Summary = post.Summary,
            ContentJson = post.ContentJson,
            Status = post.Status,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            Tags = post.Tags,
            DatePublished = post.DatePublished,
        };
        await PopulateAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Posts.Edit)]
    public async Task<IActionResult> Edit(int id, PostEditViewModel vm)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();
        if (!ModelState.IsValid) { await PopulateAsync(vm); return View(vm); }

        await ApplyAsync(vm, post, false);
        await _db.SaveChangesAsync();
        Success("Post saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Posts.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        Success("Post deleted.");
        return RedirectToIndex();
    }

    private async Task ApplyAsync(PostEditViewModel vm, Post post, bool isNew)
    {
        var now = DateTime.UtcNow;
        post.Title = vm.Title.Trim();
        post.Slug = await _slugs.UniqueSlugAsync(
            string.IsNullOrWhiteSpace(vm.Slug) ? vm.Title : vm.Slug!,
            async slug => !await _db.Posts.AnyAsync(p => p.Slug == slug && p.PostID != post.PostID),
            isNew ? null : post.Slug);
        post.PostCategoryID = vm.PostCategoryID;
        post.FeaturedMediaID = vm.FeaturedMediaID;
        post.Summary = vm.Summary;
        post.ContentJson = vm.ContentJson;
        post.ContentHtml = _renderer.Render(vm.ContentJson);
        post.Status = vm.Status;
        post.MetaTitle = vm.MetaTitle;
        post.MetaDescription = vm.MetaDescription;
        post.Tags = vm.Tags;
        post.DatePublished = vm.DatePublished ?? (vm.Status == ContentStatus.Published ? now : null);
        post.DateModified = now;
        if (isNew) post.DateCreated = now;
    }

    private async Task PopulateAsync(PostEditViewModel vm)
    {
        var categories = await _db.PostCategories.OrderBy(c => c.Name)
            .Select(c => new { c.PostCategoryID, c.Name }).ToListAsync();
        vm.CategoryOptions = new SelectList(categories, "PostCategoryID", "Name", vm.PostCategoryID);

        if (vm.FeaturedMediaID is { } mediaId && await _db.Media.AnyAsync(m => m.MediaID == mediaId))
            vm.FeaturedMediaUrl = Url.Action("File", "MediaFiles", new { area = "", id = mediaId });
    }
}
