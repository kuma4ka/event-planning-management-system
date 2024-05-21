using EventManagementSystem.Models.UserRelated;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventManagementSystem.Utils;

public static class FindBy
{
    public static async Task<AccountModel?> FindByEmailAsync(string? email, UserManager<AccountModel> userManager)
    {
        return await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}