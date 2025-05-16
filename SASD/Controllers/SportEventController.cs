// SASD/Controllers/SportEventController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SASD.Models;
using SASD.ViewModels; // Add this using statement
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for List

public class SportEventController : Controller
{
    private readonly ApplicationDbContext _context;

    public SportEventController(ApplicationDbContext context)
    {
        _context = context;
    }

    // MODIFIED ACTION for master-detail view with navigation
    public async Task<IActionResult> SportEvents(int? id)
    {
        var viewModel = new SportEventDetailViewModel();

        var allEventIds = await _context.SportEvents
                                        .OrderBy(se => se.StartDate) // Or OrderBy(se => se.Id)
                                        .Select(se => se.Id)
                                        .ToListAsync();

        if (!allEventIds.Any())
        {
            viewModel.HasEvents = false;
            return View(viewModel); // Or RedirectToAction("NoEventsView") or pass a message
        }

        viewModel.HasEvents = true;
        int currentEventId = id ?? allEventIds.First();

        viewModel.CurrentSportEvent = await _context.SportEvents
            .Include(se => se.Sport)
            .Include(se => se.PlayerRecords)
            .FirstOrDefaultAsync(se => se.Id == currentEventId);

        if (viewModel.CurrentSportEvent == null)
        {
            // If an invalid ID is passed, default to the first event
            if (allEventIds.Any())
            {
                currentEventId = allEventIds.First();
                viewModel.CurrentSportEvent = await _context.SportEvents
                   .Include(se => se.Sport)
                   .Include(se => se.PlayerRecords)
                   .FirstOrDefaultAsync(se => se.Id == currentEventId);
            }
            else
            {
                viewModel.HasEvents = false;
                return View(viewModel); // No events found at all
            }
            if (viewModel.CurrentSportEvent == null) return NotFound("No sport events found.");
        }


        var currentIndex = allEventIds.IndexOf(currentEventId);

        if (currentIndex > 0)
            viewModel.PreviousEventId = allEventIds[currentIndex - 1];

        if (currentIndex < allEventIds.Count - 1)
            viewModel.NextEventId = allEventIds[currentIndex + 1];

        return View(viewModel);
    }

    // vrati view forme za dodavanje/uređivanje termina, ako je id null, vrati praznu formu, vraćamo i listu sportova kao podatak jer je
    // potrebno odabrati kroz dropdown listu sport za koji se termin dodaje
    public async Task<IActionResult> CreateEditSportEventForm(int? id)
    {
        ViewBag.Sports = await _context.Sports.OrderBy(s => s.Name).ToListAsync();

        if (id != null)
        {
            var sportEvent = await _context.SportEvents.FindAsync(id);
            if (sportEvent == null) return NotFound();
            // Pass the existing entity to the view for editing
            return View(sportEvent);
        }
        // Pass a new SportEvent model for creation, potentially with default values
        return View(new SportEvent { StartDate = DateTime.Today, EndDate = DateTime.Today.AddHours(2) });
    }

    // stvori/uredi termin i vrati view sa terminima, ako se termin uređuje, vraća i id kako bi se na view-u prikazao taj termin
    [HttpPost]
    [ValidateAntiForgeryToken] // Good practice to prevent CSRF attacks
    public async Task<IActionResult> CreateEditSportEvent(SportEvent model)
    {
        // Basic validation example (you should add more comprehensive validation)
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date must be after start date.");
        }
        if (model.SportId == 0)
        {
            ModelState.AddModelError("SportId", "Please select a sport.");
        }


        if (ModelState.IsValid)
        {
            // Ensure StartDate and EndDate are UTC before saving
            model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
            model.EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);

            if (model.Id == 0) // Creating new event
            {
                _context.SportEvents.Add(model);
            }
            else // Editing existing event
            {
                // It's generally safer to fetch the entity and then update its properties
                // to prevent overposting attacks if not all fields are bound or intended for update.
                var eventToUpdate = await _context.SportEvents.FindAsync(model.Id);
                if (eventToUpdate == null) return NotFound();

                eventToUpdate.Name = model.Name;
                eventToUpdate.StartDate = model.StartDate;
                eventToUpdate.EndDate = model.EndDate;
                eventToUpdate.MaxNoOfPlayers = model.MaxNoOfPlayers;
                eventToUpdate.Location = model.Location;
                eventToUpdate.SportId = model.SportId;
                // _context.SportEvents.Update(eventToUpdate); // EF Core tracks changes, explicit Update is often not needed if tracked
            }

