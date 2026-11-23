namespace ScannerSimulator;

public class AutomaticSimulator
{
    public void Run(EventSender sender, string companyId, string clubId)
    {
        var visitors = new List<string>();
        var random = new Random();

        Console.WriteLine($"Generuojami klubo {clubId} įvykiai.");
        Console.WriteLine("Norėdami sustabdyti, paspauskite bet kurį klavišą.");

        while (!Console.KeyAvailable)
        {
            string visitorId;
            string type;

            if (visitors.Count == 0 || random.Next(2) == 0)
            {
                visitorId = Guid.NewGuid().ToString();
                type = "Entry";
                visitors.Add(visitorId);
            }
            else
            {
                visitorId = visitors[0];
                type = "Exit";
                visitors.RemoveAt(0);
            }

            var visitEvent = new VisitEvent
            {
                EventId = Guid.NewGuid(),
                CompanyId = companyId,
                ClubId = clubId,
                VisitorId = visitorId,
                Type = type,
                OccurredAt = DateTimeOffset.Now
            };

            sender.Send(visitEvent);

            Console.WriteLine($"{type}: {visitorId}");

            Thread.Sleep(3000);
        }

        Console.ReadKey(true);
        Console.WriteLine("Generavimas sustabdytas.");
    }
}