using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class MediaController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly FileStorageService _storage;

    public MediaController(CornerstoneDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HasPermission(Permissions.Media.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Media";
        var media = await _db.Media.OrderByDescending(m => m.DateCreated).ToListAsync();
        return View(media);
    }

    [HasPermission(Permissions.Media.View)]
    public async Task<IActionResult> Picker()
    {
        var media = await _db.Media.OrderByDescending(m => m.DateCreated).Take(60).ToListAsync();
        return PartialView("_PickerGrid", media);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Media.Upload)]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var count = 0;
        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (!_storage.IsAllowedImage(file.FileName) || file.Length > _storage.MaxImageBytes)
            {
                Error($"Skipped {file.FileName}: not an allowed image or too large.");
                continue;
            }

            var stored = await _storage.SaveAsync(file, "media");
            _db.Media.Add(new Media
            {
                OriginalFileName = stored.OriginalFileName,
                StoredFileName = stored.StoredFileName,
                ContentType = stored.ContentType,
                Width = stored.Width,
                Height = stored.Height,
                SizeBytes = (int)Math.Min(stored.SizeBytes, int.MaxValue),
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            });
            count++;
        }

        await _db.SaveChangesAsync();
        if (count > 0) Success($"Uploaded {count} file(s).");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Media.Edit)]
    public async Task<IActionResult> Update(int id, string? altText)
    {
        var media = await _db.Media.FindAsync(id);
        if (media is null) return NotFound();
        media.AltText = altText;
        media.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        Success("Media updated.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Media.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await _db.Media.FindAsync(id);
        if (media is null) return NotFound();

        if (await _db.Pages.AnyAsync(p => p.FeaturedMediaID == id) ||
            await _db.Posts.AnyAsync(p => p.FeaturedMediaID == id) ||
            await _db.Sermons.AnyAsync(s => s.FeaturedMediaID == id))
        {
            Error("This image is used as a featured image. Remove those references first.");
            return RedirectToIndex();
        }

        _storage.Delete(media.StoredFileName);
        _db.Media.Remove(media);
        await _db.SaveChangesAsync();
        Success("Media deleted.");
        return RedirectToIndex();
    }
}
