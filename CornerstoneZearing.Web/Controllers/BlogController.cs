using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Enums;
using CornerstoneZearing.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Controllers;

[Route("blog")]
public class BlogController : Controller
{
    private const int PageSize = 10;
    private readonly CornerstoneDbContext _db;

    public BlogController(CornerstoneDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, string? category = null, string? tag = null)
    {
        page = Math.Max(1, page);
        var query = _db.Posts.Include(p => p.PostCategory)
            .Where(p => p.Status == ContentStatus.Published);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.PostCategory != null && p.PostCategory.Slug == category);
        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(p => p.Tags != null && (p.Tags + ",").Contains(tag + ","));

        var total = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.DatePublished ?? p.DateCreated)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .ToListAsync();

        return View(new BlogIndexViewModel
        {
            Posts = posts,
            Page = page,
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)),
            Category = category,
            Tag = tag,
            Categories = await _db.PostCategories.OrderBy(c => c.Name).ToListAsync(),
        });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Post(string slug)
    {
        var post = await _db.Posts.Include(p => p.PostCategory).Include(p => p.FeaturedMedia)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published);
        if (post is null) return NotFound();
        return View(post);
    }
}
