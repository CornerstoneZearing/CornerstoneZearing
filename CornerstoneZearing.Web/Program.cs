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
    packages.Add(new StylePackage("/styles.css")
        .Include("~/css/site.css")
    );
    packages.Add(new ScriptPackage("/scripts.js")
        .Include("~/js/site.js")
    );
});

// Add authorization services
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, PermissionClaimsPrincipalFactory>();
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(15));
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// CMS services
builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection("Uploads"));
builder.Services.AddScoped<EditorJsRenderer>();
builder.Services.AddScoped<SlugService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<RecurrenceService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddScoped<PageTemplateService>();

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
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Dynamic pages route
app.MapControllerRoute(
    name: "page",
    pattern: "{*slug}",
    defaults: new { controller = "Pages", action = "Show" }
);

// Go
app.Run();