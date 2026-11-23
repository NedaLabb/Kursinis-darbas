using System.Data;
using GymOccupancy.Data;
using GymOccupancy.Models;
using Microsoft.EntityFrameworkCore;

namespace GymOccupancy.Services;

public class EventProcessor
{
    private readonly GymDbContext db;

    public EventProcessor(GymDbContext db)
    {
        this.db = db;
    }

    public async Task<string> ProcessAsync(string companyId, VisitEvent visitEvent){
        using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var clubs = await db.Clubs.ToListAsync();
        var service = new OccupancyService(clubs);

        var clubExists = false;

        foreach (var club in clubs)
        {
            if (club.ClubId == visitEvent.ClubId && club.CompanyId == companyId)
            {
                clubExists = true;
                break;
            }
        }

        if (!clubExists)
        {
            return "Klubas nerastas arba nepriklauso šiai įmonei.";
        }

        var alreadyProcessed = await db.VisitEvents.AnyAsync(visit => visit.EventId == visitEvent.EventId);

        if (alreadyProcessed)
        {
            return "Šis įvykis jau yra apdorotas.";
        }

        var savedEvents = await db.VisitEvents.Where(visit => visit.ClubId == visitEvent.ClubId && visit.CompanyId == companyId).ToListAsync();

        service.RestoreHistory(savedEvents);

        var result = service.ProcessEvent(companyId, visitEvent);

        if (result == "Įvykis priimtas.")
        {
            db.VisitEvents.Add(visitEvent);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        return result;
    }
}