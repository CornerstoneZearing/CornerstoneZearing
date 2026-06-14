using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Data;

public static class DataInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();
        await EnsureEventsTableAsync(context);

        string[] roles = ["Administrator", "Editor", "Member"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        const string adminEmail = "ryan@cornerstonezearing.org";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Site",
                LastName = "Administrator"
            };

            var result = await userManager.CreateAsync(admin, "Proclarush^96");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }
    }

    private static async Task EnsureEventsTableAsync(ApplicationDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Events')
            BEGIN
                CREATE TABLE Events (
                    EventID uniqueidentifier NOT NULL,
                    Name nvarchar(200) NOT NULL,
                    Location nvarchar(200) NOT NULL,
                    StartDateTime datetime2 NOT NULL,
                    EndDateTime datetime2 NOT NULL,
                    Description nvarchar(max) NOT NULL,
                    IsAllDay bit NOT NULL,
                    IsPrivate bit NOT NULL DEFAULT 0,
                    RecurrenceType int NOT NULL,
                    RecurrenceInterval int NOT NULL,
                    RecurSunday bit NOT NULL,
                    RecurMonday bit NOT NULL,
                    RecurTuesday bit NOT NULL,
                    RecurWednesday bit NOT NULL,
                    RecurThursday bit NOT NULL,
                    RecurFriday bit NOT NULL,
                    RecurSaturday bit NOT NULL,
                    MonthlyYearlyPattern int NOT NULL,
                    RecurrenceEndDate datetime2 NULL,
                    DateCreated datetime2 NOT NULL,
                    DateModified datetime2 NOT NULL,
                    CONSTRAINT PK_Events PRIMARY KEY (EventID)
                )
            END

            IF NOT EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('Events') AND name = 'IsPrivate'
            )
            BEGIN
                ALTER TABLE Events ADD IsPrivate bit NOT NULL DEFAULT 0
            END");
    }
}