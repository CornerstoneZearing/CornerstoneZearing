using System.ComponentModel.DataAnnotations;
using CornerstoneZearing.Data;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class PageFormModel
{
    public Guid PageID { get; set; }

    [Required]
    [MaxLength(200)]
    [Display(Name = "Page Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Content")]
    public string ContentHtml { get; set; } = string.Empty;

    public string? ContentJson { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Template Name")]
    public string TemplateName { get; set; } = "Default";

    [Required]
    [MaxLength(200)]
    [Display(Name = "URL Slug")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*(?:/[a-z0-9]+(?:-[a-z0-9]+)*)*$", ErrorMessage = "Slug must be lowercase letters, numbers, and hyphens, with slashes separating sub-page segments (e.g. about-us/learn-more).")]
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