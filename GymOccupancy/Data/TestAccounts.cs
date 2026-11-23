using GymOccupancy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymOccupancy.Data;

public static class TestAccounts
{
    public static async Task CreateAsync(GymDbContext db)
    {
        var passwordHasher = new PasswordHasher<UserAccount>();

        var accounts = new List<UserAccount>
        {
            new UserAccount
            {
                Username = "lankytojas",
                Role = "Visitor"
            },
            new UserAccount
            {
                Username = "admin1",
                Role = "Admin",
                CompanyId = "I001"
            },
            new UserAccount
            {
                Username = "admin2",
                Role = "Admin",
                CompanyId = "I002"
            }
        };

        foreach (var account in accounts)
        {
            var exists = await db.UserAccounts.AnyAsync(user => user.Username == account.Username);

            if (exists)
            {
                continue;
            }

            account.PasswordHash = passwordHasher.HashPassword(account, "testas123");

            db.UserAccounts.Add(account);
        }

        await db.SaveChangesAsync();
    }
}