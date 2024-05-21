using System.Text.Json;
using EventManagementSystem.Data;
using EventManagementSystem.Models;
using EventManagementSystem.Models.Admin;
using EventManagementSystem.Models.UserRelated;
using EventManagementSystem.Utils;
using EventManagementSystem.Utils.Enums;
using EventManagementSystem.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole = EventManagementSystem.Utils.Enums.UserRole;

namespace EventManagementSystem.Controllers
{
    public class AdminController(UserManager<AccountModel> userManager, ApplicationDbContext context)
        : Controller, IAdmin
    {
        [HttpGet]
        public async Task<IActionResult> AdminPanel()
        {
            var user = await userManager.GetUserAsync(User);

            if (user is not { Role: UserRole.Admin })
            {
                return RedirectToAction("Index", "Home");
            }
            
            var allUsers = await userManager.Users.ToListAsync();
            
            var usersExceptCurrent = allUsers
                .Where(u => u.Id != user.Id).ToList();
            
            var events = await context.Events.ToListAsync();
            var guests = await context.Guests.ToListAsync();
            var venues = await context.Venues.ToListAsync();

            AdminPanelViewModel model = new() 
            {
                AccModels = usersExceptCurrent,
                EventModels = events,
                GuestModels = guests,
                VenueModels = venues,
            };
            
            return View(model);
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult AddUser()
        {
            var newUser = new AddUserModel();
            return View(newUser);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddUser(AddUserModel newUser)
        {
            if (!ModelState.IsValid) return View(newUser);
            
            var existingUser = await FindBy.FindByEmailAsync(newUser.Email, userManager);
            
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "This email address is already in use!");
                return View(newUser);
            }
            
            var user = new AccountModel
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                UserName = newUser.Email,
                Email = newUser.Email,
                PhoneNumber = newUser.Phone,
                Role = newUser.Role,
            };        
            
            var result = await userManager.CreateAsync(user, newUser.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("AdminPanel");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(newUser);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(AccountModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Role = model.Role;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("AdminPanel");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(AdminPanel));
                }
            }
            return RedirectToAction(nameof(AdminPanel));
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddEvent()
        {
            var model = new EventModel
            {
                EventDate = DateTime.Now,
                RegisteredAt = DateTime.Now, 
            };
            
            return View(model);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddEvent(EventModel newEvent)
        {
            if (!ModelState.IsValid) return View(newEvent);
            
            var existingEvent = await context.Events.FirstOrDefaultAsync(e => e.EventId == newEvent.EventId);
            
            if (existingEvent != null)
            {
                ModelState.AddModelError(string.Empty, "Event with such id already exists!");
                return View(newEvent);
            }

            var eventModel = new EventModel()
            {
                OrganizerId = newEvent.OrganizerId,
                EventName = newEvent.EventName,
                EventDate = newEvent.EventDate,
                Type = newEvent.Type,
                EventDescription = newEvent.EventDescription,
                RegisteredAt = newEvent.RegisteredAt,
            };
            
            await context.Events.AddAsync(eventModel);
            await context.SaveChangesAsync();

            return RedirectToAction("AdminPanel");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditEvent(int eventId)
        {
            var eventModel = await context.Events.FindAsync(eventId);
            
            if (eventModel is null)
            {
                return NotFound();
            }
            return View(eventModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventModel eventModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await context.Events.FindAsync(eventModel.EventId);
                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    existingEvent.OrganizerId = eventModel.OrganizerId;
                    existingEvent.EventName = eventModel.EventName;
                    existingEvent.EventDate = eventModel.EventDate;
                    existingEvent.Type = eventModel.Type;
                    existingEvent.EventDescription = eventModel.EventDescription;
                    existingEvent.RegisteredAt = eventModel.RegisteredAt;

                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(AdminPanel));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventModelExists(eventModel.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the event. Please try again.");
                }
            }
            return View(eventModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            var eventModel = await context.Events.FindAsync(eventId);
            
            if (eventModel != null)
            {
                context.Events.Remove(eventModel);
                await context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(AdminPanel));
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddGuest()
        {
            var model = new GuestModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGuest(GuestModel newGuest)
        {
            if (!ModelState.IsValid) return View(newGuest);

            var existingGuest = await context.Guests.FirstOrDefaultAsync(g => g.Email == newGuest.Email);
    
            if (existingGuest != null)
            {
                ModelState.AddModelError(string.Empty, "Guest with this email already exists!");
                return View(newGuest);
            }

            await context.Guests.AddAsync(newGuest);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditGuest(int guestId)
        {
            var guest = await context.Guests.FindAsync(guestId);
            if (guest == null) return NotFound();
            return View(guest);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGuest(GuestModel guestModel)
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

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGuest(int guestId)
        {
            var guest = await context.Guests.FindAsync(guestId);
            if (guest == null) return NotFound();

            context.Guests.Remove(guest);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel));
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddVenue()
        {
            var model = new VenueModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVenue(VenueModel newVenue)
        {
            if (!ModelState.IsValid) return View(newVenue);

            var existingVenue = await context.Venues.FirstOrDefaultAsync(v => v.VenueName == newVenue.VenueName && v.Address == newVenue.Address);
    
            if (existingVenue != null)
            {
                ModelState.AddModelError(string.Empty, "Venue with this name and address already exists!");
                return View(newVenue);
            }

            var currentEvent = await context.Events.FirstOrDefaultAsync(e => e.EventId == newVenue.EventId);
            
            if (currentEvent != null) 
                currentEvent.VenueId = newVenue.VenueId;

            await context.Venues.AddAsync(newVenue);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditVenue(int venueId)
        {
            var venue = await context.Venues.FindAsync(venueId);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue(VenueModel venueModel)
        {
            if (!ModelState.IsValid) return View(venueModel);

            var venue = await context.Venues.FindAsync(venueModel.VenueId);
            if (venue == null) return NotFound();

            venue.EventId = venueModel.EventId;
            venue.OrganizerId = venueModel.OrganizerId;
            venue.VenueName = venueModel.VenueName;
            venue.Address = venueModel.Address;
            venue.Capacity = venueModel.Capacity;
            venue.ImageUrl = venueModel.ImageUrl;
            venue.Description = venueModel.Description;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVenue(int venueId)
        {
            var venue = await context.Venues.FindAsync(venueId);
            if (venue == null) return NotFound();

            context.Venues.Remove(venue);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminPanel));
        }

        
        private bool EventModelExists(int id)
        {
            return context.Events.Any(e => e.EventId == id);
        }
    }
}
