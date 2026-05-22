namespace DaytonaRaceway.Adapter.Models;

public record RaceResult(
    int Position,
    string Participant,
    string? Kart,
    string? Heat,
    int CompletedLaps,
    LapTime TotalTime,
    Gap Gap,
    Gap Interval,
    LapTime BestLapTime,
    int Points,
    int ExtraPoints,
    int Penalty);
