namespace DaytonaRaceway.Agent.Dto;

public sealed record RaceStagesResponse(
    string? EventName,
    List<RaceStageDto>? Stages);

public sealed record RaceStageDto(
    int Id,
    Guid Uuid,
    string? Label,
    int Index,
    StageType Type,
    Status Status,
    bool IsFinalStage,
    List<StageHeatDto>? Heats);

public sealed record StageHeatDto(
    int? Id,
    string? Label,
    int Index,
    Status Status);