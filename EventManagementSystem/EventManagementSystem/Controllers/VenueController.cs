using EventManagementSystem.Data;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementSystem.Controllers
{
    public class VenueController(ApplicationDbContext context) : Controller
    {
        // GET: Venue/Select/{eventId}
        public async Task<IActionResult> Select(int eventId)
        {
            var eventEntity = await context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var model = new VenueSelectionModel
            {
                EventId = eventId,
                OrganizerId = eventEntity.OrganizerId,
                Venues = await context.Venues.ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Select(VenueSelectionModel model)
        {
            if (ModelState.IsValid)
            {
                var selectedVenue = await context.Venues.FindAsync(model.VenueId);
                var eventEntity = await context.Events.FindAsync(model.EventId);
                if (eventEntity == null || eventEntity.OrganizerId != model.OrganizerId)
                {
                    return NotFound();
                }

                selectedVenue.OrganizerId = model.OrganizerId;
                selectedVenue.EventId = model.EventId;
                eventEntity.VenueId = model.VenueId;
                
                await context.SaveChangesAsync();
                return RedirectToAction("Details", "Event", new { id = model.EventId });
            }

            model.Venues = await context.Venues.ToListAsync();
            return View(model);
        }
    }
}