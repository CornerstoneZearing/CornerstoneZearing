namespace CornerstoneZearing.Data.Interfaces;

public interface ICategoryEntity
{
    int CategoryId { get; }
    string Name { get; }
    string Slug { get; }
    string? Description { get; }
}