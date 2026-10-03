# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ASP.NET Core MVC website + small CMS for Cornerstone Church of Christ, Zearing, IA. Targets .NET 10, EF Core on SQL Server, ASP.NET Core Identity for admin login. Two projects in `CornerstoneZearing.slnx`:
- `CornerstoneZearing.Data` — class library: `CornerstoneDbContext`, entities (`Entities/`), enums (`Enums/`, e.g. `ContentStatus` Draft/Published/Archived), and Identity types (`Identity/ApplicationUser`, `ApplicationRole`).
- `CornerstoneZearing.Web` — the web app (public controllers/views, `Areas/Admin` CMS, services, authorization, asset packager). References the Data project.

## Commands

- Build: `dotnet build CornerstoneZearing.slnx`
- Run: `dotnet run --project CornerstoneZearing.Web` (profiles/ports in `CornerstoneZearing.Web/Properties/launchSettings.json`)
- There is no test project and no linter configured.

## Database

Schema is **not** managed by EF Core migrations. It lives in hand-written SQL scripts under `SQL/`, named `YYYY-MM-DD <Description>.sql`, applied manually in filename order (`2026-09-08 Initial Schema.sql` creates everything and seeds the Administrator role and its permission claims). When changing an entity's shape, add a new dated script and update the mapping in `CornerstoneDbContext.OnModelCreating`. Also add the new script as a `<File>` under the `/Solution Items/SQL/` folder in `CornerstoneZearing.slnx`. Match the existing script style: a `/* Cornerstone Zearing / description */` header comment, plain DDL batches separated by `GO`, no `IF NOT EXISTS` guards, transactions, or down scripts.

Naming conventions in `CornerstoneDbContext`: content tables use `{Entity}ID` int PKs (`PageID`, `EventID`, ...), configured explicitly in `OnModelCreating`. Identity uses `int` keys, with tables renamed (no `AspNet` prefix: `Users`, `Roles`, `UserRoles`, `RoleClaims`, etc.) but their `Id` columns kept as-is.

## Architecture

**Routing** (`Program.cs`, order matters): `areas` (`{area:exists}/{controller=Dashboard}/...`), then `default`, then a catch-all `{*slug}` → `Pages/Show`. CMS pages are only matched when nothing else is. Some public controllers use attribute routes instead (`EventsController` at `/events`, `MediaFilesController` at `/media-files`).

**Public CMS pages:** `Controllers/PagesController.Show` loads a published `Page` by `Slug` and renders it with the view picked by `PageTemplateService.ResolveViewName(page.Template)`. Templates are discovered by scanning `Views/Templates/*.cshtml` (files beginning with `_` are excluded), with `Default` as the fallback. To add a page template, just drop a new `.cshtml` in that folder; the model is `PageViewModel` (page, optional `Sidebar`, `FeaturedMedia`). Public navigation comes from `NavigationService` (pages with `ShowInNavigation`, tree built from `ParentPageID`). It's memory-cached for 10 minutes, so admin page mutations must call `_navigation.Invalidate()`.

**Admin area** (`Areas/Admin`): all controllers derive from `BaseAdminController` (`[Area("Admin")]`, `[Authorize]`, plus `Success`/`Error` TempData helpers). Login is at `/Admin/Account/Login` (8-hour sliding cookie). The admin layout loads `wwwroot/css/admin.css` and `wwwroot/js/admin.js` directly (not via the packager).

**Permission-based authorization** (`Web/Authorization/`): actions are gated with `[HasPermission(Permissions.X.Y)]`, not roles. Permission strings (e.g. `Pages.Edit`) are defined in `Permissions.cs` and are stored as role claims with claim type `permission`. `PermissionClaimsPrincipalFactory` copies a user's role permission claims into the cookie at sign-in. `PermissionPolicyProvider` builds `permission:{name}` policies on demand. Changes to role permissions only take effect on next sign-in or security-stamp revalidation (15 min). When adding a permission: add the constant in `Permissions.cs` (and to `Permissions.All` so the Roles admin screen can assign it), and add a SQL script granting it to the Administrator role if appropriate.

**Editor.js content:** `Page`, `Post`, `Sermon`, and `Sidebar` store `ContentJson` (Editor.js blocks) and `ContentHtml`. The browser only posts the JSON (`initEditor` in `wwwroot/js/admin.js`, used via `Areas/Admin/Views/Shared/_EditorJs.cshtml`). The admin controllers render HTML **server-side** on save via `EditorJsRenderer.Render(json)`, which also sanitizes it with HtmlSanitizer (tag/attribute allowlist in its constructor). Public views output `ContentHtml` with `Html.Raw`. Custom block plugins are `wwwroot/js/editorjs-*.js` (bootstrap card, bootstrap grid, media image). **Adding a block type requires both** registering it in the `tools` map in `admin.js` **and** adding a `case` in `EditorJsRenderer` (and allowing any new tags/attributes in the sanitizer), or the block will silently drop from the rendered HTML.

**Events/recurrence:** `Event` stores a raw RFC 5545 `RecurrenceRule` (without the `RRULE:` prefix), an optional `RecurrenceEndDate`, newline-separated `RecurrenceExceptions` (EXDATEs), and `AllDay`. `RecurrenceService` uses Ical.Net to expand events into `EventOccurrence`s for a date range (with a safety cap) and to produce iCal output. Public `/events` serves a calendar view, a JSON `Feed(start, end)`, `Detail`, and an `.ics` feed. Datetimes are handled as `DateTimeKind.Unspecified` local times.

**Uploads:** `FileStorageService` (configured by the `Uploads` section of `appsettings.json`) stores files under `App_Data/uploads/{category}/yyyy/MM/{guid}{ext}`, outside `wwwroot`. Uploaded files are served through `MediaFilesController` (`/media-files/...`) by `Media`/`Document` ID. ImageSharp reads image dimensions.

**Asset packager** (`Packager/`): a hand-rolled bundler/minifier. Packages are declared in `Program.cs` via `AddPackages(...)`, served by `PackageMiddleware` (`app.UsePackages()`, before routing), cached in memory, and referenced with the `<package name="..." />` tag helper. Currently `/styles.css` and `/scripts.js` are registered, but the layouts link `site.css`/`admin.css`/`admin.js` directly, so check which mechanism a view actually uses before editing assets.

**Config:** `appsettings.json` contains real DB/SMTP host info. Override secrets locally in `appsettings.Development.json` rather than editing the committed file. `IMailService`/`MailService` sends SMTP mail using the `Smtp` section.
