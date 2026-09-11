namespace CornerstoneZearing.Data.Entities;

public class PostCategory : ICategoryEntity
{
    public int PostCategoryID { get; set; }
    public int CategoryId => PostCategoryID;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
