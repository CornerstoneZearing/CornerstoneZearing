namespace CornerstoneZearing.Website.Packager
{
    public sealed class PackageCollection
    {
        private readonly Dictionary<string, Package> _Packages = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<Package> All
        {
            get
            {
                return _Packages.Values;
            }
        }

        /// <summary>
        /// Adds a package to the collection.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public PackageCollection Add(Package package)
        {
            _Packages[package.VirtualPath] = package;
            return this;
        }

        /// <summary>
        /// Gets a package from the collection by its virtual path.
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public Package? Get(string virtualPath)
        {
            return _Packages.TryGetValue(virtualPath.TrimStart('~').TrimEnd('/'), out var pkg) ? pkg : null;
        }
    }
}