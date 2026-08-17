using CornerstoneZearing.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Event> Events { get; set; }

    public DbSet<MediaDocument> MediaDocuments { get; set; }

    public DbSet<MediaImage> MediaImages { get; set; }

    public DbSet<Page> Pages { get; set; }

    public DbSet<Sidebar> Sidebars { get; set; }

    public DbSet<SlideshowSlide> SlideshowSlides { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --------
        // Identity
        // --------

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.Id).HasColumnName("UserID");
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
            b.Property(r => r.Id).HasColumnName("RoleID");
        });

        builder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable("UserRoles");
            b.Property(ur => ur.UserId).HasColumnName("UserID");
            b.Property(ur => ur.RoleId).HasColumnName("RoleID");
        });

        builder.Entity<IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable("UserClaims");
            b.Property(uc => uc.Id).HasColumnName("UserClaimID");
            b.Property(uc => uc.UserId).HasColumnName("UserID");
        });

        builder.Entity<IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable("UserLogins");
            b.Property(ul => ul.UserId).HasColumnName("UserID");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable("RoleClaims");
            b.Property(rc => rc.Id).HasColumnName("RoleClaimID");
            b.Property(rc => rc.RoleId).HasColumnName("RoleID");
        });

        builder.Entity<IdentityUserToken<Guid>>(b =>
        {
            b.ToTable("UserTokens");
            b.Property(ut => ut.UserId).HasColumnName("UserID");
        });

        // -----------
        // Application
        // -----------

        builder.Entity<Event>(b =>
        {
            b.HasKey(e => e.EventID);
            b.Property(e => e.EventID).ValueGeneratedOnAdd();
        });

        builder.Entity<MediaDocument>(b =>
        {
            b.ToTable("MediaDocuments");
            b.HasKey(md => md.MediaDocumentID);
            b.Property(md => md.MediaDocumentID).ValueGeneratedOnAdd();
            b.Property(md => md.OriginalFileName).HasMaxLength(260);
            b.Property(md => md.StoredFileName).HasMaxLength(300);
            b.Property(md => md.ContentType).HasMaxLength(100);
            b.Property(md => md.Description).HasMaxLength(500);
        });

        builder.Entity<MediaImage>(b =>
        {
            b.ToTable("MediaImages");
            b.HasKey(mi => mi.MediaImageID);
            b.Property(mi => mi.MediaImageID).ValueGeneratedOnAdd();
            b.Property(mi => mi.OriginalFileName).HasMaxLength(260);
            b.Property(mi => mi.StoredFileName).HasMaxLength(300);
            b.Property(mi => mi.ContentType).HasMaxLength(100);
            b.Property(mi => mi.AltText).HasMaxLength(500);
        });

        builder.Entity<Page>(b =>
        {
            b.HasKey(p => p.PageID);
            b.HasIndex(p => new { p.ParentPageID, p.UrlSlug }).IsUnique();
            b.Property(p => p.PageID).ValueGeneratedOnAdd();
            b.HasOne(p => p.ParentPage)
                .WithMany(p => p.ChildPages)
                .HasForeignKey(p => p.ParentPageID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Sidebar>(b =>
        {
            b.HasKey(s => s.SidebarID);
            b.Property(s => s.SidebarID).ValueGeneratedOnAdd();
        });

        builder.Entity<SlideshowSlide>(b =>
        {
            b.ToTable("SlideshowSlides");
            b.HasKey(ss => ss.SlideshowSlideID);
            b.Property(ss => ss.SlideshowSlideID).ValueGeneratedOnAdd();
            b.Property(ss => ss.Name).HasMaxLength(260);
            b.Property(ss => ss.StoredFileName).HasMaxLength(300);
            b.Property(ss => ss.AltText).HasMaxLength(500);
            b.Property(ss => ss.Link).HasMaxLength(260);
        });
    }
}