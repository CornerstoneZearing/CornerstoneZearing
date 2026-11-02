using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Identity;
using CornerstoneZearing.Web.Authorization;
using CornerstoneZearing.Web.Packager;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<CornerstoneDbContext>(options =>
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
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<CornerstoneDbContext>()
.AddDefaultTokenProviders();

// Add mail service
builder.Services.AddTransient<IMailService, MailService>();

// Add Razor pages
builder.Services.AddRazorPages();

// Add controllers with views
builder.Services.AddControllersWithViews();

// Add memory cache
builder.Services.AddMemoryCache();

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add packages
builder.Services.AddPackages(packages =>
{
    packages.Add(new StylePackage("/css/admin.css")
        .Include("~/css/admin/simplebar.css")
        .Include("~/css/admin/style.css")
        .Include("~/css/admin/examples.css")
    );
    packages.Add(new ScriptPackage("/js/admin.js")
        .Include("~/js/admin/config.js")
        .Include("~/js/admin/colormode.js")
        .Include("~/js/admin/coreui.js")
        .Include("~/js/admin/simplebar.js")
    );
    packages.Add(new StylePackage("/styles.css")
        .Include("~/css/theme.css")
        .Include("~/css/events.css")
    );
    packages.Add(new ScriptPackage("/scripts.js")
        .Include("~/js/theme.js")
    );
});

// Add authorization services
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, PermissionClaimsPrincipalFactory>();
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(15));
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// CMS services
builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection("Uploads"));
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<RecurrenceService>();
builder.Services.AddScoped<SlugService>();

// Start web application
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UsePackages();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Make it so
app.Run();