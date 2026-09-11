using CornerstoneZearing.Data.Enums;

namespace CornerstoneZearing.Data.Entities;

public class Page
{
    public int PageID { get; set; }
    public int? ParentPageID { get; set; }
    public int? SidebarID { get; set; }
    public int? FeaturedMediaID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Template { get; set; }
    public string? ContentJson { get; set; }
    public string? ContentHtml { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int SortOrder { get; set; }
    public bool ShowInNavigation { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public Page? ParentPage { get; set; }
    public ICollection<Page> ChildPages { get; set; } = new List<Page>();
    public Sidebar? Sidebar { get; set; }
    public Media? FeaturedMedia { get; set; }
}
