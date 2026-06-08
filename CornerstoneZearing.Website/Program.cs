using CornerstoneZearing.Website.Packager;

var builder = WebApplication.CreateBuilder(args);

// Add MVC and Cofoundry services
builder.Services
    .AddMvc()
    .AddCofoundry(builder.Configuration);

// Add packages
builder.Services.AddPackages(packages =>
{
    packages.Add(new StylePackage("~/styles/cornerstone.css")
        .Include("~/styles/theme.css")
        .Include("~/styles/theme-elements.css")
        .Include("~/styles/theme-blog.css")
        .Include("~/styles/theme-church.css")
        .Include("~/styles/custom.css"));

    packages.Add(new ScriptPackage("~/scripts/cornerstone.js")
        .Include("~/scripts/app.js"));
});

// Start web application
var app = builder.Build();
app.UseHttpsRedirection();
app.UseCofoundry();
app.UseStaticFiles();
app.UsePackages();
app.Run();