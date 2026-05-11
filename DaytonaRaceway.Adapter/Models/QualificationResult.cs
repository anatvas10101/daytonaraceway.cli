namespace DaytonaRaceway.Adapter.Models;

public record QualificationResult(
    int Position,
    string Participant,
    string? Kart,
    string? Heat,
    LapTime BestLapTime,
    Gap Gap,
    Gap Interval,
    double Percent,
    int CompletedLaps);
