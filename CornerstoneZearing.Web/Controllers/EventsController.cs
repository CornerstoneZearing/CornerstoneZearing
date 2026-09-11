using CornerstoneZearing.Data;
using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Controllers;

[Route("events")]
public class EventsController : Controller
{
    private readonly CornerstoneDbContext _db;
    private readonly RecurrenceService _recurrence;

    public EventsController(CornerstoneDbContext db, RecurrenceService recurrence)
    {
        _db = db;
        _recurrence = recurrence;
    }

    private bool CanSeePrivate => User.Identity?.IsAuthenticated == true;

    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("feed")]
    public async Task<IActionResult> Feed(DateTime start, DateTime end)
    {
        if (end <= start || (end - start).TotalDays > 400)
            return BadRequest();

        var events = await _db.Events
            .Where(e => CanSeePrivate || !e.Private)
            .ToListAsync();

        var items = _recurrence.ExpandAll(events, start, end).Select(o => new
        {
            id = o.EventID,
            title = o.Title,
            start = o.Start.ToString("s"),
            end = o.End.ToString("s"),
            url = Url.Action("Detail", new { id = o.EventID }),
            extendedProps = new { location = o.Location, isPrivate = o.Private },
        });

        return Json(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null || (ev.Private && !CanSeePrivate)) return NotFound();
        return View(ev);
    }

    [HttpGet("feed.ics")]
    public async Task<IActionResult> Ics()
    {
        var events = await _db.Events.Where(e => !e.Private).ToListAsync();
        var ics = _recurrence.ToIcs(events);
        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", "cornerstone-events.ics");
    }
}
