using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class DashboardController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;

    public DashboardController(CornerstoneDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewBag.Stats = new Dictionary<string, int>
        {
            ["Pages"] = await _db.Pages.CountAsync(),
            ["Posts"] = await _db.Posts.CountAsync(),
            ["Sermons"] = await _db.Sermons.CountAsync(),
            ["Events"] = await _db.Events.CountAsync(),
            ["Media"] = await _db.Media.CountAsync(),
            ["Documents"] = await _db.Documents.CountAsync(),
        };
        return View();
    }
}
