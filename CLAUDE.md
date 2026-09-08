# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ASP.NET Core MVC website for Cornerstone Church of Christ, Zearing, IA, targeting .NET 10, using EF Core with SQL Server and ASP.NET Core Identity for the admin login. Two projects are in the solution:
- `CornerstoneZearing.Web/CornerstoneZearing.Web.csproj` — the web app (controllers, views, admin area, asset pipeline), namespace root `CornerstoneZearing.Web`.
- `CornerstoneZearing.Data/CornerstoneZearing.Data.csproj` — class library holding `ApplicationDbContext` and all entities. Entity classes and their enums live under the `CornerstoneZearing.Data.Entities` namespace/folder; the `DbContext` itself is `CornerstoneZearing.Data`. The web project references this project.

## Commands

Run from the repo root against the solution, or from `CornerstoneZearing.Web/` for the web project directly.

- Build: `dotnet build CornerstoneZearing.slnx`
- Run locally: `dotnet run --project CornerstoneZearing.Web` (see `CornerstoneZearing.Web/Properties/launchSettings.json` for profiles/ports)
- Restore packages: `dotnet restore`
- There is no test project in this repo currently.

Database schema is not managed via EF Core migrations — it's hand-written SQL scripts under `SQL/` (e.g. `SQL/2026-06-12 ASP.NET Identity.sql`), named `YYYY-MM-DD <Description>.sql` and applied manually/in order by filename date to a SQL Server instance. When changing an entity's shape, add a new dated `.sql` script rather than generating an EF migration. Existing scripts are plain sequential DDL batches separated by `GO` — no `IF NOT EXISTS` guards, no transactions, no rollback/down script — match that style rather than adding idempotency guards.

## Architecture

**Two-tier routing split (public vs. admin):**
- Public site: `Controllers/` + `Views/` (top-level). `HomeController.Index()`/`Render(slug)` load a published `Page` from the DB by `UrlSlug` and render it through `Views/Templates/{page.TemplateName}.cshtml` — pages are CMS-driven, not static views. `Views/Templates/Default.cshtml` renders `Page.ContentHtml` as raw HTML.
- Admin site: `Areas/Admin/` — full MVC area (`Controllers/`, `Views/`, `Models/`, `Helpers/`) protected by `[Authorize(Roles = "Administrator,Editor")]`, routed via the `{area:exists}/{controller=Home}/{action=Index}/{id?}` route in `Program.cs`. Admin login lives at `/Admin/Account/Login` (configured via `ConfigureApplicationCookie` in `Program.cs`: 8-hour sliding expiration cookie).
- Route registration order in `Program.cs` matters: the `areas` route, then the `default` route, then the catch-all `page` route (`{slug}` → `Home/Render`) last, so it only matches when no other route (including area/controller routes) does.
- Middleware pipeline order in `Program.cs`: HTTPS redirect/static files → `UsePackages()` → `UseRouting()` → `UseAuthentication()` → `UseAuthorization()` → the three routes above. `UseExceptionHandler("/Home/Error")` and `UseHsts()` are only added outside `Development`.
- Identity password/lockout policy is configured in `Program.cs`: passwords require digit, lowercase, uppercase, and non-alphanumeric with a minimum length of 8; lockout triggers after 5 failed attempts for 15 minutes; `RequireUniqueEmail = true`.

**Identity model:** `ApplicationDbContext` (`CornerstoneZearing.Data/ApplicationDbContext.cs`) extends `IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ...>` with all Identity tables renamed (`Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `RoleClaims`, `UserTokens`) and PK columns renamed to `<Entity>ID` to match the hand-written SQL schema. `ApplicationUser`/`ApplicationRole` (in `CornerstoneZearing.Data/Entities/`) are the custom Identity subclasses. Follow this same table/column-naming convention (`{Entity}ID` PK, `ValueGeneratedOnAdd()`) for any new entity added to `ApplicationDbContext.OnModelCreating`.

**Content entities** (`CornerstoneZearing.Data/Entities/`, namespace `CornerstoneZearing.Data.Entities`): `Page` (CMS page with `TemplateName`, `UrlSlug`, `PageStatus` draft/published/withdrawn, plus self-referential `ParentPageID` for the page hierarchy / breadcrumbs), `Event` (calendar event with full recurrence rules — daily/weekly/monthly/yearly, day-of-week flags, `MonthlyYearlyPattern`), `MediaImage`/`MediaDocument` (uploaded file metadata), `SlideshowSlide`, `Sidebar` (named reusable sidebar content block, same dual `ContentJson`/`ContentHtml` storage as `Page`). Entity-related enums (`PageStatus`, `RecurrenceType`, `MonthlyYearlyPattern`) live in `Entities/Enums.cs` in that project — anything referenced by an entity type belongs here, not in the web project's `Enums.cs` (which only holds `PackageType` for the asset pipeline below).

**Editor.js content model:** `Page` (and `Sidebar`) store content twice — `ContentJson` (Editor.js block data) and `ContentHtml` (rendered HTML, what `Views/Templates/Default.cshtml` renders on the public site). The HTML is rendered **client-side** in the browser by `edjsHTML` inside `Areas/Admin/Views/Pages/Form.cshtml` (and `Sidebars/Form.cshtml`) and posted to the controller as a hidden `ContentHtml` field on save — the controller just persists both strings, it does not render. Pages saved before `ContentJson` was tracked fall back to a single `raw` HTML block.

Custom Editor.js block plugins live in `wwwroot/scripts/editorjs-*.js` (`editorjs-bootstrap-card.js`, `editorjs-bootstrap-grid.js`, `editorjs-media-image.js`). Every block type registered in the `tools` map in `Form.cshtml` must also have an explicit parser entry in the `edjsHTML({...})` config in the same file (edjsHTML's built-ins only cover the standard blocks) — add both when adding a block type.

**Custom asset packaging pipeline** (`Packager/`) — a small hand-rolled bundler/minifier, not webpack/vite/gulp:
- `Package`/`StylePackage`/`ScriptPackage`: named virtual bundles built from a list of `~/`-relative wwwroot files.
- Packages are declared once in `Program.cs` via `builder.Services.AddPackages(packages => { ... })`.
- `PackageProcessor` concatenates + minifies (`PackageMinifier`) the source files on first request and caches the result in-memory, keyed by virtual path; output is content-hashed for the ETag.
- `PackageMiddleware` (registered via `app.UsePackages()`, before `UseRouting`) intercepts requests matching a package's virtual path and serves the cached/minified bundle directly, bypassing MVC.
- The `<package name="..." />` Razor tag helper (`PackageTagHelper`) emits the right `<link>`/`<script>` tag with a cache-busting `?v={hash}` query string.
- To add a new CSS/JS bundle: add source files under `wwwroot/styles` or `wwwroot/scripts`, register a new `StylePackage`/`ScriptPackage` in `Program.cs`, then reference it in a view with `<package name="/styles/your-bundle.css" />` (or `.js`).

**Mail:** `IMailService`/`MailService` (SMTP-based) registered as transient in `Program.cs`; SMTP settings come from the `Smtp` section of `appsettings.json` (credentials left blank in source — set via `appsettings.Development.json`, which is gitignored, or environment-specific config in production).

**Connection strings / secrets:** `appsettings.json` in this repo contains real-looking SMTP host and DB connection info committed to source — treat this file as environment config to be overridden locally via `appsettings.Development.json`, not as a template to fill in blindly.
