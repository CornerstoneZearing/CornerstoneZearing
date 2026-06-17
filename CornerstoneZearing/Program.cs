using CornerstoneZearing.Data;
using CornerstoneZearing.Packager;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add identity service
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add packages
builder.Services.AddPackages(packages =>
{
    packages.Add(new StylePackage("/styles/cornerstone.css")
        .Include("~/styles/theme.css")
        .Include("~/styles/theme-elements.css")
        .Include("~/styles/theme-blog.css")
        .Include("~/styles/theme-church.css")
        .Include("~/styles/theme-custom.css")
    );
    packages.Add(new ScriptPackage("/scripts/cornerstone.js")
        .Include("~/scripts/theme-custom.js")
    );
    packages.Add(new StylePackage("/styles/cornerstone-admin.css")
        .Include("~/styles/admin.css")
        .Include("~/styles/admin-login.css")
    );
    packages.Add(new ScriptPackage("/scripts/cornerstone-admin.js")
        .Include("~/scripts/admin-custom.js")
    );
});

// Add controllers with views
builder.Services.AddControllersWithViews();

// Start web application
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UsePackages();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Dynamic pages route
app.MapControllerRoute(
    name: "page",
    pattern: "{slug}",
    defaults: new { controller = "Home", action = "Render" });

// Ensure uploads/images and uploads/documents directories exist; migrate any loose image files
MigrateUploads(app.Environment.WebRootPath);

// Database migrations TODO2026 comment out later
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await DataInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred initializing the database. Check your connection string in appsettings.json.");
    }
}

app.Run();

static void MigrateUploads(string webRootPath)
{
    var uploadsPath = Path.Combine(webRootPath, "uploads");
    var imagesPath = Path.Combine(uploadsPath, "images");
    var documentsPath = Path.Combine(uploadsPath, "documents");
    Directory.CreateDirectory(imagesPath);
    Directory.CreateDirectory(documentsPath);

    if (!Directory.Exists(uploadsPath)) return;
    var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
    foreach (var file in Directory.GetFiles(uploadsPath))
    {
        if (!imageExtensions.Contains(Path.GetExtension(file))) continue;
        var dest = Path.Combine(imagesPath, Path.GetFileName(file));
        if (!File.Exists(dest)) File.Move(file, dest);
    }
}