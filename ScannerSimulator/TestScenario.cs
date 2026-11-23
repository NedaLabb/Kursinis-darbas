namespace ScannerSimulator;

public class TestScenario
{
    private VisitEvent? firstEvent;

    public void Run(EventSender sender)
    {
        var visitors = new List<string>();
        firstEvent = null;

        for (int i = 1; i <= 30; i++)
        {
            var visitorId = Guid.NewGuid().ToString();
            visitors.Add(visitorId);

            var entry = new VisitEvent
            {
                EventId = Guid.NewGuid(),
                CompanyId = "I001",
                ClubId = "K002",
                VisitorId = visitorId,
                Type = "Entry",
                OccurredAt = DateTimeOffset.Now
            };

            if (firstEvent == null)
            {
                firstEvent = entry;
            }

            sender.Send(entry);
        }

        Console.WriteLine("Išsiųsta 30 įėjimų.");

        for (int i = 0; i < 5; i++)
        {
            var exit = new VisitEvent
            {
                EventId = Guid.NewGuid(),
                CompanyId = "I001",
                ClubId = "K002",
                VisitorId = visitors[i],
                Type = "Exit",
                OccurredAt = DateTimeOffset.Now
            };

            sender.Send(exit);
        }

        Console.WriteLine("Išsiųsti 5 išėjimai.");
    }

    public void RepeatEvent(EventSender sender)
    {
        if (firstEvent == null)
        {
            Console.WriteLine("Pirmiausia paleiskite testinį scenarijų.");
            return;
        }

        sender.Send(firstEvent);
        Console.WriteLine("Pakartotinai išsiųstas tas pats įvykis.");
    }
}