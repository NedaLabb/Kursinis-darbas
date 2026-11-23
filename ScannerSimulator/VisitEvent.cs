namespace ScannerSimulator;

public class VisitEvent
{
    public Guid EventId { get; set; }
    public string CompanyId { get; set; } = "";
    public string ClubId { get; set; } = "";
    public string VisitorId { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}