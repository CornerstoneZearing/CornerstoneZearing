namespace CornerstoneZearing.Data.Entities;

public class Document
{
    public int DocumentID { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SizeBytes { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}
