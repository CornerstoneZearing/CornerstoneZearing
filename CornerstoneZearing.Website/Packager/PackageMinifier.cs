using System.Text.RegularExpressions;

namespace CornerstoneZearing.Website.Packager
{
    internal static partial class PackageMinifier
    {
        /// <summary>
        /// Minifies the specified CSS content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string MinifyCss(string content)
        {
            content = CssComment().Replace(content, string.Empty);
            content = WhitespaceRun().Replace(content, " ");
            content = CssStructural().Replace(content, "$1");
            content = content.Replace(";}", "}");
            return content.Trim();
        }

        /// <summary>
        /// Minifies the specified JavaScript content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string MinifyJs(string content)
        {
            content = JsBlockComment().Replace(content, string.Empty);
            content = ExcessiveBlankLines().Replace(content, "\n\n");
            return content.Trim();
        }

        // CSS: /* ... */
        [GeneratedRegex(@"/\*[\s\S]*?\*/")]
        private static partial Regex CssComment();

        // Any run of whitespace characters (collapse to single space)
        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRun();

        // Optional whitespace around CSS structural characters
        [GeneratedRegex(@"\s*([{};,>~+])\s*")]
        private static partial Regex CssStructural();

        // JS block comments that are NOT license comments (/*! ... */)
        [GeneratedRegex(@"/\*(?!!)([\s\S]*?)\*/")]
        private static partial Regex JsBlockComment();

        // Three or more consecutive newlines
        [GeneratedRegex(@"\n{3,}")]
        private static partial Regex ExcessiveBlankLines();
    }
}