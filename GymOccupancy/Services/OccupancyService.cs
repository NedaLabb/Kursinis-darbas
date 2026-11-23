using GymOccupancy.Models;

namespace GymOccupancy.Services;

public class OccupancyService
{
    private readonly List<Club> clubs = new()
    {
        new Club
        {
            ClubId = "K001",
            CompanyId = "I001",
            Name = "Centro klubas"
        },
        new Club
        {
            ClubId = "K002",
            CompanyId = "I001",
            Name = "Parko klubas"
        },
        new Club
        {
            ClubId = "K003",
            CompanyId = "I002",
            Name = "Šiaurės klubas"
        }
    };

    private readonly Dictionary<string, HashSet<string>> visitorsByClub = new();
    private readonly HashSet<Guid> processedEventIds = new();
    private readonly List<VisitEvent> history = new();

    public OccupancyService(List<Club>? databaseClubs = null)
    {
        if (databaseClubs != null)
        {
            clubs = databaseClubs;
        }

        foreach (var club in clubs)
        {
            visitorsByClub.Add(club.ClubId, new HashSet<string>());
        }
    }

    public int GetCurrentCount(string companyId, string clubId)
    {
        foreach (var club in clubs)
        {
            if (club.ClubId == clubId && club.CompanyId == companyId)
            {
                return visitorsByClub[clubId].Count;
            }
        }

        throw new InvalidOperationException("Klubas nerastas arba nepriklauso šiai įmonei.");
    }

    public string ProcessEvent(string companyId, VisitEvent visitEvent)
    {
        if (visitEvent.EventId == Guid.Empty || string.IsNullOrWhiteSpace(visitEvent.VisitorId) || visitEvent.OccurredAt == default)
        {
            return "Netinkami įvykio duomenys.";
        }

        if (visitEvent.CompanyId != companyId)
        {
            return "Įvykis priklauso kitai įmonei.";
        }

        GetCurrentCount(companyId, visitEvent.ClubId);

        if (processedEventIds.Contains(visitEvent.EventId))
        {
            return "Šis įvykis jau apdorotas.";
        }

        var visitors = visitorsByClub[visitEvent.ClubId];

        if (visitEvent.Type == "Entry")
        {
            if (visitors.Contains(visitEvent.VisitorId))
            {
                return "Lankytojas jau yra klube.";
            }

            visitors.Add(visitEvent.VisitorId);
        }
        else if (visitEvent.Type == "Exit")
        {
            if (!visitors.Contains(visitEvent.VisitorId))
            {
                return "Negalima užregistruoti išėjimo: lankytojo nėra klube.";
            }

            visitors.Remove(visitEvent.VisitorId);
        }
        else
        {
            return "Nežinomas įvykio tipas.";
        }

        processedEventIds.Add(visitEvent.EventId);
        history.Add(visitEvent);

        return "Įvykis priimtas.";
    }

    public void RestoreHistory(List<VisitEvent> savedEvents)
    {
        foreach (var visitEvent in savedEvents.OrderBy(visit => visit.OccurredAt))
        {
            var result = ProcessEvent(visitEvent.CompanyId, visitEvent);

            if (result != "Įvykis priimtas.")
            {
                throw new InvalidOperationException(
                    $"Nepavyko atkurti istorijos: {result}");
            }
        }
    }

    public List<VisitEvent> GetHistory(string companyId, string clubId){
        GetCurrentCount(companyId, clubId);

        var clubHistory = new List<VisitEvent>();

        foreach (var visitEvent in history)
        {
            if (visitEvent.CompanyId == companyId &&
                visitEvent.ClubId == clubId)
            {
                clubHistory.Add(new VisitEvent
                {
                    EventId = visitEvent.EventId,
                    CompanyId = visitEvent.CompanyId,
                    ClubId = visitEvent.ClubId,
                    VisitorId = visitEvent.VisitorId,
                    Type = visitEvent.Type,
                    OccurredAt = visitEvent.OccurredAt
                });
            }
        }

        return clubHistory;
    }
}