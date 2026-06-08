namespace CornerstoneZearing.Website.Packager
{
    /// <summary>
    /// Initilization constructor.
    /// </summary>
    /// <param name="virtualPath"></param>
    /// <param name="type"></param>
    public abstract class Package(string virtualPath, PackageType type)
    {
        private readonly List<string> _Paths = [];

        public string VirtualPath { get; } = virtualPath.TrimStart('~').TrimEnd('/');

        public PackageType Type { get; } = type;

        public IReadOnlyList<string> FilePaths
        {
            get
            {
                return _Paths;
            }
        }

        /// <summary>
        /// Includes specified virtual paths to the package.
        /// </summary>
        /// <param name="virtualPaths"></param>
        /// <returns></returns>
        public Package Include(params string[] virtualPaths)
        {
            foreach (var path in virtualPaths)
            {
                _Paths.Add(path.TrimStart('~'));
            }
            return this;
        }
    }

    public sealed class StylePackage(string virtualPath) : Package(virtualPath, PackageType.Style);

    public sealed class ScriptPackage(string virtualPath) : Package(virtualPath, PackageType.Script);

    public enum PackageType
    {
        Style,
        Script
    }
}