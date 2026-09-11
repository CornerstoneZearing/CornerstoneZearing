using CornerstoneZearing.Data.Enums;

namespace CornerstoneZearing.Data.Entities;

public class Post
{
    public int PostID { get; set; }
    public int? PostCategoryID { get; set; }
    public int? FeaturedMediaID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ContentJson { get; set; }
    public string? ContentHtml { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }
    public DateTime? DatePublished { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public PostCategory? PostCategory { get; set; }
    public Media? FeaturedMedia { get; set; }
}
