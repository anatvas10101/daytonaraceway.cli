using System.Text.Json.Serialization;

namespace DaytonaRaceway.Agent.Dto;

public sealed record SessionManagementResponse(
    [property: JsonPropertyName("race_name")] string? EventName,
    [property: JsonPropertyName("race_stages")] List<StageResponseDto>? Stages);

public sealed record StageResponseDto(
    [property: JsonPropertyName("uuid")] Guid Uuid,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("num")] int Index,
    [property: JsonPropertyName("stage_type")] StageType Type,
    [property: JsonPropertyName("status")] Status Status,
    [property: JsonPropertyName("heats")] List<StageHeatResponseDto>? Heats);

public sealed record StageHeatResponseDto(
    [property: JsonPropertyName("race_heat_id")] int? Id,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("num")] int Index,
    [property: JsonPropertyName("status")] Status Status);
