using System.Text.Json.Serialization;

namespace DaytonaRaceway.Agent.Dto;

public sealed record EventParticipantsResponse(
    [property: JsonPropertyName("race_name")] string? EventName,
    [property: JsonPropertyName("number_of_participants")] int ParticipantsCount,
    [property: JsonPropertyName("selected_participants")] List<EventParticipantResponse>? Participants);

public sealed record EventParticipantResponse(
    [property: JsonPropertyName("race_participant_id")] int Id,
    [property: JsonPropertyName("name")] string? Name);
