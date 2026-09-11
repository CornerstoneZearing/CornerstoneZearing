using System.ComponentModel.DataAnnotations;

namespace CornerstoneZearing.Web.Areas.Admin.Models;

public class UserListItem
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public class UserEditViewModel
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

public class RoleCheckbox
{
    public int RoleId { get; set; }
    public string Name { get; set; } = "";
    public bool Assigned { get; set; }
}

public class RoleEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(256)]
    public string Name { get; set; } = "";

    [StringLength(256)]
    public string? Description { get; set; }

    /// <summary>Permission strings that are checked.</summary>
    public List<string> Permissions { get; set; } = new();
}
