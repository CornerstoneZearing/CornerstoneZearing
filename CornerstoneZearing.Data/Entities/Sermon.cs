using CornerstoneZearing.Data.Enums;

namespace CornerstoneZearing.Data.Entities;

public class Sermon
{
    public int SermonID { get; set; }
    public int SermonCategoryID { get; set; }
    public int? FeaturedDocumentID { get; set; }
    public int? FeaturedMediaID { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime SermonDate { get; set; }
    public string? ContentJson { get; set; }
    public string? ContentHtml { get; set; }
    public string? Speaker { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public SermonCategory? SermonCategory { get; set; }
    public Document? FeaturedDocument { get; set; }
    public Media? FeaturedMedia { get; set; }
}
