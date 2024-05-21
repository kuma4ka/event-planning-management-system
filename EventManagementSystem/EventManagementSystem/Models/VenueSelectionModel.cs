using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models
{
    public class VenueSelectionModel
    {
        [Required(ErrorMessage = "EventId is required")]
        public int EventId { get; init; }
        
        [Required(ErrorMessage = "VenueId is required")]
        public int VenueId { get; init; }
        
        [Required(ErrorMessage = "OrganizerId is required")]
        public string? OrganizerId { get; init; }
        
        public List<VenueModel> Venues { get; set; } = new();
    }
}