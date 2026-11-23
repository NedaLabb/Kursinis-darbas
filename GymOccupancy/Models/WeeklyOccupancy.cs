namespace GymOccupancy.Models;

public class WeeklyOccupancy
{
    public DayOfWeek Day { get; set; }
    public int Hour { get; set; }
    public double AveragePeople { get; set; }
    public double MinutesWithData { get; set; }
}