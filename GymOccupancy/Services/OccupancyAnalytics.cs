using GymOccupancy.Models;

namespace GymOccupancy.Services;

public class OccupancyAnalytics
{
    public double? CalculateAverage(List<VisitEvent> history, DateTimeOffset from, DateTimeOffset to){
        
        if (from >= to)
        {
            throw new ArgumentException("Laikotarpio pradžia turi būti ankstesnė už pabaigą.");
        }

        var events = history.OrderBy(visit => visit.OccurredAt).ToList();

        if (events.Count < 2)
        {
            return null;
        }

        if (from < events[0].OccurredAt)
        {
            from = events[0].OccurredAt;
        }

        if (to > events[events.Count - 1].OccurredAt)
        {
            to = events[events.Count - 1].OccurredAt;
        }

        if (from >= to)
        {
            return null;
        }

        var peopleCount = 0;
        double peopleCountTimesMinutes = 0;
        var previousTime = from;

        foreach (var visit in events)
        {
            if (visit.OccurredAt >= to)
            {
                break;
            }

            if (visit.OccurredAt > from)
            {
                var minutes = (visit.OccurredAt - previousTime).TotalMinutes;
                peopleCountTimesMinutes += peopleCount * minutes;
                previousTime = visit.OccurredAt;
            }

            if (visit.Type == "Entry")
            {
                peopleCount++;
            }
            else if (visit.Type == "Exit")
            {
                peopleCount--;
            }
        }

        var remainingMinutes = (to - previousTime).TotalMinutes;
        peopleCountTimesMinutes += peopleCount * remainingMinutes;

        var totalMinutes = (to - from).TotalMinutes;

        return peopleCountTimesMinutes / totalMinutes;
    }

    public List<HourlyOccupancy> CalculateHourly(List<VisitEvent> history, DateTimeOffset from, DateTimeOffset to){
        
        if (from >= to)
        {
            throw new ArgumentException("Laikotarpio pradžia turi būti ankstesnė už pabaigą.");
        }

        var results = new List<HourlyOccupancy>();
        var events = history.OrderBy(visit => visit.OccurredAt).ToList();

        if (events.Count < 2)
        {
            return results;
        }

        if (from < events[0].OccurredAt)
        {
            from = events[0].OccurredAt;
        }

        if (to > events[events.Count - 1].OccurredAt)
        {
            to = events[events.Count - 1].OccurredAt;
        }

        var countFrom = from.ToUniversalTime();
        var endTime = to.ToUniversalTime();

        while (countFrom < endTime)
        {
            var hourStart = new DateTimeOffset(
                countFrom.Year,
                countFrom.Month,
                countFrom.Day,
                countFrom.Hour,
                0, 0,
                TimeSpan.Zero);

            var countUntil = hourStart.AddHours(1);

            if (countUntil > endTime)
            {
                countUntil = endTime;
            }

            var average = CalculateAverage(events, countFrom, countUntil);

            if (average.HasValue)
            {
                results.Add(new HourlyOccupancy
                {
                    HourStart = hourStart,
                    AveragePeople = average.Value,
                    MinutesWithData = (countUntil - countFrom).TotalMinutes
                });
            }

            countFrom = countUntil;
        }

        return results;
    }

    public List<WeeklyOccupancy> CalculateWeeklyAverages(List<VisitEvent> history, DateTimeOffset from, DateTimeOffset to){
    var hours = CalculateHourly(history, from, to);
    var results = new List<WeeklyOccupancy>();
    var lithuaniaTime = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vilnius");

    foreach (var hour in hours)
    {
        var localTime = TimeZoneInfo.ConvertTime(
            hour.HourStart, lithuaniaTime);

        WeeklyOccupancy? result = null;

        foreach (var item in results)
        {
            if (item.Day == localTime.DayOfWeek &&
                item.Hour == localTime.Hour)
            {
                result = item;
                break;
            }
        }

        if (result == null)
        {
            result = new WeeklyOccupancy
            {
                Day = localTime.DayOfWeek,
                Hour = localTime.Hour
            };

            results.Add(result);
        }

        var totalMinutes = result.MinutesWithData + hour.MinutesWithData;

        result.AveragePeople = (result.AveragePeople * result.MinutesWithData + hour.AveragePeople * hour.MinutesWithData) / totalMinutes;

        result.MinutesWithData = totalMinutes;
    }
    return results;
    }
}