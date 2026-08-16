namespace CornerstoneZearing.Data.Entities;

public class SlideshowSlide
{
    public Guid SlideshowSlideID { get; set; }

    public string Name { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateModified { get; set; }
}