using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class SlideshowController : Controller
{
    private const long _MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly string[] _AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    private readonly ApplicationDbContext _DbContext;
    private readonly IWebHostEnvironment _Environment;

    private string UploadsPath => Path.Combine(_Environment.WebRootPath, "uploads", "slideshow");

    public SlideshowController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _DbContext = context;
        _Environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var slides = await _DbContext.SlideshowSlides
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
        return View(slides);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new SlideshowSlideFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Create(SlideshowSlideFormModel model)
    {
        if (model.ImageFile == null || model.ImageFile.Length == 0)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "An image is required.");
        }
        else
        {
            var imageError = ValidateImage(model.ImageFile);
            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var storedFileName = await SaveImageAsync(model.ImageFile!);

        var slide = new SlideshowSlide
        {
            SlideshowSlideID = Guid.NewGuid(),
            Name = model.Name.Trim(),
            StoredFileName = storedFileName,
            AltText = model.AltText.Trim(),
            Link = model.Link.Trim(),
            IsActive = model.IsActive,
            DisplayOrder = model.DisplayOrder,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        _DbContext.SlideshowSlides.Add(slide);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Slide \"{slide.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var slide = await _DbContext.SlideshowSlides.FindAsync(id);
        if (slide == null) return NotFound();

        var model = new SlideshowSlideFormModel
        {
            SlideshowSlideID = slide.SlideshowSlideID,
            Name = slide.Name,
            StoredFileName = slide.StoredFileName,
            AltText = slide.AltText,
            Link = slide.Link,
            IsActive = slide.IsActive,
            DisplayOrder = slide.DisplayOrder
        };

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Edit(SlideshowSlideFormModel model)
    {
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var imageError = ValidateImage(model.ImageFile);
            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var slide = await _DbContext.SlideshowSlides.FindAsync(model.SlideshowSlideID);
        if (slide == null) return NotFound();

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var oldPath = Path.Combine(UploadsPath, slide.StoredFileName);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
            slide.StoredFileName = await SaveImageAsync(model.ImageFile);
        }

        slide.Name = model.Name.Trim();
        slide.AltText = model.AltText.Trim();
        slide.Link = model.Link.Trim();
        slide.IsActive = model.IsActive;
        slide.DisplayOrder = model.DisplayOrder;
        slide.DateModified = DateTime.UtcNow;

        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Slide \"{slide.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var slide = await _DbContext.SlideshowSlides.FindAsync(id);
        if (slide == null) return NotFound();
        return View(slide);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var slide = await _DbContext.SlideshowSlides.FindAsync(id);
        if (slide == null) return NotFound();

        var filePath = Path.Combine(UploadsPath, slide.StoredFileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _DbContext.SlideshowSlides.Remove(slide);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Slide \"{slide.Name}\" deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private string? ValidateImage(IFormFile file)
    {
        if (file.Length > _MaxFileSizeBytes)
        {
            return "File exceeds the 10 MB maximum size.";
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_AllowedExtensions.Contains(extension))
        {
            return "Only JPG, PNG, and GIF files are allowed.";
        }

        return null;
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        Directory.CreateDirectory(UploadsPath);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var baseName = Path.GetFileNameWithoutExtension(file.FileName);
        var slug = Regex.Replace(baseName.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        if (string.IsNullOrEmpty(slug)) slug = "slide";

        var candidate = $"{slug}{extension}";
        if (System.IO.File.Exists(Path.Combine(UploadsPath, candidate)))
        {
            var counter = 1;
            do
            {
                candidate = $"{slug}-{counter++}{extension}";
            }
            while (System.IO.File.Exists(Path.Combine(UploadsPath, candidate)));
        }

        var filePath = Path.Combine(UploadsPath, candidate);
        await using var output = System.IO.File.Create(filePath);
        await file.CopyToAsync(output);

        return candidate;
    }
}
