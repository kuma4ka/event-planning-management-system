using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models.UserRelated;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }
    
    [Required]
    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }
    
    public bool Changed { get; set; } = false;
}