using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SASD.Models;

public class SportEventController : Controller
{
    private readonly ApplicationDbContext _context;

    public SportEventController(ApplicationDbContext context)
    {
        _context = context;
    }

    // dohvati listu svih termina zajedno sa postignućima igrača i vrati view s tim podacima
    // treba imati na umu da na viewu prikazujemo jedan po jedan termin, a ne sve odjednom, potrebno dodati navigaciju termin po termin
    public async Task<IActionResult> SportEvents()
    {
        var sportEvents = await _context.SportEvents
            .Include(se => se.Sport)
            .Include(se => se.PlayerRecords)
            .ToListAsync();

        return View(sportEvents);
    }

    //  vrati view forme za dodavanje/uređivanje termina, ako je id null, vrati praznu formu, vraćamo i listu sportova kao podatak jer je
    //  potrebno odabrati kroz dropdown listu sport za koji se termin dodaje
    public async Task<IActionResult> CreateEditSportEventForm(int? id)
    {
        ViewBag.Sports = await _context.Sports.ToListAsync(); // dodaj listu sportova u viewbag

        if (id != null)
        {
            var sportEvent = await _context.SportEvents.FindAsync(id);
            if (sportEvent == null) return NotFound();

            var model = new SportEvent
            {
                Id = sportEvent.Id,
                Name = sportEvent.Name,
                StartDate = sportEvent.StartDate,
                EndDate = sportEvent.EndDate,
                MaxNoOfPlayers = sportEvent.MaxNoOfPlayers,
                Location = sportEvent.Location,
                SportId = sportEvent.SportId
            };

            return View(model);
        }

        return View();
    }

    // stvori/uredi termin i vrati view sa terminima, ako se termin uređuje, vraća i id kako bi se na view-u prikazao taj termin, potrebno implementirati
    //na frontendu
    [HttpPost]
    public async Task<IActionResult> CreateEditSportEvent(SportEvent model)
    {

        if (model.Id == 0)
        {
            _context.SportEvents.Add(model);
        }
        else
        {
            _context.SportEvents.Update(model);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("SportEvents", new { id = model.Id });
    }

    // izbriši odabrani termin i vrati view sa terminima
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var sportEvent = await _context.SportEvents.FindAsync(id);
        if (sportEvent == null) return NotFound();

        _context.SportEvents.Remove(sportEvent);
        await _context.SaveChangesAsync();
        return RedirectToAction("SportEvents");
    }

    // vrati view forme za dodavanje/uređivanje postignuća igrača, ako je id null, vrati praznu formu, vraćamo i id sportskog termina
    public async Task<IActionResult> CreateEditPlayerRecordForm(int? id, int sportEventId)
    {
        ViewBag.SportEventId = sportEventId; // dodaj id sportskog termina u viewbag

        if (id != null)
        {
            var playerRecord = await _context.PlayerRecords.FindAsync(id);
            if (playerRecord == null) return NotFound();
            return View(playerRecord);
        }

        return View();
    }

    // stvori/uredi postignuće igrača i vrati view sa terminima, ako se postignuće uređuje, vraća id termina kako bi se na view-u prikazao taj termin, potrebno implementirati na frontendu
    [HttpPost]
    public async Task<IActionResult> CreateEditPlayerRecord(PlayerRecord model)
    {
        if (model.Id == 0)
        {
            _context.PlayerRecords.Add(model);
        }
        else
        {
            _context.PlayerRecords.Update(model);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("SportEvents", new { id = model.SportEventId });
    }

    // izbriši odabrano postignuće igrača i vrati view sa terminima
    [HttpPost]
    public async Task<IActionResult> DeletePlayerRecord(int id, int sportEventId)
    {
        var playerRecord = await _context.PlayerRecords.FindAsync(id);
        if (playerRecord == null) return NotFound();

        _context.PlayerRecords.Remove(playerRecord);
        await _context.SaveChangesAsync();

        return RedirectToAction("SportEvents", new { id = sportEventId });
    }

}
