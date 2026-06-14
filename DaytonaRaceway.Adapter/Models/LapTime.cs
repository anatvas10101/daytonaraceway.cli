namespace DaytonaRaceway.Adapter.Models;

public record LapTime : IComparable<LapTime>, IComparable
{
    protected LapTime(int rawMs) => RawMs = rawMs;

    private static LapTime Empty => new(0);

    internal static LapTime Max => new(int.MaxValue);

    public int RawMs { get; }

    public bool IsMax => RawMs == int.MaxValue;
    
    public static LapTime From(int rawMs, int defaultLapTimeMs = 120_000)
        => rawMs != 0 ? new LapTime(rawMs) : new LapTime(defaultLapTimeMs);

    public static LapTime Parse(string source)
    {
        if (string.IsNullOrWhiteSpace(source) || source == "-")
            return Empty;

        var parts = source.Split('.');
        if (parts.Length > 2)
            throw new ArgumentException($"Invalid LapTime format: {source}");

        var msPart = parts.Length == 2 ? int.Parse(parts[1]) : 0;

        parts = parts[0].Split(':');
        return parts.Length switch
        {
            3 => new LapTime(int.Parse(parts[0]) * 3600 * 1000 + int.Parse(parts[1]) * 60000 + int.Parse(parts[2]) * 1000 + msPart),
            2 => new LapTime(int.Parse(parts[0]) * 60000 + int.Parse(parts[1]) * 1000 + msPart),
            1 => new LapTime(int.Parse(parts[0]) * 1000 + msPart),
            _ => throw new ArgumentException($"Invalid LapTime format: {source}")
        };
    }

    protected string FormatLapTime(Formatter? formatter = null)
    {
        if (RawMs is 0 or int.MaxValue)
        {
            return "-";
        }

        formatter ??= Formatter.Default;
        var seconds = RawMs / 1000;
        var minutes = seconds / 60;

        var minutesFormatted = $"{(formatter.IncludeMinutes ? $"{minutes}" : "")}";
        var secondsFormatted = $"{(formatter.IncludeMinutes ? $"{(seconds % 60):00}" : $"{seconds}")}";
        var millisecondsFormatted = $"{(RawMs % 1000):000}";

        return $"{minutesFormatted}{(formatter.IncludeMinutes ? formatter.MinutesSecondsSeparator : "")}{secondsFormatted}{formatter.MillisecondsSeparator}{millisecondsFormatted}";
    }
    
    public override string ToString() => FormatLapTime();

    public int CompareTo(LapTime? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return RawMs.CompareTo(other.RawMs);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is LapTime other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(LapTime)}");
    }
};

public record Formatter(
    bool IncludeMinutes = true,
    char MinutesSecondsSeparator = ':',
    char MillisecondsSeparator = '.')
{
    public static readonly Formatter Default = new();
    public static readonly Formatter Gap = new(false, ':', '.');
}
