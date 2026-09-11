using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class DocumentsController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;
    private readonly FileStorageService _storage;

    public DocumentsController(CornerstoneDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HasPermission(Permissions.Documents.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Documents";
        return View(await _db.Documents.OrderByDescending(d => d.DateCreated).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Documents.Upload)]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var count = 0;
        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (!_storage.IsAllowedDocument(file.FileName) || file.Length > _storage.MaxDocumentBytes)
            {
                Error($"Skipped {file.FileName}: not an allowed document type or too large.");
                continue;
            }

            var stored = await _storage.SaveAsync(file, "documents");
            _db.Documents.Add(new Document
            {
                OriginalFileName = stored.OriginalFileName,
                StoredFileName = stored.StoredFileName,
                ContentType = stored.ContentType,
                SizeBytes = (int)Math.Min(stored.SizeBytes, int.MaxValue),
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            });
            count++;
        }

        await _db.SaveChangesAsync();
        if (count > 0) Success($"Uploaded {count} document(s).");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Documents.Edit)]
    public async Task<IActionResult> Update(int id, string? description)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();
        doc.Description = description;
        doc.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        Success("Document updated.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Documents.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();

        if (await _db.Sermons.AnyAsync(s => s.FeaturedDocumentID == id))
        {
            Error("This document is attached to a sermon. Remove that reference first.");
            return RedirectToIndex();
        }

        _storage.Delete(doc.StoredFileName);
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        Success("Document deleted.");
        return RedirectToIndex();
    }
}
