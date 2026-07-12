using System.Text.Json;
using DaytonaRaceway.Agent.Dto;

namespace DaytonaRaceway.Agent;

public sealed class ApiAgent : IDisposable
{
    public const string DefaultIpHost = "213.7.195.58"; // TODO:: 192.168.11.2 for local calls at Daytona track
    public const int DefaultPort = 8080; // TODO:: 80 for local calls at Daytona track

    private readonly HttpClient _client;

    public ApiAgent(IAgentSettings settings)
    {
        var uri = new UriBuilder(Uri.UriSchemeHttp, settings.SourceIp, settings.Port).ToString();
        _client = new HttpClient
        {
            BaseAddress = new Uri(uri),
            Timeout = settings.Timeout,
        };
    }

    public async Task<RaceStagesResponse> GetStages(Guid raceId, CancellationToken cancellationToken)
    {
        var stagesUrl = $"/rest-api/races/race/{raceId}";
        using var getEventStagesRequest = new HttpRequestMessage(HttpMethod.Get, stagesUrl);
        using var getEventStagesResponse = await _client.SendAsync(getEventStagesRequest, cancellationToken);
        var eventStagesRawResponse = await getEventStagesResponse.Content.ReadAsStringAsync(cancellationToken);

        using var jsonDocument = JsonDocument.Parse(eventStagesRawResponse);
        var stagesElement = jsonDocument.RootElement
            .GetProperty("data")
            .GetProperty("regulations")
            .GetProperty("data")
            .GetProperty("stages")
            .GetProperty("data");

        var eventStages = stagesElement.Deserialize<List<EventStageResponseDto>>()
            !.ToDictionary(m => (m.Index, m.Label), m => m);

        var sessionsUrl = $"/rest-api/races/race/{raceId}/session-management";
        using var getEventSessionsInfoRequest = new HttpRequestMessage(HttpMethod.Get, sessionsUrl);
        using var getEventSessionsInfoResponse = await _client.SendAsync(getEventSessionsInfoRequest, cancellationToken);
        var eventSessionsInfoRawResponse = await getEventSessionsInfoResponse.Content.ReadAsStringAsync(cancellationToken);

        var data = JsonSerializer.Deserialize<SessionManagementResponse>(eventSessionsInfoRawResponse)!;

        return new RaceStagesResponse(
            data.EventName,
            data.Stages
                ?.Select(s =>
                {
                    var stg = eventStages.GetValueOrDefault((s.Index, s.Label));
                    return new RaceStageDto(
                        stg?.Id ?? 0,
                        s.Uuid,
                        s.Label,
                        s.Index,
                        s.Type,
                        s.Status,
                        stg?.IsFinalStage is true,
                        s.Heats
                            ?.Where(h => !string.IsNullOrEmpty(h.GreenFlag))
                            .Select(h => new StageHeatDto(h.Id, h.Label, h.Index, h.Status))
                            .OrderBy(h => h.Index)
                            .ToList());
                })
                .OrderBy(x => x.Index)
                .ToList());
    }

    public async Task<List<HeatResultResponse>> GetHeatResults(
        Guid raceId,
        int heatId,
        CancellationToken cancellationToken)
    {
        var url = $"/rest-api/races/race/{raceId}/session-management/heat/{heatId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await _client.SendAsync(request, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        using var json = JsonDocument.Parse(rawContent);

        return json.RootElement
            .GetProperty("data")
            .GetProperty("race_heat_runs")
            .Deserialize<HeatResultResponse[]>()
            !.Select(r => r with { HeatId = heatId })
            .ToList();
    }

    public async Task<HeatRunDetailsResponse> GetHeatRunDetails(int heatRunId, CancellationToken cancellationToken)
    {
        var url = $"/rest-api/races/race/session-management/race-heat-run-info?race_heat_run_id={heatRunId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await _client.SendAsync(request, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        using var json = JsonDocument.Parse(rawContent);

        return json.RootElement
            .GetProperty("data")
            .Deserialize<HeatRunDetailsResponse>()!;
    }

    public async Task<EventParticipantsResponse> GetParticipants(Guid raceId, CancellationToken cancellationToken)
    {
        var url = $"/rest-api/races/race/{raceId}/participants";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await _client.SendAsync(request, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<EventParticipantsResponse>(rawContent)!;
    }

    public void Dispose() => _client.Dispose();
}