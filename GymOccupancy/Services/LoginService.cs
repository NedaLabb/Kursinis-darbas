using GymOccupancy.Data;
using GymOccupancy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymOccupancy.Services;

public class LoginService
{
    private readonly GymDbContext db;

    public LoginService(GymDbContext db)
    {
        this.db = db;
    }

    public async Task<UserAccount?> CheckLoginAsync(string username, string password){
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var account = await db.UserAccounts.SingleOrDefaultAsync(user => user.Username == username);

        if (account is null)
        {
            return null;
        }

        var passwordHasher = new PasswordHasher<UserAccount>();

        var result = passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return account;
    }
}