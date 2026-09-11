namespace CornerstoneZearing.Data.Entities;

public class Sidebar
{
    public int SidebarID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ContentJson { get; set; }
    public string? ContentHtml { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
