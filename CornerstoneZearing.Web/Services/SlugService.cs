using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CornerstoneZearing.Web.Services;

public partial class SlugService
{
    public string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = NonSlugChars().Replace(slug, "-");
        slug = MultiDash().Replace(slug, "-").Trim('-');
        return slug;
    }

    /// <summary>Slugify then append -2, -3, ... until <paramref name="isUnique"/> is satisfied.</summary>
    public async Task<string> UniqueSlugAsync(string input, Func<string, Task<bool>> isUnique, string? current = null)
    {
        var baseSlug = Slugify(input);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "item";

        var candidate = baseSlug;
        var suffix = 2;
        while (!string.Equals(candidate, current, StringComparison.OrdinalIgnoreCase) && !await isUnique(candidate))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }

        return candidate;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugChars();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiDash();
}
