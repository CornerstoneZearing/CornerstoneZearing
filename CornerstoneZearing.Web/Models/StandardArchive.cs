using Piranha.AttributeBuilder;
using Piranha.Models;

namespace CornerstoneZearing.Web.Models
{
    [PageType(Title = "Standard archive", IsArchive = true)]
    public class StandardArchive : Page<StandardArchive>
    {
        /// <summary>
        /// The currently loaded post archive.
        /// </summary>
        public PostArchive<PostInfo> Archive { get; set; }
    }
}
