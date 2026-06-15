using CornerstoneZearing.Areas.Admin.Models;
using CornerstoneZearing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator,Editor")]
public class EventsController : Controller
{
    private readonly ApplicationDbContext _DbContext;

    /// <summary>
    /// Initialization constructor.
    /// </summary>
    /// <param name="context"></param>
    public EventsController(ApplicationDbContext context)
    {
        _DbContext = context;
    }

    /// <summary>
    /// List page.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var events = await _DbContext.Events
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
        return View(events);
    }

    /// <summary>
    /// Create page.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new EventFormModel());
    }

    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var evnt = new Event
        {
            EventID = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        ApplyModel(model, evnt);

        _DbContext.Events.Add(evnt);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Event \"{evnt.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Edit page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var evnt = await _DbContext.Events.FindAsync(id);
        if (evnt == null)
        {
            return NotFound();
        }

        var model = new EventFormModel
        {
            EventID = evnt.EventID,
            Name = evnt.Name,
            Location = evnt.Location,
            Description = evnt.Description,
            IsAllDay = evnt.IsAllDay,
            IsPrivate = evnt.IsPrivate,
            StartDateTime = evnt.StartDateTime.ToLocalTime(),
            EndDateTime = evnt.EndDateTime.ToLocalTime(),
            RecurrenceType = evnt.RecurrenceType,
            RecurrenceInterval = evnt.RecurrenceInterval,
            RecurSunday = evnt.RecurSunday,
            RecurMonday = evnt.RecurMonday,
            RecurTuesday = evnt.RecurTuesday,
            RecurWednesday = evnt.RecurWednesday,
            RecurThursday = evnt.RecurThursday,
            RecurFriday = evnt.RecurFriday,
            RecurSaturday = evnt.RecurSaturday,
            MonthlyYearlyPattern = evnt.MonthlyYearlyPattern,
            RecurrenceEndDate = evnt.RecurrenceEndDate.HasValue ? evnt.RecurrenceEndDate.Value.ToLocalTime() : null
        };

        return View("Form", model);
    }

    /// <summary>
    /// Updates an event.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var evnt = await _DbContext.Events.FindAsync(model.EventID);
        if (evnt == null)
        {
            return NotFound();
        }

        ApplyModel(model, evnt);
        evnt.DateModified = DateTime.UtcNow;
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = $"Event \"{evnt.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Delete confirmation page.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var evnt = await _DbContext.Events.FindAsync(id);
        if (evnt == null)
        {
            return NotFound();
        }

        return View(evnt);
    }

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var evnt = await _DbContext.Events.FindAsync(id);
        if (evnt == null)
        {
            return NotFound();
        }

        _DbContext.Events.Remove(evnt);
        await _DbContext.SaveChangesAsync();

        TempData["Success"] = "Event deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Applies values from the event model to the event entity.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="evnt"></param>
    private static void ApplyModel(EventFormModel model, Event evnt)
    {
        evnt.Name = model.Name;
        evnt.Location = model.Location ?? string.Empty;
        evnt.Description = model.Description ?? string.Empty;
        evnt.IsAllDay = model.IsAllDay;
        evnt.IsPrivate = model.IsPrivate;

        if (model.IsAllDay)
        {
            evnt.StartDateTime = DateTime.SpecifyKind(model.StartDateTime.Date, DateTimeKind.Local).ToUniversalTime();
            evnt.EndDateTime = DateTime.SpecifyKind(model.EndDateTime.Date, DateTimeKind.Local).ToUniversalTime();
        }
        else
        {
            evnt.StartDateTime = model.StartDateTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(model.StartDateTime, DateTimeKind.Local).ToUniversalTime() : model.StartDateTime.ToUniversalTime();
            evnt.EndDateTime = model.EndDateTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(model.EndDateTime, DateTimeKind.Local).ToUniversalTime() : model.EndDateTime.ToUniversalTime();
        }

        evnt.RecurrenceType = model.RecurrenceType;
        evnt.RecurrenceInterval = model.RecurrenceType == RecurrenceType.None ? 1 : model.RecurrenceInterval;
        evnt.RecurSunday = model.RecurSunday;
        evnt.RecurMonday = model.RecurMonday;
        evnt.RecurTuesday = model.RecurTuesday;
        evnt.RecurWednesday = model.RecurWednesday;
        evnt.RecurThursday = model.RecurThursday;
        evnt.RecurFriday = model.RecurFriday;
        evnt.RecurSaturday = model.RecurSaturday;
        evnt.MonthlyYearlyPattern = model.MonthlyYearlyPattern;
        evnt.RecurrenceEndDate = model.RecurrenceType == RecurrenceType.None ? null : model.RecurrenceEndDate.HasValue ? DateTime.SpecifyKind(model.RecurrenceEndDate.Value.Date, DateTimeKind.Local).ToUniversalTime() : null;
    }
}