using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Agent.Dto;

namespace DaytonaRaceway.Adapter;

public sealed class QualificationResultsAdapter : IDisposable
{
    private readonly ApiAgent _agent;

    public QualificationResultsAdapter(IAgentSettings agentSettings)
    {
        _agent = new ApiAgent(agentSettings);
    }

    public void Dispose() => _agent.Dispose();

    public async Task<IReadOnlyCollection<QualificationResult>> GetQualificationStageResults(
        Guid raceId,
        int stageId,
        CancellationToken cancellationToken)
    {
        var qualificationStages = await GetQualificationStages(raceId, cancellationToken);
        var stage = qualificationStages
                ?.FirstOrDefault(s => s.Id == stageId
                    || s.Label?.Equals($"Qualy {stageId}", StringComparison.OrdinalIgnoreCase) is true)
            ?? throw new InvalidOperationException($"Qualification stage {stageId} not found in race {raceId}.");

        return await GetAndTransformQualificationStageResults(raceId, stage, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MultiStageQualificationResult>> GetMultiStageQualificationResults(
        Guid raceId,
        CancellationToken cancellationToken)
    {
        var qualificationStages = await GetQualificationStages(raceId, cancellationToken);
        if (qualificationStages is null || qualificationStages.Count == 0)
        {
            throw new InvalidOperationException($"Race {raceId} does not contain any qualification stages.");
        }

        var stageResults = (await Task.WhenAll(
                qualificationStages.Select(stage =>
                    GetAndTransformQualificationStageResults(raceId, stage, cancellationToken))))
            .ToArray();

        return stageResults
            .SelectMany(stage => stage.Select(r => r.Participant))
            .Distinct()
            .Select(participant =>
            {
                var resultsPerStage = stageResults
                    .Select((stage, index) =>
                    (
                        Stage: index + 1,
                        BestLapTime: stage.FirstOrDefault(r => r.Participant == participant)?.BestLapTime ?? LapTime.Max
                    ))
                    .ToArray();

                return new MultiStageQualificationResult(participant, resultsPerStage);
            })
            .ToArray();
    }

    private async Task<IReadOnlyCollection<RaceStageDto>?> GetQualificationStages(
        Guid raceId,
        CancellationToken cancellationToken)
    {
        var stages = await _agent.GetStages(raceId, cancellationToken);
        return stages.Stages?.Where(s => s.Type == StageType.Qualification).ToArray();
    }

    private async Task<IReadOnlyCollection<QualificationResult>> GetAndTransformQualificationStageResults(
        Guid raceId,
        RaceStageDto stage,
        CancellationToken cancellationToken)
    {
        if (stage.Heats is null || stage.Heats.All(h => h.Status != Status.Finished))
        {
            throw new InvalidOperationException($"Stage {stage.Id} has not finished heats.");
        }

        var finishedHeats = stage.Heats.Where(h => h.Status == Status.Finished).ToArray();
        var heatResultResponses = await Task.WhenAll(finishedHeats.Where(h => h.Id.HasValue)
            .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken)));
        var heatResults = heatResultResponses.SelectMany(results => results).ToArray();

        var orderedResults = heatResults
            .Select(r => r.BestLapTimeRaw == 0 ? r with { BestLapTimeRaw = int.MaxValue } : r) // Consider absence of result as a maximum time for further sorting
            .OrderBy(r => r.BestLapTimeRaw)
            .ToArray();

        return orderedResults
            .Select((result, i) => new QualificationResult(
                Position: i + 1,
                result.Participant ?? $"Participant {result.ParticipantId}",
                result.Kart,
                finishedHeats.First(h => h.Id == result.HeatId).Label,
                LapTime.From(result.BestLapTimeRaw),
                Gap: i == 0 || result.BestLapTimeRaw == int.MaxValue
                    ? Gap.FromMs(0)
                    : Gap.FromMs(result.BestLapTimeRaw - orderedResults[0].BestLapTimeRaw),
                Interval: i == 0 || result.BestLapTimeRaw == int.MaxValue
                    ? Gap.FromMs(0)
                    : Gap.FromMs(result.BestLapTimeRaw - orderedResults[i - 1].BestLapTimeRaw),
                result.BestLapTimeRaw == int.MaxValue
                    ? 0d
                    : Math.Round(result.BestLapTimeRaw * 100d / orderedResults[0].BestLapTimeRaw, 3),
                result.CompletedLaps))
            .ToArray();
    }
}
