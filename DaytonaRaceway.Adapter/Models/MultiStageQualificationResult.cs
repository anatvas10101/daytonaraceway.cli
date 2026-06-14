namespace DaytonaRaceway.Adapter.Models;

public record MultiStageQualificationResult(
    string Participant,
    IReadOnlyCollection<(int Stage, LapTime BestLapTime)> Results)
{
    public LapTime StageLapTime(int stage) => Results.FirstOrDefault(result => result.Stage == stage).BestLapTime;

    public LapTime BestLapTime => Results.MinBy(x => x.BestLapTime.RawMs).BestLapTime;

    public LapTime AggregatedLapTime
        => Results.Any(x => x.BestLapTime == LapTime.Max)
            ? LapTime.Max
            : LapTime.From(Results.Sum(x => x.BestLapTime.RawMs));
}
