namespace CornerstoneZearing.Data.Entities;

/// <summary>Shared shape for post/sermon categories so the admin can render them with one view.</summary>
public interface ICategoryEntity
{
    int CategoryId { get; }
    string Name { get; }
    string Slug { get; }
    string? Description { get; }
}
