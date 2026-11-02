namespace CornerstoneZearing.Web.Authorization;

public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Posts
    {
        public const string View = "Posts.View";
        public const string Create = "Posts.Create";
        public const string Edit = "Posts.Edit";
        public const string Delete = "Posts.Delete";
    }

    public static class ContentBlocks
    {
        public const string View = "ContentBlocks.View";
        public const string Create = "ContentBlocks.Create";
        public const string Edit = "ContentBlocks.Edit";
        public const string Delete = "ContentBlocks.Delete";
    }

    public static class Media
    {
        public const string View = "Media.View";
        public const string Upload = "Media.Upload";
        public const string Edit = "Media.Edit";
        public const string Delete = "Media.Delete";
    }

    public static class Documents
    {
        public const string View = "Documents.View";
        public const string Upload = "Documents.Upload";
        public const string Edit = "Documents.Edit";
        public const string Delete = "Documents.Delete";
    }

    public static class Events
    {
        public const string View = "Events.View";
        public const string Create = "Events.Create";
        public const string Edit = "Events.Edit";
        public const string Delete = "Events.Delete";
    }

    public static class Sermons
    {
        public const string View = "Sermons.View";
        public const string Create = "Sermons.Create";
        public const string Edit = "Sermons.Edit";
        public const string Delete = "Sermons.Delete";
    }

    public static class Categories
    {
        public const string Manage = "Categories.Manage";
    }

    public static class Users
    {
        public const string Manage = "Users.Manage";
    }

    public static class Roles
    {
        public const string Manage = "Roles.Manage";
    }

    public static IReadOnlyList<PermissionGroup> Groups { get; } = new List<PermissionGroup>
    {
        new("Posts", new[] { Posts.View, Posts.Create, Posts.Edit, Posts.Delete }),
        new("ContentBlocks", new[] { ContentBlocks.View, ContentBlocks.Create, ContentBlocks.Edit, ContentBlocks.Delete }),
        new("Media", new[] { Media.View, Media.Upload, Media.Edit, Media.Delete }),
        new("Documents", new[] { Documents.View, Documents.Upload, Documents.Edit, Documents.Delete }),
        new("Events", new[] { Events.View, Events.Create, Events.Edit, Events.Delete }),
        new("Sermons", new[] { Sermons.View, Sermons.Create, Sermons.Edit, Sermons.Delete }),
        new("Categories", new[] { Categories.Manage }),
        new("Users", new[] { Users.Manage }),
        new("Roles", new[] { Roles.Manage }),
    };

    public static IEnumerable<string> All => Groups.SelectMany(g => g.Permissions);
}

public record PermissionGroup(string Name, IReadOnlyList<string> Permissions);