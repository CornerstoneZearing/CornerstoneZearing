namespace CornerstoneZearing.Data.Entities;

public class Media
{
    public int MediaID { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SizeBytes { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}
