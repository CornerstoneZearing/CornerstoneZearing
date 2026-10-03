namespace CornerstoneZearing.Web.Services;

public static class PagePaths
{
    /// <summary>Builds each page's full hierarchical path (e.g. <c>about-us/history</c>), keyed by PageID.</summary>
    public static Dictionary<int, string> Build(IEnumerable<(int PageID, int? ParentPageID, string Slug)> pages)
    {
        var byId = pages.ToDictionary(p => p.PageID);
        var paths = new Dictionary<int, string>();

        foreach (var page in byId.Values)
        {
            var segments = new List<string>();
            var visited = new HashSet<int>();
            var current = (int?)page.PageID;
            while (current is { } id && byId.TryGetValue(id, out var node) && visited.Add(id))
            {
                segments.Add(node.Slug);
                current = node.ParentPageID;
            }

            segments.Reverse();
            paths[page.PageID] = string.Join('/', segments);
        }

        return paths;
    }
}
