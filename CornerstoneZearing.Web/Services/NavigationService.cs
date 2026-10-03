using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CornerstoneZearing.Web.Services;

public record NavNode(int PageID, string Title, string Path, IReadOnlyList<NavNode> Children);

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

            // Paths need every page (including hidden ones) since a parent may not be in the navigation.
            var all = await _db.Pages
                .Select(p => new { p.PageID, p.ParentPageID, p.Slug })
                .ToListAsync();
            var paths = PagePaths.Build(all.Select(p => (p.PageID, p.ParentPageID, p.Slug)));

            var pages = await _db.Pages
                .Where(p => p.ShowInNavigation && p.Status == ContentStatus.Published)
                .OrderBy(p => p.SortOrder).ThenBy(p => p.Title)
                .Select(p => new { p.PageID, p.ParentPageID, p.Title })
                .ToListAsync();

            List<NavNode> Build(int? parentId) =>
                pages.Where(p => p.ParentPageID == parentId)
                     .Select(p => new NavNode(p.PageID, p.Title, paths[p.PageID], Build(p.PageID)))
                     .ToList();

            return (IReadOnlyList<NavNode>)Build(null);
        })!;

    public void Invalidate() => _cache.Remove(CacheKey);
}
