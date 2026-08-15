using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace CornerstoneZearing.Web.Packager;

public sealed record PackageOutput(string content, string contentType, string hash);

public sealed class PackageProcessor
{
    private readonly IWebHostEnvironment _Environment;
    private readonly ConcurrentDictionary<string, PackageOutput> _Cache = new(StringComparer.OrdinalIgnoreCase);

    public PackageProcessor(IWebHostEnvironment environment, PackageCollection packages)
    {
        _Environment = environment;

        // Re-build a package's cached output whenever one of its source files changes on disk,
        // so edits show up on the next request without an app restart.
        foreach (var package in packages.All)
        {
            foreach (var path in package.FilePaths)
            {
                ChangeToken.OnChange(
                    () => _Environment.WebRootFileProvider.Watch(path),
                    () => Invalidate(package.VirtualPath));
            }
        }
    }

    /// <summary>
    /// Returns a cached package.
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    public PackageOutput GetOrBuild(Package package)
    {
        return _Cache.GetOrAdd(package.VirtualPath, _ => Build(package));
    }

    /// <summary>
    /// Invalidates a cached package.
    /// </summary>
    /// <param name="virtualPath"></param>
    public void Invalidate(string virtualPath)
    {
        _Cache.TryRemove(virtualPath, out _);
    }

    /// <summary>
    /// Invalidates all cached packages.
    /// </summary>
    public void InvalidateAll()
    {
        _Cache.Clear();
    }

    /// <summary>
    /// Builds the package by reading, concatenating, and minifying files.
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    private PackageOutput Build(Package package)
    {
        var sb = new StringBuilder();
        foreach (var path in package.FilePaths)
        {
            var file = _Environment.WebRootFileProvider.GetFileInfo(path);
            if (!file.Exists || file.IsDirectory)
            {
                throw new FileNotFoundException($"File '{path}' not found for package '{package.VirtualPath}'.");
            }
            using var reader = new StreamReader(file.CreateReadStream());
            sb.AppendLine(reader.ReadToEnd());
        }

        var raw = sb.ToString();

        string content;
        string contentType;

        if (package.Type == PackageType.Style)
        {
            contentType = "text/css; charset=utf-8";
            content = PackageMinifier.MinifyCss(raw);
        }
        else
        {
            contentType = "application/javascript; charset=utf-8";
            content = PackageMinifier.MinifyJs(raw);
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..8].ToLowerInvariant();
        return new PackageOutput(content, contentType, hash);
    }
}