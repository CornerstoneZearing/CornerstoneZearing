namespace CornerstoneZearing.Data.Entities;

public class SermonCategory : ICategoryEntity
{
    public int SermonCategoryID { get; set; }
    public int CategoryId => SermonCategoryID;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }

    public ICollection<Sermon> Sermons { get; set; } = new List<Sermon>();
}
