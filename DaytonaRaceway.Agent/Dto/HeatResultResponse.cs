using System.Text.Json.Serialization;

namespace DaytonaRaceway.Agent.Dto;

public sealed record HeatResultResponse(
    [property: JsonPropertyName("id")] int HeatRunId,
    [property: JsonPropertyName("pos")] int Position,
    [property: JsonPropertyName("grid_position")] int StartingPosition,
    [property: JsonPropertyName("race_participant_id")] int ParticipantId,
    [property: JsonPropertyName("race_participant_name")] string? Participant,
    [property: JsonPropertyName("kart")] string? Kart,
    [property: JsonPropertyName("total_laps")] int CompletedLaps,
    [property: JsonPropertyName("total_time")] string TotalTime,
    [property: JsonPropertyName("best_lap_time")] string BestLapTime,
    [property: JsonPropertyName("best_lap_time_raw")] int BestLapTimeRaw,
    [property: JsonPropertyName("gap")] string Gap,
    [property: JsonPropertyName("diff")] string Interval,
    [property: JsonPropertyName("consistency_lap")] string Consistency,
    [property: JsonPropertyName("avg_lap")] string AvgLapTime,
    [property: JsonIgnore] int HeatId)
{
    [JsonIgnore]
    public int EndToEndStagePosition { get; init; }
}
