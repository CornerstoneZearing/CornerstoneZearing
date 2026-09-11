using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Enums;
using CornerstoneZearing.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Controllers;

[Route("sermons")]
public class SermonsController : Controller
{
    private readonly CornerstoneDbContext _db;

    public SermonsController(CornerstoneDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? category = null)
    {
        var query = _db.Sermons.Include(s => s.SermonCategory)
            .Where(s => s.Status == ContentStatus.Published);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(s => s.SermonCategory!.Slug == category);

        return View(new SermonsIndexViewModel
        {
            Sermons = await query.OrderByDescending(s => s.SermonDate).ToListAsync(),
            Categories = await _db.SermonCategories.OrderBy(c => c.Name).ToListAsync(),
            Category = category,
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var sermon = await _db.Sermons
            .Include(s => s.SermonCategory)
            .Include(s => s.FeaturedDocument)
            .Include(s => s.FeaturedMedia)
            .FirstOrDefaultAsync(s => s.SermonID == id && s.Status == ContentStatus.Published);
        if (sermon is null) return NotFound();
        return View(sermon);
    }
}
