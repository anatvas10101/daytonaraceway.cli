namespace DaytonaRaceway.Adapter.Models;

public record TimeAttackResult(
    int Position,
    string Participant,
    LapTime AverageLapTime,
    IReadOnlyCollection<TimeAttackSessionResult> SessionResults);

public record TimeAttackSessionResult(
    int KartNumber,
    LapTime BestLapTime,
    bool IsFirstHeat,
    bool IsBestLap);

public record TimeAttackKartResult(
    int KartNumber,
    LapTime TopNAvgLapTime);
