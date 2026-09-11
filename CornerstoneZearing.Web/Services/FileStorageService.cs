using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace CornerstoneZearing.Web.Services;

public class UploadOptions
{
    public string RootPath { get; set; } = "App_Data/uploads";
    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxDocumentBytes { get; set; } = 50 * 1024 * 1024;
    public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
    public string[] AllowedDocumentExtensions { get; set; } =
        { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
}

public record StoredFile(string StoredFileName, string OriginalFileName, string ContentType, long SizeBytes, int Width, int Height);

public class FileStorageService
{
    private readonly UploadOptions _options;
    private readonly string _root;

    public FileStorageService(IOptions<UploadOptions> options, IWebHostEnvironment env)
    {
        _options = options.Value;
        _root = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(env.ContentRootPath, _options.RootPath);
    }

    public bool IsAllowedImage(string fileName) =>
        _options.AllowedImageExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public bool IsAllowedDocument(string fileName) =>
        _options.AllowedDocumentExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public long MaxImageBytes => _options.MaxImageBytes;
    public long MaxDocumentBytes => _options.MaxDocumentBytes;

    public async Task<StoredFile> SaveAsync(IFormFile file, string category, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var stored = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{ext}";
        var absolute = Path.Combine(_root, category, stored.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

        await using (var stream = new FileStream(absolute, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream, ct);
        }

        int width = 0, height = 0;
        if (category == "media" && ext != ".svg")
        {
            try
            {
                var info = await Image.IdentifyAsync(absolute, ct);
                width = info.Width;
                height = info.Height;
            }
            catch
            {
                // Non-fatal: dimensions stay 0 for formats ImageSharp cannot read.
            }
        }

        return new StoredFile(
            $"{category}/{stored}",
            Path.GetFileName(file.FileName),
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            file.Length,
            width,
            height);
    }

    public string GetAbsolutePath(string storedFileName) =>
        Path.Combine(_root, storedFileName.Replace('/', Path.DirectorySeparatorChar));

    public bool Exists(string storedFileName) => File.Exists(GetAbsolutePath(storedFileName));

    public void Delete(string storedFileName)
    {
        var path = GetAbsolutePath(storedFileName);
        if (File.Exists(path))
            File.Delete(path);
    }
}