            await _context.SaveChangesAsync();
            // Redirect to the SportEvents view, displaying the created/edited event
            return RedirectToAction("SportEvents", new { id = model.Id });
        }

        // If model state is invalid, return to the form with the current model and errors
        ViewBag.Sports = await _context.Sports.OrderBy(s => s.Name).ToListAsync();
        return View("CreateEditSportEventForm", model);
    }

    // izbriši odabrani termin i vrati view sa terminima
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSportEvent(int id) // Renamed to avoid conflict with SportController.Delete
    {
        var sportEvent = await _context.SportEvents
                                     .Include(se => se.PlayerRecords) // Important: load related records for cascading delete or checking
                                     .FirstOrDefaultAsync(se => se.Id == id);
        if (sportEvent == null) return NotFound();

        // Optional: Check if there are player records. You might want to prevent deletion or handle it.
        if (sportEvent.PlayerRecords != null && sportEvent.PlayerRecords.Any())
        {
            // Example: Disallow deletion if players are registered, or delete them too (configure cascading delete in DB/EF)
            // For now, we'll assume deletion is allowed and player records will be handled by DB constraints or EF config
            _context.PlayerRecords.RemoveRange(sportEvent.PlayerRecords); // Explicitly remove player records if not cascaded
        }

        _context.SportEvents.Remove(sportEvent);
        await _context.SaveChangesAsync();
        // Redirect to the main SportEvents page (will show the first event by default)
        return RedirectToAction("SportEvents");
    }

    // vrati view forme za dodavanje/uređivanje postignuća igrača, ako je id null, vrati praznu formu, vraćamo i id sportskog termina
    public async Task<IActionResult> CreateEditPlayerRecordForm(int? id, int sportEventId)
    {
        ViewBag.SportEventId = sportEventId;
        var sportEvent = await _context.SportEvents.FindAsync(sportEventId);
        if (sportEvent == null) return NotFound("Sport Event not found.");
        ViewBag.SportEventName = sportEvent.Name; // For display on the form

        if (id != null) // Editing existing record
        {
            var playerRecord = await _context.PlayerRecords.FindAsync(id);
            if (playerRecord == null) return NotFound("Player Record not found.");
            if (playerRecord.SportEventId != sportEventId) return BadRequest("Record does not belong to this event.");
            return View(playerRecord);
        }

        // Creating new record
        return View(new PlayerRecord { SportEventId = sportEventId });
    }

    // stvori/uredi postignuće igrača i vrati view sa terminima
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEditPlayerRecord(PlayerRecord model)
    {
        if (ModelState.IsValid)
        {
            if (model.Id == 0) // Creating new record
            {
                System.Diagnostics.Debug.WriteLine($"  -------------------------------- PROBLEM: Entity with ID {model.Id} is trying to be added]");
                _context.PlayerRecords.Add(model);
            }
            else // Editing existing record
            {
                var recordToUpdate = await _context.PlayerRecords.FindAsync(model.Id);
                if (recordToUpdate == null) return NotFound();

                // Ensure the SportEventId is not changed maliciously if it's part of the form
                if (recordToUpdate.SportEventId != model.SportEventId) return BadRequest("Cannot change the Sport Event association for this record.");

                recordToUpdate.PlayerName = model.PlayerName;
                recordToUpdate.PlayerSurname = model.PlayerSurname;
                recordToUpdate.NoOfPoints = model.NoOfPoints;
                recordToUpdate.Arrived = model.Arrived;
                recordToUpdate.Description = model.Description;
                //_context.PlayerRecords.Update(recordToUpdate); // EF Core tracks changes
                // ---- DIAGNOSTIC CODE START ----
                System.Diagnostics.Debug.WriteLine($"--- Debugging PlayerRecord Update for ID: {model.Id} ---");
                var entryForRecordToUpdate = _context.Entry(recordToUpdate);
                System.Diagnostics.Debug.WriteLine($"State of 'recordToUpdate' (ID: {recordToUpdate.Id}): {entryForRecordToUpdate.State}");

                // Check if the 'model' instance (from form) is somehow being tracked
                var entryForModelFromForm = _context.Entry(model);
                System.Diagnostics.Debug.WriteLine($"State of 'model' from form (ID: {model.Id}): {entryForModelFromForm.State}");

                System.Diagnostics.Debug.WriteLine("All tracked PlayerRecord entities:");
                foreach (var trackedEntry in _context.ChangeTracker.Entries<PlayerRecord>())
                {
                    System.Diagnostics.Debug.WriteLine($"  Tracked ID: {trackedEntry.Entity.Id}, State: {trackedEntry.State}");
                    if (trackedEntry.Entity.Id == model.Id && trackedEntry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
                    {
                        System.Diagnostics.Debug.WriteLine($"  PROBLEM: Entity with ID {model.Id} is unexpectedly in 'Added' state!");
                    }
                }
                System.Diagnostics.Debug.WriteLine("--- End Debugging ---");
                // ---- DIAGNOSTIC CODE END ----
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("SportEvents", new { id = model.SportEventId });
        }
        // If model state invalid, return to form
        var sportEvent = await _context.SportEvents.FindAsync(model.SportEventId);
        ViewBag.SportEventName = sportEvent?.Name;
        return View("CreateEditPlayerRecordForm", model);
    }

    // izbriši odabrano postignuće igrača i vrati view sa terminima
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePlayerRecord(int id) // Removed sportEventId from params, get it from record
    {
        var playerRecord = await _context.PlayerRecords.FindAsync(id);
        if (playerRecord == null) return NotFound();

        int sportEventIdToRedirect = playerRecord.SportEventId; // Store before removing

        _context.PlayerRecords.Remove(playerRecord);
        await _context.SaveChangesAsync();

        return RedirectToAction("SportEvents", new { id = sportEventIdToRedirect });
    }
}