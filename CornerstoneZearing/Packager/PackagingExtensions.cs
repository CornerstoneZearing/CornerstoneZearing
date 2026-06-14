namespace CornerstoneZearing.Packager;

public static class PackagingExtensions
{
    /// <summary>
    /// Adds the package collection and processor to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddPackages(this IServiceCollection services, Action<PackageCollection> configure)
    {
        var collection = new PackageCollection();
        configure(collection);
        services.AddSingleton(collection);
        services.AddSingleton<PackageProcessor>();
        return services;
    }

    /// <summary>
    /// Adds the package middleware to the application request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UsePackages(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PackageMiddleware>();
    }
}