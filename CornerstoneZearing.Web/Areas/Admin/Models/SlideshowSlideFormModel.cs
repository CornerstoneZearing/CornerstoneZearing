using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class SlideshowSlideFormModel
{
    public Guid SlideshowSlideID { get; set; }

    [Required]
    [MaxLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    public string? StoredFileName { get; set; }

    [MaxLength(500)]
    [Display(Name = "Alt Text")]
    public string AltText { get; set; } = string.Empty;

    [MaxLength(2000)]
    [Display(Name = "Link URL")]
    public string Link { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Range(0, 9999)]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }
}