using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CornerstoneZearing.Website.Packager
{
    [HtmlTargetElement("package", TagStructure = TagStructure.WithoutEndTag)]
    public sealed class PackageTagHelper(PackageCollection packages, PackageProcessor processor) : TagHelper
    {
        [HtmlAttributeName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Processes the package tag by retrieving the specified package and generating the appropriate HTML tags for its files.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var package = packages.Get(Name);
            if (package is null)
            {
                return;
            }

            var result = processor.GetOrBuild(package);
            var url = $"{package.VirtualPath}?v={result.hash}";
            var tag = package.Type == PackageType.Style ? $"""<link rel="stylesheet" href="{url}" />""" : $"""<script src="{url}"></script>""";
            output.Content.SetHtmlContent(tag);
        }
    }
}