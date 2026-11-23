namespace GymOccupancy.Models;

public class HourlyOccupancy
{
    public DateTimeOffset HourStart { get; set; }
    public double AveragePeople { get; set; }
    public double MinutesWithData { get; set; }
}