using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using UserRole = EventManagementSystem.Utils.Enums.UserRole;

namespace EventManagementSystem.Models.UserRelated
{
    public class AccountModel : IdentityUser
    {
        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [Display(Name = "Role")]
        public UserRole Role { get; set; } = 0;
    }
}