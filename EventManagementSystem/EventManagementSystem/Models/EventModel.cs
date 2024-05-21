using System.ComponentModel.DataAnnotations;
using EventManagementSystem.Utils.Enums;

namespace EventManagementSystem.Models
{
    public class EventModel
    {
        [Required]
        public int EventId { get; init; }
        
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        public string? OrganizerId { get; set; }

        public int VenueId { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        public string? EventName { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public EventType Type { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 10)]
        public string? EventDescription { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime RegisteredAt { get; set; }
    }
}