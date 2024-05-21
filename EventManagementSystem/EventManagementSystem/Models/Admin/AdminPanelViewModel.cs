using EventManagementSystem.Models.UserRelated;
using System.Collections.Generic;

namespace EventManagementSystem.Models.Admin
{
    public class AdminPanelViewModel
    {
        public List<AccountModel>? AccModels { get; set; }
        public List<EventModel>? EventModels { get; set; }
        public List<GuestModel>? GuestModels { get; set; }
        public List<VenueModel>? VenueModels { get; set; }
    }
}