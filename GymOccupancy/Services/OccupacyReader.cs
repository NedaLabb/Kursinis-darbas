using GymOccupancy.Data;
using Microsoft.EntityFrameworkCore;
using GymOccupancy.Models;

namespace GymOccupancy.Services;

public class OccupancyReader
{
    private readonly GymDbContext db;

    public OccupancyReader(GymDbContext db)
    {
        this.db = db;
    }

    public async Task<int?> GetCurrentCountAsync(string clubId)
    {
        var clubExists = await db.Clubs.AnyAsync(club => club.ClubId == clubId);

        if (!clubExists)
        {
            return null;
        }

        var entries = await db.VisitEvents.CountAsync(visit => visit.ClubId == clubId && visit.Type == "Entry");

        var exits = await db.VisitEvents.CountAsync(visit => visit.ClubId == clubId && visit.Type == "Exit");

        return entries - exits;
    }

    public async Task<List<VisitEvent>> GetHistoryAsync(string clubId)
    {
        var history = await db.VisitEvents.Where(visit => visit.ClubId == clubId) .OrderBy(visit => visit.OccurredAt).ToListAsync();
        return history;
    }
}