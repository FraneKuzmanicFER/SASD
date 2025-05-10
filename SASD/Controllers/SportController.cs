using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SASD.Models;

public class SportController : Controller
{
    private readonly ApplicationDbContext _context;

    public SportController(ApplicationDbContext context)
    {
        _context = context;
    }

    // dohvati sve sportove i vrati view sa sportovima
    public async Task<IActionResult> Sports()
    {
        var sports = await _context.Sports.ToListAsync();
        return View(sports);
    }

    // vrati view forme za dodavanje/uređivanje sporta, ako je id null, vrati praznu formu
    public IActionResult CreateEditSportForm(int? id)
    {
        if (id != null)
        {
            var sport = _context.Sports.Find(id);
            return View(sport);
        }
        return View();
    }

    // stvori/uredi sport i vrati view sa sportovima
    [HttpPost]
    public async Task<IActionResult> CreateEditSport(Sport model)
    {

        if (model.Id == 0)
        {
            _context.Sports.Add(model);

        }
        else
        {
            _context.Sports.Update(model);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Sports");
    }

    // Izbriši sport i vrati na view sa sportovima
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var sport = await _context.Sports.FindAsync(id);
        if (sport == null) return NotFound();

        _context.Sports.Remove(sport);
        await _context.SaveChangesAsync();
        return RedirectToAction("Sports");
    }
}
