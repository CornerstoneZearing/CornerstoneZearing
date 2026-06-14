using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Data;

public class Page
{
    public Guid PageID { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string TemplateName { get; set; } = "Default";

    [Required, MaxLength(200)]
    public string UrlSlug { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public PageStatus Status { get; set; } = PageStatus.Draft;
}
