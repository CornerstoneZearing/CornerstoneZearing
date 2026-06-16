namespace CornerstoneZearing.Data;

public class MediaDocument
{
    public Guid MediaDocumentID { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DateUploaded { get; set; }
    public Guid UploadedByUserID { get; set; }
}
