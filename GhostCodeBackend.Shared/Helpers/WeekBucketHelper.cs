using System.Globalization;

namespace GhostCodeBackend.Shared.Helpers;

public static class WeekBucketHelper
{
    public static int GetWeekBucket(DateTime time)
    {
        var week = ISOWeek.GetWeekOfYear(time);
        return time.Year * 1000 + week;
    }
}