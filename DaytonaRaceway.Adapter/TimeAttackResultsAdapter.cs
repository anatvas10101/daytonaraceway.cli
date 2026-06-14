using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Agent.Dto;

namespace DaytonaRaceway.Adapter;

public sealed class TimeAttackResultsAdapter : IDisposable
{
    private readonly ApiAgent _agent;

    public TimeAttackResultsAdapter(IAgentSettings agentSettings)
    {
        _agent = new ApiAgent(agentSettings);
    }

    public void Dispose() => _agent.Dispose();

    public async Task<(
        IReadOnlyCollection<TimeAttackResult> Results,
        IReadOnlyCollection<TimeAttackKartResult> KartResults)> GetResults(
        Guid raceId,
        int defaultLapTimeMs,
        int nForTopNKartResults,
        CancellationToken cancellationToken = default)
    {
        var stages = await _agent.GetStages(raceId, cancellationToken);
        var finishedHeats = stages.Stages
            !.SelectMany(stage => stage.Heats
                    ?.Where(heat => heat.Status == Status.Finished && heat.Id.HasValue)
                ?? [])
            .ToArray();
        var firstHeatId = finishedHeats.Min(heat => heat.Id)!.Value;

        var results = (await Task.WhenAll(finishedHeats
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(results => results)
            .GroupBy(result => result.Participant ?? $"Participant {result.ParticipantId}")
            .Select(resultsGroup => new
            {
                Participant = resultsGroup.Key,
                Results = resultsGroup.Select(result => (result.HeatId, Kart: result.Kart!, LapTime: LapTime.From(result.BestLapTimeRaw, defaultLapTimeMs)))
            })
            .Select(result => new
            {
                result.Participant,
                result.Results,
                Average = LapTime.From((int)Math.Round(result.Results.Average(r => r.LapTime.RawMs), MidpointRounding.AwayFromZero)),
                Best = LapTime.From(result.Results.Min(r => r.LapTime.RawMs)),
            })
            .OrderBy(result => result.Average)
            .Select((result, rank) => new TimeAttackResult(
                rank + 1,
                result.Participant,
                result.Average,
                result.Results
                    .Select(r => new TimeAttackSessionResult(
                        int.TryParse(r.Kart, out var kartNumber) ? kartNumber : 0,
                        r.LapTime,
                        IsFirstHeat: r.HeatId == firstHeatId,
                        IsBestLap: r.LapTime.Equals(result.Best))).ToArray()))
            .ToArray();

        var kartResults = results
            .SelectMany(result => result.SessionResults)
            .GroupBy(result => result.KartNumber)
            .Select(result => new TimeAttackKartResult(
                result.Key,
                TopNAvgLapTime: LapTime.From(
                    (int)Math.Round(
                        result
                            .OrderBy(r => r.BestLapTime.RawMs)
                            .Take(nForTopNKartResults)
                            .Average(r => r.BestLapTime.RawMs),
                        MidpointRounding.AwayFromZero))))
            .ToArray();

        return (results, kartResults);
    }
}
