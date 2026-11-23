namespace ScannerSimulator;

public class HistoryScenario
{
    public void Run(EventSender sender)
    {
        var random = new Random(42);
        var events = new List<VisitEvent>();

        var start = new DateTimeOffset(2026, 11, 9, 0, 0, 0, TimeSpan.FromHours(2));

        for (int day = 0; day < 7; day++)
        {
            var visitorCount = random.Next(10, 31);

            for (int i = 0; i < visitorCount; i++)
            {
                var visitorId = Guid.NewGuid().ToString();
                var entryTime = start.AddDays(day).AddMinutes(random.Next(1440));

                var exitTime = entryTime.AddMinutes(random.Next(30, 121));

                events.Add(CreateEvent(visitorId, "Entry", entryTime));
                events.Add(CreateEvent(visitorId, "Exit", exitTime));
            }
        }

        foreach (var visit in events.OrderBy(visit => visit.OccurredAt))
        {
            sender.Send(visit);
        }

        Console.WriteLine($"Išsiųsta istorinių įvykių: {events.Count}");
    }

    private VisitEvent CreateEvent(
        string visitorId, string type, DateTimeOffset time)
    {
        return new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K001",
            VisitorId = visitorId,
            Type = type,
            OccurredAt = time
        };
    }
}