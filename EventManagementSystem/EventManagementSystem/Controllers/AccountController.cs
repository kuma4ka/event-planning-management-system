using EventManagementSystem.Data;
using EventManagementSystem.Models.Authentication;
using EventManagementSystem.Models.UserRelated;
using EventManagementSystem.Utils;
using EventManagementSystem.Utils.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Controllers
{
    public class AccountController : Controller, IAccount
    {
        private readonly UserManager<AccountModel> _userManager;
        private readonly SignInManager<AccountModel> _signInManager;
        private readonly ApplicationDbContext _context;
        public delegate Task UserEventHandler(string userId, string message);
        public event UserEventHandler UserEventOccurred;
        
        public AccountController(UserManager<AccountModel> userManager, SignInManager<AccountModel> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            UserEventOccurred += async (userId, message) =>
            {
                TempData["UserNotification"] = message;
            };
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var existingUser = await FindBy.FindByEmailAsync(model.Email, _userManager);
                
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "This email is already in use!");
                return View(model);
            }
                
            var user = new AccountModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            if (model.Password is null) return View(model);
            
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await UserEventOccurred?.Invoke(user.Id, "The user is successfully registered!")!;
                return RedirectToAction("Index", "Home");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);
            
            var user = await FindBy.FindByEmailAsync(model.Email, _userManager);
            
            if (model.Password != null && user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _signInManager.SignInAsync(user, model.RememberMe);
                await UserEventOccurred?.Invoke(user.Id, "Authorisation was successful!")!;
                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError(string.Empty, "Error during authorisation!");
            
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = await _userManager.GetUserAsync(User);
            
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            model.Changed = true;
        
            return View(model);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            return View(model);
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid) return View(model);

            if (user == null) return View(model);
            
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                model.Changed = true;
                return View(model);
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var events = _context.Events.Where(e => e.OrganizerId == user.Id).ToList();

            return View(events);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewGuests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var guests = _context.Guests.Where(g => g.OrganizerId == user.Id).ToList();

            return View(guests);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewVenues()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var venue = _context.Venues.Where(g => g.OrganizerId == user.Id).ToList();

            return View(venue);
        }
    }
}
