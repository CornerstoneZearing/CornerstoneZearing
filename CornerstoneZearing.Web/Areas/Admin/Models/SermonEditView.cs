using System.ComponentModel.DataAnnotations;
using CornerstoneZearing.Data.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class SermonEditView
{
    public int SermonID { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int SermonCategoryID { get; set; }

    public int? FeaturedDocumentID { get; set; }
    public int? FeaturedMediaID { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime SermonDate { get; set; } = DateTime.Today;

    [StringLength(100)]
    public string? Speaker { get; set; }

    public string? ContentJson { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public SelectList? CategoryOptions { get; set; }
    public SelectList? DocumentOptions { get; set; }
    public string? FeaturedMediaUrl { get; set; }
}