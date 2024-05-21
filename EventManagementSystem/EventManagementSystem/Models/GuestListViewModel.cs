using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Models;

public class GuestListViewModel
{
    [Required]
    public EventModel? Event { get; set; }
    public List<GuestModel>? Guests { get; set; }
}