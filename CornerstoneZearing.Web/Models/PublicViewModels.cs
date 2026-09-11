using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Services;

namespace CornerstoneZearing.Web.Models;

public class HomeViewModel
{
    public List<Post> RecentPosts { get; set; } = new();
    public List<EventOccurrence> UpcomingEvents { get; set; } = new();
    public Sermon? LatestSermon { get; set; }
}

public class PageViewModel
{
    public Page Page { get; set; } = null!;
    public Sidebar? Sidebar { get; set; }
    public Media? FeaturedMedia { get; set; }
}

public class BlogIndexViewModel
{
    public IReadOnlyList<Post> Posts { get; set; } = Array.Empty<Post>();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public IReadOnlyList<PostCategory> Categories { get; set; } = Array.Empty<PostCategory>();
}

public class SermonsIndexViewModel
{
    public IReadOnlyList<Sermon> Sermons { get; set; } = Array.Empty<Sermon>();
    public IReadOnlyList<SermonCategory> Categories { get; set; } = Array.Empty<SermonCategory>();
    public string? Category { get; set; }
}
