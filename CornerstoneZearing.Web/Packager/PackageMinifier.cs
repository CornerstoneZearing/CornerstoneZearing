using NUglify;

namespace CornerstoneZearing.Web.Packager;

internal static partial class PackageMinifier
{
    /// <summary>
    /// Minifies the specified CSS content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string MinifyCss(string content)
    {
        return Uglify.Css(content).Code;
    }

    /// <summary>
    /// Minifies the specified JavaScript content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string MinifyJs(string content)
    {
        return Uglify.Js(content).Code;
    }
}