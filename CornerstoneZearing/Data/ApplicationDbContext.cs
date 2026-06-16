using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Data;

public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Page> Pages { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<MediaImage> MediaImages { get; set; }
    public DbSet<MediaDocument> MediaDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        builder.Entity<Page>(b =>
        {
            b.HasKey(p => p.PageID);
            b.HasIndex(p => p.UrlSlug).IsUnique();
            b.Property(p => p.PageID).ValueGeneratedOnAdd();
        });

        builder.Entity<Event>(b =>
        {
            b.HasKey(e => e.EventID);
            b.Property(e => e.EventID).ValueGeneratedOnAdd();
        });

        builder.Entity<MediaImage>(b =>
        {
            b.ToTable("MediaImages");
            b.HasKey(m => m.MediaImageID);
            b.Property(m => m.MediaImageID).ValueGeneratedOnAdd();
            b.Property(m => m.OriginalFileName).HasMaxLength(260);
            b.Property(m => m.StoredFileName).HasMaxLength(300);
            b.Property(m => m.ContentType).HasMaxLength(100);
            b.Property(m => m.AltText).HasMaxLength(500);
        });

        builder.Entity<MediaDocument>(b =>
        {
            b.ToTable("MediaDocuments");
            b.HasKey(d => d.MediaDocumentID);
            b.Property(d => d.MediaDocumentID).ValueGeneratedOnAdd();
            b.Property(d => d.OriginalFileName).HasMaxLength(260);
            b.Property(d => d.StoredFileName).HasMaxLength(300);
            b.Property(d => d.ContentType).HasMaxLength(100);
            b.Property(d => d.Description).HasMaxLength(500);
        });
    }
}
