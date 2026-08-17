using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Data.Entities;

public class Sidebar
{
    public Guid SidebarID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string ContentHtml { get; set; } = string.Empty;

    public string ContentJson { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; }

    public DateTime DateModified { get; set; }
}