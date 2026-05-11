using System.Text.Json.Serialization;

namespace DaytonaRaceway.Agent.Dto;

public sealed record EventStageResponseDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("num")] int Index,
    [property: JsonPropertyName("is_final_race")] bool IsFinalStage);
