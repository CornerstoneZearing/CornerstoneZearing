using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class DocumentsController : Controller
{
    private const long _MaxFileSizeBytes = 25 * 1024 * 1024;
    private static readonly string[] _AllowedExtensions = [".pdf", ".docx", ".xlsx", ".pptx", ".txt"];
    private readonly ApplicationDbContext _DbContext;
    private readonly IWebHostEnvironment _Environment;

    private string UploadsPath => Path.Combine(_Environment.WebRootPath, "uploads", "documents");

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="environment"></param>
    public DocumentsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _DbContext = context;
        _Environment = environment;
    }

    /// <summary>
    /// List page.
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public async Task<IActionResult> Index(string? search)
    {
        var query = _DbContext.MediaDocuments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.OriginalFileName.Contains(term) || d.Description.Contains(term));
        }

        var docs = await query.OrderByDescending(d => d.DateUploaded).ToListAsync();
        ViewBag.Search = search?.Trim() ?? string.Empty;
        return View(docs);
    }

    /// <summary>
    /// Handle document uploads.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return UploadError("No file was provided.");
        }

        if (file.Length > _MaxFileSizeBytes)
        {
            return UploadError("File exceeds the 25 MB maximum size.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_AllowedExtensions.Contains(extension))
        {
            return UploadError("Only PDF, DOCX, XLSX, PPTX, and TXT files are allowed.");
        }

        await using var stream = file.OpenReadStream();
        if (!await IsValidDocumentAsync(stream, extension))
        {
            return UploadError("The file content does not match the expected format.");
        }

        stream.Position = 0;
        Directory.CreateDirectory(UploadsPath);

        var storedFileName = UniqueFileName(Path.GetFileNameWithoutExtension(file.FileName), extension);
        var filePath = Path.Combine(UploadsPath, storedFileName);
        await using (var output = System.IO.File.Create(filePath))
        {
            await stream.CopyToAsync(output);
        }

        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };

        var userID = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : Guid.Empty;

        var doc = new MediaDocument
        {
            MediaDocumentID = Guid.NewGuid(),
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSize = file.Length,
            Description = string.Empty,
            DateUploaded = DateTime.UtcNow,
            UploadedByUserID = userID
        };

        _DbContext.MediaDocuments.Add(doc);
        await _DbContext.SaveChangesAsync();

        if (IsAjaxRequest())
        {
            return Json(new { success = true, id = doc.MediaDocumentID, url = $"/uploads/documents/{doc.StoredFileName}", name = doc.OriginalFileName, size = doc.FileSize, type = doc.ContentType });
        }

        TempData["Success"] = $"\"{doc.OriginalFileName}\" uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Updates the description for a document.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDescription(Guid id, string? description)
    {
        var doc = await _DbContext.MediaDocuments.FindAsync(id);
        if (doc == null) return NotFound();
        doc.Description = (description ?? string.Empty).Trim();
        await _DbContext.SaveChangesAsync();
        return Json(new { success = true });
    }

    /// <summary>
    /// Delete confirmation page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var doc = await _DbContext.MediaDocuments.FindAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var doc = await _DbContext.MediaDocuments.FindAsync(id);
        if (doc == null) return NotFound();

        var filePath = Path.Combine(UploadsPath, doc.StoredFileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _DbContext.MediaDocuments.Remove(doc);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"\"{doc.OriginalFileName}\" deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Deal with upload errors.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private IActionResult UploadError(string message)
    {
        if (IsAjaxRequest())
        {
            return Json(new { success = false, error = message });
        }

        TempData["Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Generates a unique file name for uploaded document.
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    private string UniqueFileName(string baseName, string extension)
    {
        var slug = Regex.Replace(baseName.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(slug))
        {
            slug = "document";
        }

        var candidate = $"{slug}{extension}";
        if (!System.IO.File.Exists(Path.Combine(UploadsPath, candidate)))
        {
            return candidate;
        }

        var counter = 1;
        do
        {
            candidate = $"{slug}-{counter++}{extension}";
        }
        while (System.IO.File.Exists(Path.Combine(UploadsPath, candidate)));

        return candidate;
    }

    /// <summary>
    /// Determines if the request is an AJAX request.
    /// </summary>
    /// <returns></returns>
    private bool IsAjaxRequest()
    {
        return Request.Headers.Accept.ToString().Contains("application/json");
    }

    /// <summary>
    /// Validates the uploaded content stream to ensure it's actually a document.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    private static async Task<bool> IsValidDocumentAsync(Stream stream, string extension)
    {
        if (extension == ".txt")
        {
            var buffer = new byte[512];
            var read = await stream.ReadAsync(buffer);
            if (read == 0) return false;
            // Reject if any null bytes — strong indicator of binary content
            return !buffer.Take(read).Any(b => b == 0);
        }

        // PDF: %PDF  |  DOCX/XLSX/PPTX: PK\x03\x04 (ZIP local file header)
        var signatures = new Dictionary<string, byte[]>
        {
            [".pdf"] = [0x25, 0x50, 0x44, 0x46],
            [".docx"] = [0x50, 0x4B, 0x03, 0x04],
            [".xlsx"] = [0x50, 0x4B, 0x03, 0x04],
            [".pptx"] = [0x50, 0x4B, 0x03, 0x04],
        };

        if (!signatures.TryGetValue(extension, out var sig)) return false;
        var magic = new byte[sig.Length];
        var bytesRead = await stream.ReadAsync(magic);
        return bytesRead == sig.Length && sig.SequenceEqual(magic);
    }
}