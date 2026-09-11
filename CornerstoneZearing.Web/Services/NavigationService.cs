using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CornerstoneZearing.Web.Services;

public record NavNode(int PageID, string Title, string Slug, IReadOnlyList<NavNode> Children);

public class NavigationService
{
    private const string CacheKey = "public-navigation";
    private readonly CornerstoneDbContext _db;
    private readonly IMemoryCache _cache;

    public NavigationService(CornerstoneDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public Task<IReadOnlyList<NavNode>> GetNavigationAsync() =>
        _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var pages = await _db.Pages
                .Where(p => p.ShowInNavigation && p.Status == ContentStatus.Published)
                .OrderBy(p => p.SortOrder).ThenBy(p => p.Title)
                .Select(p => new { p.PageID, p.ParentPageID, p.Title, p.Slug })
                .ToListAsync();

            List<NavNode> Build(int? parentId) =>
                pages.Where(p => p.ParentPageID == parentId)
                     .Select(p => new NavNode(p.PageID, p.Title, p.Slug, Build(p.PageID)))
                     .ToList();

            return (IReadOnlyList<NavNode>)Build(null);
        })!;

    public void Invalidate() => _cache.Remove(CacheKey);
}
