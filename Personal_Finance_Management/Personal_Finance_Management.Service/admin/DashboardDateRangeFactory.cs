namespace Personal_Finance_Management.Service.Admin;

internal static class DashboardTimeframe
{
    public const string Day = "day";
    public const string Month = "month";
    public const string Year = "year";

    public static string Normalize(string? timeframe)
    {
        return string.IsNullOrWhiteSpace(timeframe)
            ? Day
            : timeframe.Trim().ToLowerInvariant();
    }
}

internal static class DashboardDateRangeFactory
{
    public static DashboardDateRange Create(string timeframe, DateTimeOffset now)
    {
        var periodStart = GetPeriodStart(timeframe, now);
        var periodEnd = GetPeriodEnd(timeframe, periodStart);
        var trendRange = BuildTrendRange(timeframe, now);

        return new DashboardDateRange(periodStart, periodEnd, trendRange);
    }

    private static DateTimeOffset GetPeriodStart(string timeframe, DateTimeOffset now)
    {
        var utcNow = now.UtcDateTime;
        return timeframe switch
        {
            DashboardTimeframe.Day => new DateTimeOffset(utcNow.Date, TimeSpan.Zero),
            DashboardTimeframe.Month => new DateTimeOffset(new DateTime(utcNow.Year, utcNow.Month, 1), TimeSpan.Zero),
            DashboardTimeframe.Year => new DateTimeOffset(new DateTime(utcNow.Year, 1, 1), TimeSpan.Zero),
            _ => new DateTimeOffset(utcNow.Date, TimeSpan.Zero)
        };
    }

    private static DateTimeOffset GetPeriodEnd(string timeframe, DateTimeOffset periodStart)
    {
        return timeframe switch
        {
            DashboardTimeframe.Day => periodStart.AddDays(1),
            DashboardTimeframe.Month => periodStart.AddMonths(1),
            DashboardTimeframe.Year => periodStart.AddYears(1),
            _ => periodStart.AddDays(1)
        };
    }

    private static TrendRange BuildTrendRange(string timeframe, DateTimeOffset now)
    {
        var utcNow = now.UtcDateTime;
        return timeframe switch
        {
            DashboardTimeframe.Day => BuildDailyRange(utcNow),
            DashboardTimeframe.Month => BuildMonthlyRange(utcNow),
            DashboardTimeframe.Year => BuildYearlyRange(utcNow),
            _ => BuildDailyRange(utcNow)
        };
    }

    private static TrendRange BuildDailyRange(DateTime utcNow)
    {
        var startDate = utcNow.Date.AddDays(-6);
        var buckets = new List<TrendBucket>();

        for (var i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var start = new DateTimeOffset(date, TimeSpan.Zero);
            buckets.Add(new TrendBucket(start, GetDayLabel(start.DayOfWeek)));
        }

        var rangeStart = buckets.First().Start;
        var rangeEnd = buckets.Last().Start.AddDays(1);
        return new TrendRange(rangeStart, rangeEnd, buckets);
    }

    private static TrendRange BuildMonthlyRange(DateTime utcNow)
    {
        var monthStart = new DateTime(utcNow.Year, utcNow.Month, 1).AddMonths(-11);
        var buckets = new List<TrendBucket>();

        for (var i = 0; i < 12; i++)
        {
            var date = monthStart.AddMonths(i);
            var start = new DateTimeOffset(date, TimeSpan.Zero);
            buckets.Add(new TrendBucket(start, start.ToString("yyyy-MM")));
        }

        var rangeStart = buckets.First().Start;
        var rangeEnd = buckets.Last().Start.AddMonths(1);
        return new TrendRange(rangeStart, rangeEnd, buckets);
    }

    private static TrendRange BuildYearlyRange(DateTime utcNow)
    {
        var yearStart = new DateTime(utcNow.Year - 4, 1, 1);
        var buckets = new List<TrendBucket>();

        for (var i = 0; i < 5; i++)
        {
            var date = new DateTime(yearStart.Year + i, 1, 1);
            var start = new DateTimeOffset(date, TimeSpan.Zero);
            buckets.Add(new TrendBucket(start, date.Year.ToString()));
        }

        var rangeStart = buckets.First().Start;
        var rangeEnd = buckets.Last().Start.AddYears(1);
        return new TrendRange(rangeStart, rangeEnd, buckets);
    }

    private static string GetDayLabel(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "T2",
            DayOfWeek.Tuesday => "T3",
            DayOfWeek.Wednesday => "T4",
            DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6",
            DayOfWeek.Saturday => "T7",
            DayOfWeek.Sunday => "CN",
            _ => dayOfWeek.ToString()
        };
    }
}

internal sealed record DashboardDateRange(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    TrendRange Trend);

internal sealed record TrendRange(
    DateTimeOffset Start,
    DateTimeOffset End,
    IReadOnlyList<TrendBucket> Buckets);

internal sealed record TrendBucket(DateTimeOffset Start, string Label);
