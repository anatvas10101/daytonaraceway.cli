using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Agent.Dto;

namespace DaytonaRaceway.Adapter;

public sealed class RaceResultsAdapter : IDisposable
{
    private readonly ApiAgent _agent;

    public RaceResultsAdapter(IAgentSettings agentSettings)
    {
        _agent = new ApiAgent(agentSettings);
    }

    public void Dispose() => _agent.Dispose();

    public async Task<(string? Stage, IReadOnlyCollection<RaceResult> Results)> GetRaceStageResults(
        Guid raceId,
        int stageId,
        FinalStageHeatsOrdering finalStageHeatsOrdering,
        CancellationToken cancellationToken)
    {
        var raceStages = await GetRaceStages(raceId, cancellationToken);
        var stage = raceStages
                ?.FirstOrDefault(s => s.Id == stageId
                    || s.Label?.Equals($"Race {stageId}", StringComparison.OrdinalIgnoreCase) is true)
            ?? throw new InvalidOperationException($"Race stage {stageId} not found in race {raceId}.");

        return (stage.Label,
            await GetAndTransformRaceStageResults(raceId, stage, finalStageHeatsOrdering, cancellationToken));
    }

    private async Task<IReadOnlyCollection<RaceStageDto>?> GetRaceStages(
        Guid raceId,
        CancellationToken cancellationToken)
    {
        var stages = await _agent.GetStages(raceId, cancellationToken);
        return stages.Stages?.Where(s => s.Type == StageType.Race).ToArray();
    }

    private async Task<IReadOnlyCollection<RaceResult>> GetAndTransformRaceStageResults(
        Guid raceId,
        RaceStageDto stage,
        FinalStageHeatsOrdering finalStageHeatsOrdering,
        CancellationToken cancellationToken)
    {
        if (stage.Heats is null || stage.Heats.All(h => h.Status != Status.Finished))
        {
            throw new InvalidOperationException($"Stage {stage.Id} has not finished heats.");
        }

        var finishedHeats = stage.Heats!.Where(h => h.Status == Status.Finished).ToArray();
        var heatResultResponses = await Task.WhenAll(finishedHeats.Where(h => h.Id.HasValue)
            .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken)));
        var heatResults = heatResultResponses.SelectMany(results => results).ToArray();

        var orderedResults =
            (stage.IsFinalStage && finalStageHeatsOrdering == FinalStageHeatsOrdering.Desc
                ? heatResults.OrderByDescending(r => r.HeatId)
                : heatResults.OrderBy(r => r.HeatId))
            .ThenBy(r => r.Position)
            .ToArray();

        var pointsScale = stage.IsFinalStage
            ? PointsDistribution.CreateFinalScale(orderedResults.Length)
            : PointsDistribution.CreateStageScale(orderedResults.GroupBy(r => r.HeatId).Max(x => x.Count()));

        var participantPerHeatWithBestLapExtraPoint = orderedResults
            .Select(r => r.BestLapTimeRaw == 0 ? r with { BestLapTimeRaw = int.MaxValue } : r)
            .GroupBy(r => r.HeatId)
            .ToDictionary(h => h.Key, x => x.MinBy(r => r.BestLapTimeRaw)!.ParticipantId);

        return orderedResults
            .Select((result, endToEndPosition) => new RaceResult(
                result.Position,
                result.Participant ?? $"Participant {result.ParticipantId}",
                result.Kart,
                finishedHeats.First(h => h.Id == result.HeatId).Label,
                result.CompletedLaps,
                LapTime.Parse(result.TotalTime),
                Gap: Gap.ParseGap(result.Gap),
                Interval: Gap.ParseGap(result.Interval),
                BestLapTime: new LapTime(result.BestLapTimeRaw),
                Points: pointsScale[stage.IsFinalStage ? endToEndPosition + 1 : result.Position],
                ExtraPoints: participantPerHeatWithBestLapExtraPoint[result.HeatId] == result.ParticipantId ? 1 : 0))
            .ToArray();
    }
}
