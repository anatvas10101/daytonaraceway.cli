namespace DaytonaRaceway.Adapter.Models;

public record TotalResults(
    string Participant,
    int? QualificationExtraPoints,
    Dictionary<string, TotalResultItem> ResultsPerStage);

public record TotalResultItem(
    string Stage,
    string Heat,
    string Participant,
    int Position,
    int BasePoints,
    int ExtraPoints,
    int PenaltyPoints)
{
    public int TotalPoints => BasePoints + ExtraPoints - PenaltyPoints;
};
