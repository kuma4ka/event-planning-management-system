using EventManagementSystem.Models;
using EventManagementSystem.Models.UserRelated;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Utils.Interfaces;

public interface IAdmin
{
    public Task<IActionResult> AdminPanel();
    public Task<IActionResult> AddUser(AddUserModel newUser);
    public Task<IActionResult> EditUser(AccountModel model);
    public Task<IActionResult> DeleteUser(string userId);
    public Task<IActionResult> AddEvent(EventModel newEvent);
    public Task<IActionResult> EditEvent(EventModel eventModel);
    public Task<IActionResult> DeleteEvent(int eventId);
}