using CornerstoneZearing.Data;
using CornerstoneZearing.Data.Entities;
using CornerstoneZearing.Web.Areas.Admin.Models;
using CornerstoneZearing.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CornerstoneZearing.Web.Areas.Admin.Controllers;

public class EventsController : BaseAdminController
{
    private readonly CornerstoneDbContext _db;

    public EventsController(CornerstoneDbContext db) => _db = db;

    [HasPermission(Permissions.Events.View)]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Events";
        var events = await _db.Events.OrderByDescending(e => e.StartDateTime).ToListAsync();
        return View(events);
    }

    [HasPermission(Permissions.Events.Create)]
    public IActionResult Create()
    {
        ViewData["Title"] = "New event";
        return View("Edit", new EventEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Events.Create)]
    public async Task<IActionResult> Create(EventEditViewModel vm)
    {
        Validate(vm);
        if (!ModelState.IsValid) return View("Edit", vm);

        var ev = new Event();
        Apply(vm, ev, true);
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        Success("Event created.");
        return RedirectToIndex();
    }

    [HasPermission(Permissions.Events.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e is null) return NotFound();

        ViewData["Title"] = "Edit event";
        return View(new EventEditViewModel
        {
            EventID = e.EventID,
            Title = e.Title,
            StartDateTime = e.StartDateTime,
            EndDateTime = e.EndDateTime,
            Location = e.Location,
            Description = e.Description,
            Private = e.Private,
            RecurrenceRule = e.RecurrenceRule,
            RecurrenceEndDate = e.RecurrenceEndDate,
            RecurrenceExceptions = e.RecurrenceExceptions,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Events.Edit)]
    public async Task<IActionResult> Edit(int id, EventEditViewModel vm)
    {
        var e = await _db.Events.FindAsync(id);
        if (e is null) return NotFound();

        Validate(vm);
        if (!ModelState.IsValid) return View(vm);

        Apply(vm, e, false);
        await _db.SaveChangesAsync();
        Success("Event saved.");
        return RedirectToIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission(Permissions.Events.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e is null) return NotFound();
        _db.Events.Remove(e);
        await _db.SaveChangesAsync();
        Success("Event deleted.");
        return RedirectToIndex();
    }

    private void Validate(EventEditViewModel vm)
    {
        if (vm.EndDateTime < vm.StartDateTime)
            ModelState.AddModelError(nameof(vm.EndDateTime), "End must be on or after the start.");
    }

    private static void Apply(EventEditViewModel vm, Event ev, bool isNew)
    {
        var now = DateTime.UtcNow;
        ev.Title = vm.Title.Trim();
        ev.StartDateTime = vm.StartDateTime;
        ev.EndDateTime = vm.EndDateTime;
        ev.Location = vm.Location;
        ev.Description = vm.Description;
        ev.Private = vm.Private;
        ev.RecurrenceRule = string.IsNullOrWhiteSpace(vm.RecurrenceRule) ? null : vm.RecurrenceRule.Trim();
        ev.RecurrenceEndDate = vm.RecurrenceEndDate;
        ev.RecurrenceExceptions = string.IsNullOrWhiteSpace(vm.RecurrenceExceptions) ? null : vm.RecurrenceExceptions.Trim();
        ev.DateModified = now;
        if (isNew) ev.DateCreated = now;
    }
}
