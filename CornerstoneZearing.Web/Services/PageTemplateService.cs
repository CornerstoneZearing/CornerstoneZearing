namespace CornerstoneZearing.Web.Services;

public class PageTemplateService
{
    private readonly string _templatesPath;

    public PageTemplateService(IWebHostEnvironment env)
    {
        _templatesPath = Path.Combine(env.ContentRootPath, "Views", "Templates");
    }

    public const string Default = "Default";

    public IReadOnlyList<string> GetTemplateNames()
    {
        if (!Directory.Exists(_templatesPath))
            return new[] { Default };

        var names = Directory.EnumerateFiles(_templatesPath, "*.cshtml")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => !string.IsNullOrEmpty(n) && !n!.StartsWith('_'))
            .Select(n => n!)
            .OrderBy(n => n)
            .ToList();

        if (names.Count == 0)
            names.Add(Default);

        return names;
    }

    public string ResolveViewName(string? template)
    {
        var available = GetTemplateNames();
        if (!string.IsNullOrWhiteSpace(template) && available.Contains(template, StringComparer.OrdinalIgnoreCase))
            return $"~/Views/Templates/{template}.cshtml";

        return $"~/Views/Templates/{Default}.cshtml";
    }
}
