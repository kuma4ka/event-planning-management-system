using EventManagementSystem.Data;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventManagementSystem.Models.UserRelated;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Controllers
{
    public class EventController(UserManager<AccountModel> userManager, ApplicationDbContext context)
        : Controller
    {
        public async Task<IActionResult> List()
        {
            var user = await userManager.GetUserAsync(User);
            var events = await context.Events
                                       .Where(e => e.OrganizerId == user.Id)
                                       .ToListAsync();
            return View(events);
        }

        public IActionResult Create()
        {
            var userId = userManager.GetUserId(User);
            ViewData["OrganizerId"] = userId;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventModel eventModel)
        {
            if (!ModelState.IsValid) return View(eventModel);

            var user = await userManager.GetUserAsync(User);
    
            eventModel.RegisteredAt = DateTime.Now;
            eventModel.OrganizerId = user.Id;
    
            context.Add(eventModel);
            await context.SaveChangesAsync();
    
            TempData["SuccessMessage"] = "Event created successfully!";
    
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var eventModel = await context.Events.FindAsync(id);
            
            if (eventModel == null || eventModel.OrganizerId != user.Id)
            {
                return NotFound();
            }
            
            return View(eventModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventModel eventModel)
        {
            if (!ModelState.IsValid) return View(eventModel);

            var existingEvent = await context.Events.FindAsync(eventModel.EventId);
            if (existingEvent == null) return NotFound();

            existingEvent.EventName = eventModel.EventName;
            existingEvent.EventDate = eventModel.EventDate;
            existingEvent.Type = eventModel.Type;
            existingEvent.EventDescription = eventModel.EventDescription;
            existingEvent.VenueId = eventModel.VenueId;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var eventModel = await context.Events.FindAsync(id);

            if (eventModel == null) return RedirectToAction(nameof(List));
            
            var venue = context.Venues.FirstOrDefault(v => v.EventId == eventModel.EventId);

            if (venue != null)
            {
                venue.OrganizerId = null;
                venue.EventId = null;
            }
                
            var guests = await context.Guests
                .Where(g => g.EventId == eventModel.EventId)
                .ToListAsync();;

            foreach (var guest in guests)
            {
                context.Guests.Remove(guest);
            }
                
            context.Events.Remove(eventModel);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await context.Events.FindAsync(id);

            if (eventModel == null) return RedirectToAction(nameof(List));

            var venue = context.Venues.FirstOrDefault(v => v.EventId == eventModel.EventId);

            if (venue != null)
            {
                venue.OrganizerId = null;
                venue.EventId = null;
            }

            var guests = await context.Guests
                .Where(g => g.EventId == eventModel.EventId)
                .ToListAsync();

            foreach (var guest in guests)
            {
                context.Guests.Remove(guest);
            }

            context.Events.Remove(eventModel);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(List));
        }

        public IActionResult Details(int id)
        {
            var eventModel = context.Events.FirstOrDefault(e => e.EventId == id);
            if (eventModel == null)
            {
                return NotFound();
            }

            var guests = context.Guests.Where(g => g.EventId == id).ToList();
            var venue = context.Venues.FirstOrDefault(v => v.VenueId == eventModel.VenueId);

            var viewModel = new EventDetailsViewModel
            {
                Event = eventModel,
                Guests = guests,
                Venue = venue
            };

            return View(viewModel);
        }
    }
}
