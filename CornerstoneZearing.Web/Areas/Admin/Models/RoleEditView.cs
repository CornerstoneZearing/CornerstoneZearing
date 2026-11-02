using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class RoleEditView
{
    public int Id { get; set; }

    [Required, StringLength(256)]
    public string Name { get; set; } = "";

    [StringLength(256)]
    public string? Description { get; set; }

    public List<string> Permissions { get; set; } = new();
}