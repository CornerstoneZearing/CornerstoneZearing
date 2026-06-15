using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Areas.Admin.Models;

public class PageFormModel
{
    public Guid PageID { get; set; }

    [Required]
    [MaxLength(200)]
    [Display(Name = "Page Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Template Name")]
    public string TemplateName { get; set; } = "Default";

    [Required]
    [MaxLength(200)]
    [Display(Name = "URL Slug")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must be lowercase letters, numbers, and hyphens only.")]
    public string UrlSlug { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "Meta Title")]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    [Display(Name = "Meta Description")]
    public string? MetaDescription { get; set; }

    [Display(Name = "Status")]
    public PageStatus Status { get; set; } = PageStatus.Draft;
}