using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class UserEditView
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    public List<RoleCheckbox> Roles { get; set; } = new();
}