using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class SidebarFormModel
{
    public Guid SidebarID { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Content")]
    public string ContentHtml { get; set; } = string.Empty;

    public string ContentJson { get; set; } = string.Empty;
}