namespace CornerstoneZearing.Data.Entities;

public class ContentBlock
{
    public int ContentBlockID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ContentJson { get; set; }
    public string? ContentHtml { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}