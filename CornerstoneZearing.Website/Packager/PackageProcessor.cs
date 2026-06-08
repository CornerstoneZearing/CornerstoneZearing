using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace CornerstoneZearing.Website.Packager
{
    public sealed record PackageOutput(string content, string contentType, string hash);

    public sealed class PackageProcessor(IWebHostEnvironment environment)
    {
        private readonly ConcurrentDictionary<string, PackageOutput> _Cache = new(StringComparer.OrdinalIgnoreCase);

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
                var file = environment.WebRootFileProvider.GetFileInfo(path);
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
}