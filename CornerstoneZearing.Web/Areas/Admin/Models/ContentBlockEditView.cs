using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class ContentBlockEditView
{
    public int ContentBlockID { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? ContentJson { get; set; }
}