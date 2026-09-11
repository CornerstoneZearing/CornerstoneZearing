using System.ComponentModel.DataAnnotations;
using CornerstoneZearing.Data.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class PageEditViewModel
{
    public int PageID { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Slug { get; set; }

    public int? ParentPageID { get; set; }
    public int? SidebarID { get; set; }
    public int? FeaturedMediaID { get; set; }

    [StringLength(100)]
    public string? Template { get; set; }

    public string? ContentJson { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [StringLength(500)]
    public string? MetaDescription { get; set; }

    public int SortOrder { get; set; }
    public bool ShowInNavigation { get; set; }

    public SelectList? ParentOptions { get; set; }
    public SelectList? SidebarOptions { get; set; }
    public SelectList? TemplateOptions { get; set; }
    public string? FeaturedMediaUrl { get; set; }
}

public class PostEditViewModel
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

public class SidebarEditViewModel
{
    public int SidebarID { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? ContentJson { get; set; }
}

public class CategoryEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Slug { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class SermonEditViewModel
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

public class EventEditViewModel
{
    public int EventID { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required, DataType(DataType.DateTime)]
    public DateTime StartDateTime { get; set; } = DateTime.Today.AddHours(9);

    [Required, DataType(DataType.DateTime)]
    public DateTime EndDateTime { get; set; } = DateTime.Today.AddHours(10);

    [StringLength(100)]
    public string? Location { get; set; }

    public string? Description { get; set; }

    public bool Private { get; set; }

    [StringLength(500)]
    public string? RecurrenceRule { get; set; }

    [DataType(DataType.Date)]
    public DateTime? RecurrenceEndDate { get; set; }

    public string? RecurrenceExceptions { get; set; }
}
