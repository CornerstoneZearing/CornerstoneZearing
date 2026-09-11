using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Data.Enums;
using CornerstoneZearing.Web.Models;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Controllers;

public class HomeController : Controller
{
    private readonly CornerstoneDbContext _db;
    private readonly RecurrenceService _recurrence;

    public HomeController(CornerstoneDbContext db, RecurrenceService recurrence)
    {
        _db = db;
        _recurrence = recurrence;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var vm = new HomeViewModel
        {
            RecentPosts = await _db.Posts
                .Where(p => p.Status == ContentStatus.Published)
                .OrderByDescending(p => p.DatePublished ?? p.DateCreated)
                .Take(3).ToListAsync(),
            LatestSermon = await _db.Sermons
                .Where(s => s.Status == ContentStatus.Published)
                .OrderByDescending(s => s.SermonDate)
                .FirstOrDefaultAsync(),
        };

        var events = await _db.Events.Where(e => !e.Private).ToListAsync();
        vm.UpcomingEvents = _recurrence.ExpandAll(events, now, now.AddMonths(2))
            .Where(o => o.Start >= now)
            .OrderBy(o => o.Start)
            .Take(5)
            .ToList();

        return View(vm);
    }

    [Route("Home/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
