using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Interfaces;
using CornerstoneZearing.Web.Packager;
using CornerstoneZearing.Web.Services;
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
    );
    packages.Add(new ScriptPackage("/scripts/cornerstone.js")
        .Include("~/scripts/theme.js")
    );
    packages.Add(new StylePackage("/styles/cornerstone-admin.css")
        .Include("~/styles/admin-grid.css")
        .Include("~/styles/admin.css")
    );
    packages.Add(new StylePackage("/styles/cornerstone-login.css")
        .Include("~/styles/admin-login.css")
    );
    packages.Add(new ScriptPackage("/scripts/cornerstone-admin.js")
        .Include("~/scripts/admin.js")
    );
});

// Add mail service
builder.Services.AddTransient<IMailService, MailService>();

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

app.Run();