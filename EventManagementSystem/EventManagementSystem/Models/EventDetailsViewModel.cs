namespace EventManagementSystem.Models;

public class EventDetailsViewModel
{
    public EventModel Event { get; set; }
    public List<GuestModel> Guests { get; set; }
    public VenueModel Venue { get; set; }
}