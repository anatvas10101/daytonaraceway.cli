using System.Text.Json.Serialization;

namespace DaytonaRaceway.Agent.Dto;

public record HeatRunDetailsResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("driver_name")] string Participant,
    [property: JsonPropertyName("laps")] LapsData Laps,
    [property: JsonPropertyName("penalties")] PenaltyData[] Penalties);

public record LapsData(
    [property: JsonPropertyName("data")] LapData[] Data);

public record LapData(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("lap_number")] int LapNumber,
    [property: JsonPropertyName("lap_time")] string LapTime,
    [property: JsonPropertyName("is_invalid")] bool IsInvalid,
    [property: JsonPropertyName("is_suspicious")] bool IsSuspicious);

public record PenaltyData(
    [property: JsonPropertyName("penalty")] int Penalty);
