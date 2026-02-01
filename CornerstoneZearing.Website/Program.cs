using CornerstoneZearing.Website.Identity;
using CornerstoneZearing.Website.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

// Create the web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("CornerstoneZearing") ??
    throw new InvalidOperationException("Connection string 'CornerstoneZearing' not found.");

// Register SQL Server provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register database exception filter for development
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure ASP.NET Identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure SMTP Service
builder.Services.AddTransient<IEmailSender, SmtpService>();

// Turn on MVC services
builder.Services.AddControllersWithViews();

// Build the web application
var app = builder.Build();

// Configure the request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Turn on endpoint routing
app.UseRouting();

// Turn on authorization
app.UseAuthorization();

// Turn on static file handling
app.MapStaticAssets();

// Configure admin area route
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}"
);

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Turn on Razor Pages
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// P@ssw0rd123!