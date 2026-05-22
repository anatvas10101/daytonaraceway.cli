namespace DaytonaRaceway.Adapter.Models;

public record TotalResults(
    string Participant,
    int? QualificationExtraPoints,
    Dictionary<(string Stage, bool IsFinal), TotalResultItem> ResultsPerStage);

public record TotalResultItem(
    string Stage,
    bool IsFinalStage,
    int HeatId,
    string Heat,
    int ParticipantId,
    string Participant,
    int Position,
    int BasePoints,
    int ExtraPoints,
    int PenaltyPoints)
{
    public int TotalPoints => BasePoints + ExtraPoints - PenaltyPoints;

    public int ChampionshipPoints { get; init; }
};
