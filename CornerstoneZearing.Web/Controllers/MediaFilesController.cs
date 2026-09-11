using CornerstoneZearing.Data;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CornerstoneZearing.Web.Controllers;

[Route("media-files")]
public class MediaFilesController : Controller
{
    private readonly CornerstoneDbContext _db;
    private readonly FileStorageService _storage;

    public MediaFilesController(CornerstoneDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet("image/{id:int}")]
    [ResponseCache(Duration = 604800, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> File(int id)
    {
        var media = await _db.Media.FindAsync(id);
        if (media is null || !_storage.Exists(media.StoredFileName))
            return NotFound();

        var path = _storage.GetAbsolutePath(media.StoredFileName);
        return PhysicalFile(path, media.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("document/{id:int}")]
    public async Task<IActionResult> Document(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null || !_storage.Exists(doc.StoredFileName))
            return NotFound();

        var path = _storage.GetAbsolutePath(doc.StoredFileName);
        return PhysicalFile(path, doc.ContentType, doc.OriginalFileName, enableRangeProcessing: true);
    }
}
