using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Data;

public class CornerstoneDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public CornerstoneDbContext(DbContextOptions<CornerstoneDbContext> options)
        : base(options)
    {
    }

    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<Sidebar> Sidebars => Set<Sidebar>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Sermon> Sermons => Set<Sermon>();
    public DbSet<SermonCategory> SermonCategories => Set<SermonCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity: custom table names (no "AspNet" prefix).
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.FirstName).HasMaxLength(100);
            b.Property(u => u.LastName).HasMaxLength(100);
        });
        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
            b.Property(r => r.Description).HasMaxLength(256);
        });
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

        ConfigurePage(builder);
        ConfigurePost(builder);
        ConfigureSermon(builder);
        ConfigureSimpleContent(builder);
    }

    private static void ConfigurePage(ModelBuilder builder)
    {
        builder.Entity<Page>(b =>
        {
            b.ToTable("Pages");
            b.HasKey(p => p.PageID);
            b.Property(p => p.Title).HasMaxLength(250).IsRequired();
            b.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            b.Property(p => p.Template).HasMaxLength(100);
            b.Property(p => p.ContentJson).HasColumnType("nvarchar(max)");
            b.Property(p => p.ContentHtml).HasColumnType("nvarchar(max)");
            b.Property(p => p.Status).HasConversion<int>();
            b.Property(p => p.MetaTitle).HasMaxLength(200);
            b.Property(p => p.MetaDescription).HasMaxLength(500);
            b.HasIndex(p => p.Slug).IsUnique();

            b.HasOne(p => p.ParentPage)
                .WithMany(p => p.ChildPages)
                .HasForeignKey(p => p.ParentPageID)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(p => p.Sidebar)
                .WithMany(s => s.Pages)
                .HasForeignKey(p => p.SidebarID)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(p => p.FeaturedMedia)
                .WithMany()
                .HasForeignKey(p => p.FeaturedMediaID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePost(ModelBuilder builder)
    {
        builder.Entity<PostCategory>(b =>
        {
            b.ToTable("PostCategories");
            b.HasKey(c => c.PostCategoryID);
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            b.Property(c => c.Description).HasMaxLength(500);
            b.HasIndex(c => c.Slug).IsUnique();
        });

        builder.Entity<Post>(b =>
        {
            b.ToTable("Posts");
            b.HasKey(p => p.PostID);
            b.Property(p => p.Title).HasMaxLength(250).IsRequired();
            b.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            b.Property(p => p.Summary).HasMaxLength(500);
            b.Property(p => p.ContentJson).HasColumnType("nvarchar(max)");
            b.Property(p => p.ContentHtml).HasColumnType("nvarchar(max)");
            b.Property(p => p.Status).HasConversion<int>();
            b.Property(p => p.MetaTitle).HasMaxLength(200);
            b.Property(p => p.MetaDescription).HasMaxLength(500);
            b.Property(p => p.Tags).HasMaxLength(500);
            b.HasIndex(p => p.Slug).IsUnique();

            b.HasOne(p => p.PostCategory)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.PostCategoryID)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(p => p.FeaturedMedia)
                .WithMany()
                .HasForeignKey(p => p.FeaturedMediaID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSermon(ModelBuilder builder)
    {
        builder.Entity<SermonCategory>(b =>
        {
            b.ToTable("SermonCategories");
            b.HasKey(c => c.SermonCategoryID);
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            b.Property(c => c.Description).HasMaxLength(500);
            b.HasIndex(c => c.Slug).IsUnique();
        });

        builder.Entity<Sermon>(b =>
        {
            b.ToTable("Sermons");
            b.HasKey(s => s.SermonID);
            b.Property(s => s.Title).HasMaxLength(250).IsRequired();
            b.Property(s => s.ContentJson).HasColumnType("nvarchar(max)");
            b.Property(s => s.ContentHtml).HasColumnType("nvarchar(max)");
            b.Property(s => s.Speaker).HasMaxLength(100);
            b.Property(s => s.Status).HasConversion<int>();

            b.HasOne(s => s.SermonCategory)
                .WithMany(c => c.Sermons)
                .HasForeignKey(s => s.SermonCategoryID)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(s => s.FeaturedDocument)
                .WithMany()
                .HasForeignKey(s => s.FeaturedDocumentID)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(s => s.FeaturedMedia)
                .WithMany()
                .HasForeignKey(s => s.FeaturedMediaID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSimpleContent(ModelBuilder builder)
    {
        builder.Entity<Sidebar>(b =>
        {
            b.ToTable("Sidebars");
            b.HasKey(s => s.SidebarID);
            b.Property(s => s.Title).HasMaxLength(100).IsRequired();
            b.Property(s => s.ContentJson).HasColumnType("nvarchar(max)");
            b.Property(s => s.ContentHtml).HasColumnType("nvarchar(max)");
        });

        builder.Entity<Media>(b =>
        {
            b.ToTable("Media");
            b.HasKey(m => m.MediaID);
            b.Property(m => m.OriginalFileName).HasMaxLength(250).IsRequired();
            b.Property(m => m.StoredFileName).HasMaxLength(250).IsRequired();
            b.Property(m => m.ContentType).HasMaxLength(100).IsRequired();
            b.Property(m => m.AltText).HasColumnType("nvarchar(max)");
        });

        builder.Entity<Document>(b =>
        {
            b.ToTable("Documents");
            b.HasKey(d => d.DocumentID);
            b.Property(d => d.OriginalFileName).HasMaxLength(250).IsRequired();
            b.Property(d => d.StoredFileName).HasMaxLength(250).IsRequired();
            b.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
            b.Property(d => d.Description).HasColumnType("nvarchar(max)");
        });

        builder.Entity<Event>(b =>
        {
            b.ToTable("Events");
            b.HasKey(e => e.EventID);
            b.Property(e => e.Title).HasMaxLength(250).IsRequired();
            b.Property(e => e.Location).HasMaxLength(100);
            b.Property(e => e.Description).HasColumnType("nvarchar(max)");
            b.Property(e => e.RecurrenceRule).HasMaxLength(500);
            b.Property(e => e.RecurrenceExceptions).HasColumnType("nvarchar(max)");
        });
    }
}
