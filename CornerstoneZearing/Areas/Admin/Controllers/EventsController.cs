using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
        return View(events);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new EventFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var ev = new Event
        {
            EventID = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        ApplyViewModel(model, ev);

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Event \"{ev.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return View("Form", BuildViewModel(ev));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var ev = await _context.Events.FindAsync(model.EventID);
        if (ev == null) return NotFound();

        ApplyViewModel(model, ev);
        ev.DateModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Event \"{ev.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Event deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static void ApplyViewModel(EventFormViewModel model, Event ev)
    {
        ev.Name = model.Name;
        ev.Location = model.Location ?? string.Empty;
        ev.Description = model.Description ?? string.Empty;
        ev.IsAllDay = model.IsAllDay;
        ev.IsPrivate= model.IsPrivate;

        if (model.IsAllDay)
        {
            ev.StartDateTime = DateTime.SpecifyKind(model.StartDateTime.Date, DateTimeKind.Local).ToUniversalTime();
            ev.EndDateTime = DateTime.SpecifyKind(model.EndDateTime.Date, DateTimeKind.Local).ToUniversalTime();
        }
        else
        {
            ev.StartDateTime = model.StartDateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(model.StartDateTime, DateTimeKind.Local).ToUniversalTime()
                : model.StartDateTime.ToUniversalTime();
            ev.EndDateTime = model.EndDateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(model.EndDateTime, DateTimeKind.Local).ToUniversalTime()
                : model.EndDateTime.ToUniversalTime();
        }

        ev.RecurrenceType = model.RecurrenceType;
        ev.RecurrenceInterval = model.RecurrenceType == RecurrenceType.None ? 1 : model.RecurrenceInterval;
        ev.RecurSunday = model.RecurSunday;
        ev.RecurMonday = model.RecurMonday;
        ev.RecurTuesday = model.RecurTuesday;
        ev.RecurWednesday = model.RecurWednesday;
        ev.RecurThursday = model.RecurThursday;
        ev.RecurFriday = model.RecurFriday;
        ev.RecurSaturday = model.RecurSaturday;
        ev.MonthlyYearlyPattern = model.MonthlyYearlyPattern;
        ev.RecurrenceEndDate = model.RecurrenceType == RecurrenceType.None
            ? null
            : model.RecurrenceEndDate.HasValue
                ? DateTime.SpecifyKind(model.RecurrenceEndDate.Value.Date, DateTimeKind.Local).ToUniversalTime()
                : null;
    }

    private static EventFormViewModel BuildViewModel(Event ev)
    {
        return new EventFormViewModel
        {
            EventID = ev.EventID,
            Name = ev.Name,
            Location = ev.Location,
            Description = ev.Description,
            IsAllDay = ev.IsAllDay,
            IsPrivate = ev.IsPrivate,
            StartDateTime = ev.StartDateTime.ToLocalTime(),
            EndDateTime = ev.EndDateTime.ToLocalTime(),
            RecurrenceType = ev.RecurrenceType,
            RecurrenceInterval = ev.RecurrenceInterval,
            RecurSunday = ev.RecurSunday,
            RecurMonday = ev.RecurMonday,
            RecurTuesday = ev.RecurTuesday,
            RecurWednesday = ev.RecurWednesday,
            RecurThursday = ev.RecurThursday,
            RecurFriday = ev.RecurFriday,
            RecurSaturday = ev.RecurSaturday,
            MonthlyYearlyPattern = ev.MonthlyYearlyPattern,
            RecurrenceEndDate = ev.RecurrenceEndDate.HasValue
                ? ev.RecurrenceEndDate.Value.ToLocalTime()
                : null
        };
    }
}
