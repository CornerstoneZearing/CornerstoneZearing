using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class MediaController : Controller
{
    private const long _MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly string[] _AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".svg"];
    private readonly ApplicationDbContext _DbContext;
    private readonly IWebHostEnvironment _Environment;

    private string UploadsPath
    {
        get
        {
            return Path.Combine(_Environment.WebRootPath, "uploads");
        }
    }

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="environment"></param>
    public MediaController(ApplicationDbContext context, IWebHostEnvironment environment)
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
        var query = _DbContext.MediaImages.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m => m.OriginalFileName.Contains(term) || m.AltText.Contains(term));
        }

        var images = await query.OrderByDescending(m => m.DateUploaded).ToListAsync();
        ViewBag.Search = search?.Trim() ?? string.Empty;
        return View(images);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return UploadError("No file was provided.");
        }

        if (file.Length > _MaxFileSizeBytes)
        {
            return UploadError("File exceeds the 10 MB maximum size.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_AllowedExtensions.Contains(extension))
        {
            return UploadError("Only JPG, PNG, GIF, and SVG files are allowed.");
        }

        await using var stream = file.OpenReadStream();
        if (!await IsValidImageAsync(stream, extension))
        {
            return UploadError("The file content does not match a valid image.");
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
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };

        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : Guid.Empty;

        var image = new MediaImage
        {
            MediaImageID = Guid.NewGuid(),
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSize = file.Length,
            AltText = string.Empty,
            DateUploaded = DateTime.UtcNow,
            UploadedByUserID = userId
        };

        _DbContext.MediaImages.Add(image);
        await _DbContext.SaveChangesAsync();

        if (IsAjaxRequest())
        {
            return Json(new { success = true, id = image.MediaImageID, url = $"/uploads/{image.StoredFileName}", name = image.OriginalFileName, size = image.FileSize, type = image.ContentType });
        }

        TempData["Success"] = $"\"{image.OriginalFileName}\" uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Updates the alt text for an image.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="altText"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAlt(Guid id, string? altText)
    {
        var image = await _DbContext.MediaImages.FindAsync(id);
        if (image == null) return NotFound();
        image.AltText = (altText ?? string.Empty).Trim();
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
        var image = await _DbContext.MediaImages.FindAsync(id);
        if (image == null) return NotFound();
        return View(image);
    }

    /// <summary>
    /// Deletes an image.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var image = await _DbContext.MediaImages.FindAsync(id);
        if (image == null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(UploadsPath, image.StoredFileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _DbContext.MediaImages.Remove(image);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"\"{image.OriginalFileName}\" deleted successfully.";
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
    /// Generates a unique file name for uploaded image.
    /// </summary>
    /// <param name="baseName"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    private string UniqueFileName(string baseName, string extension)
    {
        var slug = Regex.Replace(baseName.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(slug))
        {
            slug = "image";
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
    /// Validates the uploaded content stream to ensure it's actually an image.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    private static async Task<bool> IsValidImageAsync(Stream stream, string extension)
    {
        if (extension == ".svg")
        {
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var header = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimStart();
            return header.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || (header.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && header.Contains("<svg", StringComparison.OrdinalIgnoreCase));
        }

        var signatures = new Dictionary<string, byte[]>
        {
            [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
            [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
            [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
            [".gif"] = new byte[] { 0x47, 0x49, 0x46, 0x38 },
        };

        if (!signatures.TryGetValue(extension, out var sig))
        {
            return false;
        }

        var magic = new byte[sig.Length];
        var read = await stream.ReadAsync(magic);
        return read == sig.Length && sig.SequenceEqual(magic);
    }
}