using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class CategoryEditView
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Slug { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}