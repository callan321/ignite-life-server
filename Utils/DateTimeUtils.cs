namespace Server.Utils;

public static class DateTimeUtils
{
    /// <summary>
    /// Checks if two datetime ranges overlap.
    /// </summary>
    public static bool IsOverlapping(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
    {
        return aStart < bEnd && aEnd > bStart;
    }

    /// <summary>
    /// Ensures that start is strictly before end (required for DateTime).
    /// </summary>
    public static bool IsValidRange(DateTime start, DateTime end)
    {
        return start < end;
    }

    /// <summary>
    /// Ensures that start is strictly before end (nullable-safe for DateTime).
    /// Returns true if either is null (assumes partial update — safe).
    /// </summary>
    public static bool IsValidRange(DateTime? start, DateTime? end)
    {
        return !start.HasValue || !end.HasValue || start.Value < end.Value;
    }

    /// <summary>
    /// Ensures that open time is before close time (used for TimeOnly).
    /// </summary>
    public static bool IsValidRange(TimeOnly open, TimeOnly close)
    {
        return open < close;
    }

    /// <summary>
    /// Nullable-safe version for TimeOnly, safe for PATCH ops.
    /// Returns true if either is null (assumes no check needed).
    /// </summary>
    public static bool IsValidRange(TimeOnly? open, TimeOnly? close)
    {
        return !open.HasValue || !close.HasValue || open.Value < close.Value;
    }
}
