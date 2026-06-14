namespace CornerstoneZearing.Packager;

public sealed class PackageMiddleware(RequestDelegate next, PackageCollection packages, PackageProcessor processor)
{
    /// <summary>
    /// Handles incoming HTTP requests to serve packaged content.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimEnd('/');
        if (path is not null)
        {
            var package = packages.Get(path);
            if (package is not null)
            {
                var output = processor.GetOrBuild(package);
                var etag = $"\"{output.hash}\"";

                if (context.Request.Headers.IfNoneMatch == etag)
                {
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    return;
                }

                context.Response.ContentType = output.contentType;
                context.Response.Headers.ETag = etag;
                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                await context.Response.WriteAsync(output.content);
                return;
            }
        }

        await next(context);
    }
}