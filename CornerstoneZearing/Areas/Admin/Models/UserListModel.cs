namespace CornerstoneZearing.Areas.Admin.Models;

public class UserListModel
{
    public Guid UserID { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string DisplayName
    {
        get
        {
            return string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName) ? Email ?? "Unknown" : $"{FirstName} {LastName}".Trim();
        }
    }

    public IList<string> Roles { get; set; } = [];
}