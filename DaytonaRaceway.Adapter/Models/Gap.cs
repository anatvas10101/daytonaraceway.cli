namespace DaytonaRaceway.Adapter.Models;

public record Gap : LapTime
{
    private const string OverLapGapDetectorSingular = "lap";
    private const string OverLapGapDetectorPlural = "laps";

    private Gap(int ms = 0, int laps = 0) : base(ms)
    {
        Milliseconds = ms;
        Laps = laps;
    }

    public int Milliseconds { get; init; }

    public int Laps { get; init; }

    public static Gap FromMs(int milliseconds) => new(milliseconds);

    public static Gap FromLaps(int laps) => new(laps: laps);
    
    public static Gap ParseGap(string source)
    {
        if (string.IsNullOrWhiteSpace(source) || source == "-")
            return new Gap();

        if (source.Contains(OverLapGapDetectorSingular))
            return FromLaps(int.Parse(source.Replace(OverLapGapDetectorPlural, "").Replace(OverLapGapDetectorSingular, "").Trim()));

        return FromMs(Parse(source).RawMs);
    }

    public override string ToString()
        => this switch
        {
            { Laps: > 0 } => $"{Laps} {(Laps == 1 ? OverLapGapDetectorSingular : OverLapGapDetectorPlural)}",
            { Milliseconds: > 0 } => FormatLapTime(Formatter.Gap),
            _ => "-",
        };
}


// 3.592
// 8.669
// 9.203
// 30.896
// 1 laps
// 1 laps
// 1 laps
// 1 laps
// 2 laps
// -
// 0.442
// 2.414
// 6.556
// 10.596
// 14.428
// 45.186
// 55.050
// 1 laps
// 1 laps
// -
// 5.168
// 5.553
// 6.132
// 7.965
// 8.595
// 20.544
// 27.168
// 40.359
// 1:11.839

