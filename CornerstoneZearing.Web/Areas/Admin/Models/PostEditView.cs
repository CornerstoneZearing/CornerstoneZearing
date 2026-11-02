using System.ComponentModel.DataAnnotations;
using CornerstoneZearing.Data.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class PostEditView
{
    public int PostID { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Slug { get; set; }

    public int? PostCategoryID { get; set; }
    public int? FeaturedMediaID { get; set; }

    [StringLength(500)]
    public string? Summary { get; set; }

    public string? ContentJson { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [StringLength(500)]
    public string? MetaDescription { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? DatePublished { get; set; }

    public SelectList? CategoryOptions { get; set; }
    public string? FeaturedMediaUrl { get; set; }
}