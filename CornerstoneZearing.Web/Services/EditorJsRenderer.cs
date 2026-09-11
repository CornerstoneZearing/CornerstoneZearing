using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ganss.Xss;

namespace CornerstoneZearing.Web.Services;

/// <summary>
/// Converts Editor.js block JSON into sanitized HTML, server-side, on save.
/// Supports the block types used by the admin editor configuration.
/// </summary>
public class EditorJsRenderer
{
    private readonly HtmlSanitizer _sanitizer;

    public EditorJsRenderer()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p", "br", "b", "strong", "i", "em", "u", "s", "mark", "code", "a",
            "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "blockquote",
            "figure", "figcaption", "img", "pre", "hr", "table", "thead", "tbody",
            "tr", "th", "td", "div", "span", "iframe", "cite"
        })
        {
            _sanitizer.AllowedTags.Add(tag);
        }
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("src");
        _sanitizer.AllowedAttributes.Add("alt");
        _sanitizer.AllowedAttributes.Add("title");
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("target");
        _sanitizer.AllowedAttributes.Add("rel");
        _sanitizer.AllowedAttributes.Add("colspan");
        _sanitizer.AllowedAttributes.Add("rowspan");
        _sanitizer.AllowedAttributes.Add("frameborder");
        _sanitizer.AllowedAttributes.Add("allowfullscreen");
        _sanitizer.AllowedAttributes.Add("width");
        _sanitizer.AllowedAttributes.Add("height");
        _sanitizer.AllowedSchemes.Add("mailto");
    }

    public string Render(string? contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            return string.Empty;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(contentJson);
        }
        catch (JsonException)
        {
            return string.Empty;
        }

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("blocks", out var blocks) ||
                blocks.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var block in blocks.EnumerateArray())
            {
                var type = block.TryGetProperty("type", out var t) ? t.GetString() : null;
                var data = block.TryGetProperty("data", out var d) ? d : default;
                sb.Append(RenderBlock(type, data));
            }

            return _sanitizer.Sanitize(sb.ToString());
        }
    }

    private static string RenderBlock(string? type, JsonElement data)
    {
        switch (type)
        {
            case "header":
            {
                var level = data.TryGetProperty("level", out var l) && l.TryGetInt32(out var lv) ? Math.Clamp(lv, 1, 6) : 2;
                return $"<h{level}>{Inline(data, "text")}</h{level}>";
            }
            case "paragraph":
                return $"<p>{Inline(data, "text")}</p>";
            case "quote":
            {
                var caption = GetString(data, "caption");
                var cite = string.IsNullOrWhiteSpace(caption) ? "" : $"<figcaption>{Encode(caption)}</figcaption>";
                return $"<figure><blockquote>{Inline(data, "text")}</blockquote>{cite}</figure>";
            }
            case "list":
            case "checklist":
                return RenderList(data);
            case "delimiter":
                return "<hr />";
            case "code":
                return $"<pre><code>{Encode(GetString(data, "code"))}</code></pre>";
            case "raw":
                return GetString(data, "html");
            case "table":
                return RenderTable(data);
            case "image":
            {
                var url = GetString(data, "url");
                if (url.Length == 0 && data.TryGetProperty("file", out var file) && file.ValueKind == JsonValueKind.Object)
                    url = file.TryGetProperty("url", out var fu) ? fu.GetString() ?? "" : "";
                if (url.Length == 0) return string.Empty;
                var caption = GetString(data, "caption");
                var fig = string.IsNullOrWhiteSpace(caption) ? "" : $"<figcaption>{Encode(caption)}</figcaption>";
                return $"<figure class=\"editorjs-image\"><img src=\"{Encode(url)}\" alt=\"{Encode(caption)}\" />{fig}</figure>";
            }
            case "embed":
            {
                var embed = GetString(data, "embed");
                if (embed.Length == 0) return string.Empty;
                return $"<figure class=\"editorjs-embed\"><iframe src=\"{Encode(embed)}\" frameborder=\"0\" allowfullscreen></iframe></figure>";
            }
            case "warning":
                return $"<div class=\"editorjs-warning\"><strong>{Encode(GetString(data, "title"))}</strong><p>{Inline(data, "message")}</p></div>";
            case "bootstrapCard":
                return RenderBootstrapCard(data);
            default:
                return string.Empty;
        }
    }

    private static string RenderBootstrapCard(JsonElement data)
    {
        var imageUrl = GetString(data, "imageUrl");
        var imageAlt = GetString(data, "imageAlt");
        var title = GetString(data, "title");
        var text = GetString(data, "text");
        var linkUrl = GetString(data, "linkUrl");
        var linkText = GetString(data, "linkText");

        var sb = new StringBuilder("<div class=\"card\">");
        if (imageUrl.Length > 0)
            sb.Append($"<img src=\"{Encode(imageUrl)}\" alt=\"{Encode(imageAlt)}\" class=\"card-img-top\" />");

        sb.Append("<div class=\"card-body\">");
        if (title.Length > 0)
            sb.Append($"<h5 class=\"card-title\">{Encode(title)}</h5>");
        if (text.Length > 0)
            sb.Append($"<p class=\"card-text\">{Encode(text)}</p>");
        if (linkUrl.Length > 0 && linkText.Length > 0)
            sb.Append($"<a href=\"{Encode(linkUrl)}\" class=\"btn btn-primary\">{Encode(linkText)}</a>");
        sb.Append("</div></div>");
        return sb.ToString();
    }

    private static string RenderList(JsonElement data)
    {
        var style = GetString(data, "style");
        var tag = style.Equals("ordered", StringComparison.OrdinalIgnoreCase) ? "ol" : "ul";
        if (!data.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var sb = new StringBuilder($"<{tag}>");
        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                sb.Append($"<li>{SanitizeInline(item.GetString())}</li>");
            }
            else if (item.ValueKind == JsonValueKind.Object)
            {
                var content = item.TryGetProperty("content", out var c) ? c.GetString() : item.TryGetProperty("text", out var tx) ? tx.GetString() : null;
                sb.Append($"<li>{SanitizeInline(content)}");
                if (item.TryGetProperty("items", out var nested) && nested.ValueKind == JsonValueKind.Array && nested.GetArrayLength() > 0)
                {
                    var wrapper = JsonDocument.Parse($"{{\"style\":\"{style}\",\"items\":{nested.GetRawText()}}}").RootElement;
                    sb.Append(RenderList(wrapper));
                }
                sb.Append("</li>");
            }
        }
        sb.Append($"</{tag}>");
        return sb.ToString();
    }

    private static string RenderTable(JsonElement data)
    {
        if (!data.TryGetProperty("content", out var rows) || rows.ValueKind != JsonValueKind.Array)
            return string.Empty;
        var withHeadings = data.TryGetProperty("withHeadings", out var wh) && wh.ValueKind == JsonValueKind.True;

        var sb = new StringBuilder("<table>");
        var first = true;
        foreach (var row in rows.EnumerateArray())
        {
            var cellTag = withHeadings && first ? "th" : "td";
            sb.Append("<tr>");
            foreach (var cell in row.EnumerateArray())
                sb.Append($"<{cellTag}>{SanitizeInline(cell.GetString())}</{cellTag}>");
            sb.Append("</tr>");
            first = false;
        }
        sb.Append("</table>");
        return sb.ToString();
    }

    private static string Inline(JsonElement data, string prop) => SanitizeInline(GetString(data, prop));

    private static string SanitizeInline(string? html) => html ?? string.Empty;

    private static string GetString(JsonElement data, string prop) =>
        data.ValueKind == JsonValueKind.Object && data.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? string.Empty
            : string.Empty;

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}
