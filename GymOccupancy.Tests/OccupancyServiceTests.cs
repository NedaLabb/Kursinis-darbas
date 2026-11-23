using GymOccupancy.Models;
using GymOccupancy.Services;
using Xunit;

namespace GymOccupancy.Tests;

public class OccupancyServiceTests
{
    [Fact]
    public void TwoEntriesAndOneExit_LeaveOneVisitor()
    {
        var service = new OccupancyService();
        var startTime = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);

        var firstEntry = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K001",
            VisitorId = "L001",
            Type = "Entry",
            OccurredAt = startTime
        };

        var secondEntry = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K001",
            VisitorId = "L002",
            Type = "Entry",
            OccurredAt = startTime.AddMinutes(15)
        };

        var firstExit = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K001",
            VisitorId = "L001",
            Type = "Exit",
            OccurredAt = startTime.AddMinutes(30)
        };

        Assert.Equal("Įvykis priimtas.", service.ProcessEvent("I001", firstEntry));
        Assert.Equal("Įvykis priimtas.", service.ProcessEvent("I001", secondEntry));
        Assert.Equal(2, service.GetCurrentCount("I001", "K001"));

        Assert.Equal("Įvykis priimtas.", service.ProcessEvent("I001", firstExit));
        Assert.Equal(1, service.GetCurrentCount("I001", "K001"));
    }

    [Fact]
    public void SameEventTwice_DoesNotIncreaseCount()
    {
        var service = new OccupancyService();

        var entry = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K001",
            VisitorId = "L001",
            Type = "Entry",
            OccurredAt = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal("Įvykis priimtas.", service.ProcessEvent("I001", entry));

        Assert.Equal("Šis įvykis jau apdorotas.",service.ProcessEvent("I001", entry));

        Assert.Equal(1, service.GetCurrentCount("I001", "K001"));
    }

    [Fact]
    public void ExitWithoutEntry_IsRejected()
    {
        var service = new OccupancyService();

        var exit = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K001",
            VisitorId = "L001",
            Type = "Exit",
            OccurredAt = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal("Negalima užregistruoti išėjimo: lankytojo nėra klube.",service.ProcessEvent("I001", exit));

        Assert.Equal(0, service.GetCurrentCount("I001", "K001"));
    }

    [Fact]
    public void OtherCompanyClub_CannotBeReadOrChanged()
    {
        var service = new OccupancyService();

        var entry = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I002",
            ClubId = "K003",
            VisitorId = "L001",
            Type = "Entry",
            OccurredAt = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal("Įvykis priimtas.", service.ProcessEvent("I002", entry));

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.GetCurrentCount("I001", "K003");
        });

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.GetHistory("I001", "K003");
        });

        var foreignEntry = new VisitEvent
        {
            EventId = Guid.NewGuid(),
            CompanyId = "I001",
            ClubId = "K003",
            VisitorId = "L002",
            Type = "Entry",
            OccurredAt = entry.OccurredAt.AddMinutes(5)
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.ProcessEvent("I001", foreignEntry);
        });

        Assert.Equal(1, service.GetCurrentCount("I002", "K003"));

        Assert.Single(service.GetHistory("I002", "K003"));
    }
}