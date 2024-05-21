using EventManagementSystem.Data;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EventManagementSystem.Models.UserRelated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Controllers
{
    public class GuestController : Controller
{
    private readonly UserManager<AccountModel> userManager;
    private readonly ApplicationDbContext context;

    public GuestController(UserManager<AccountModel> userManager, ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.context = context;
    }

    public async Task<IActionResult> List(int eventId)
    {
        var eventModel = await context.Events.FindAsync(eventId);
        var guests = await context.Guests
                                   .Where(g => g.EventId == eventId)
                                   .ToListAsync();
        
        var guestListViewModel = new GuestListViewModel()
        {
            Event = eventModel,
            Guests = guests,
        };
        
        return View(guestListViewModel);
    }

    public IActionResult Create(int eventId)
    {
        var model = new GuestModel { EventId = eventId };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GuestModel guestModel)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.GetUserAsync(User);
            guestModel.OrganizerId = user.Id;
            
            context.Add(guestModel);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List), new { eventId = guestModel.EventId });
        }
        return View(guestModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int guestId)
    {
        var guest = await context.Guests.FindAsync(guestId);
        
        if (guest == null) return NotFound();
        
        return View(guest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GuestModel guestModel)
    {
        if (!ModelState.IsValid) return View(guestModel);

        var guest = await context.Guests.FindAsync(guestModel.GuestId);
        if (guest == null) return NotFound();

        guest.EventId = guestModel.EventId;
        guest.OrganizerId = guestModel.OrganizerId;
        guest.FirstName = guestModel.FirstName;
        guest.LastName = guestModel.LastName;
        guest.Email = guestModel.Email;
        guest.PhoneNumber = guestModel.PhoneNumber;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GuestExists(guestModel.GuestId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToAction(nameof(List), new { eventId = guestModel.EventId });
    }

    private bool GuestExists(int id)
    {
        return context.Guests.Any(e => e.GuestId == id);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var guestModel = await context.Guests.FindAsync(id);

        if (guestModel == null)
        {
            return NotFound();
        }

        return View(guestModel);
    }

    [HttpPost]
    [ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int guestId)
    {
        var guestModel = await context.Guests.FindAsync(guestId);

        if (guestModel == null)
        {
            return NotFound();
        }

        context.Guests.Remove(guestModel);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(List), new { eventId = guestModel.EventId });
    }
}

}
