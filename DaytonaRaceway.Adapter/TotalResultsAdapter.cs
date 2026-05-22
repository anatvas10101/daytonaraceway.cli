using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Agent.Dto;

namespace DaytonaRaceway.Adapter;

public sealed class TotalResultsAdapter : IDisposable
{
    private const int MaxPointsForBestQualificationResult = 3;

    private readonly ApiAgent _agent;

    public TotalResultsAdapter(IAgentSettings agentSettings)
    {
        _agent = new ApiAgent(agentSettings);
    }

    public void Dispose() => _agent.Dispose();
    
    public async Task<IReadOnlyCollection<TotalResults>> GetResults(
        Guid raceId,
        FinalStageHeatsOrdering finalStageHeatsOrdering,
        CancellationToken cancellationToken)
    {
        var stages = await _agent.GetStages(raceId, cancellationToken);

        var topNQualificationResults = await GetTopTotalQualificationBestResults(
            raceId,
            stages.Stages?.Where(s => s.Type == StageType.Qualification).ToArray() ?? [],
            MaxPointsForBestQualificationResult,
            cancellationToken);

        var raceResults = await RaceStageResults(
            raceId,
            stages.Stages?.Where(s => s.Type == StageType.Race && !s.IsFinalStage).ToArray() ?? [],
            cancellationToken);

        var finalStageResults = await FinalStageResults(
            raceId,
            stages.Stages?.SingleOrDefault(s => s.Type == StageType.Race && s.IsFinalStage)!,
            finalStageHeatsOrdering,
            cancellationToken);

        return raceResults.Join(
                finalStageResults,
                rr => rr.Key,
                fr => fr.Key,
                (results, finalResults) =>
                    (ParticipantId: results.Key, Results: results.Value.Append(finalResults.Value).ToArray()))
            .Select(results => new TotalResults(
                results.Results.First().Participant,
                topNQualificationResults.GetValueOrDefault(results.ParticipantId, 0),
                results.Results.ToDictionary(
                    result => (result.Stage, result.IsFinalStage),
                    result => result)))
            .OrderByDescending(result =>
                result.ResultsPerStage.Sum(v => v.Value.TotalPoints) + (result.QualificationExtraPoints ?? 0))
            .ThenBy(result => result.ResultsPerStage.Sum(v => v.Value.PenaltyPoints))
            .ThenBy(result => result.ResultsPerStage.Last().Value.Position)
            .ToArray();
    }

    private async Task<Dictionary<ParticipantId, int>> GetTopTotalQualificationBestResults(
        Guid raceId,
        IReadOnlyCollection<RaceStageDto> stages,
        int count,
        CancellationToken cancellationToken)
    {
        return (await Task.WhenAll(stages
                .SelectMany(s => s.Heats ?? [])
                .Where(h => h.Id.HasValue)
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(heatResults => heatResults)
            .Select(result => result.BestLapTimeRaw == 0 ? result with { BestLapTimeRaw = int.MaxValue } : result)
            .GroupBy(
                result => new ParticipantId(result.ParticipantId),
                result => result)
            .ToDictionary(
                resultsGroup => resultsGroup.Key,
                resultsGroup => resultsGroup.Any(r => r.BestLapTimeRaw == int.MaxValue)
                    ? int.MaxValue
                    : resultsGroup.Sum(x => x.BestLapTimeRaw))
            .OrderBy(participantTotalResult => participantTotalResult.Value)
            .Take(count)
            .Select((participantResult, i) => (participantResult.Key, Points: MaxPointsForBestQualificationResult - i))
            .ToDictionary(
                participantResult => participantResult.Key,
                participantResult => participantResult.Points);
    }

    private async Task<Dictionary<ParticipantId, IReadOnlyCollection<TotalResultItem>>> RaceStageResults(
        Guid raceId,
        IReadOnlyCollection<RaceStageDto> stages,
        CancellationToken cancellationToken)
    {
        var heatsMap = stages
            .SelectMany(
                stage => stage.Heats ?? [],
                (stage, heat) => new
                {
                    Stage = stage.Label ?? $"{stage.Type} {stage.Index}",
                    HeatId = heat.Id ?? heat.Index,
                    Heat = heat.Label ?? $"Heat {heat.Index}"
                })
            .ToDictionary(
                item => item.HeatId,
                item => item);

        var raceStageResults = (await Task.WhenAll(stages
                .SelectMany(stage => stage.Heats ?? [])
                .Where(heat => heat.Id.HasValue)
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(heatResults => heatResults)
            .ToArray();

        var penalties = (await Task.WhenAll(raceStageResults
                .Select(result => _agent.GetHeatRunDetails(result.HeatRunId, cancellationToken))))
            .ToDictionary(
                heatRun => heatRun.Id,
                heatRun => heatRun.Penalties.Sum(p => p.Penalty));

        var participantPerHeatWithBestLapExtraPoint = raceStageResults
            .Select(result => result.BestLapTimeRaw == 0 ? result with { BestLapTimeRaw = int.MaxValue } : result)
            .GroupBy(result => result.HeatId)
            .ToDictionary(
                heatGroup => heatGroup.Key,
                heatGroup => heatGroup.MinBy(r => r.BestLapTimeRaw)!.ParticipantId);

        var maxHeatParticipantsCount = raceStageResults.GroupBy(r => r.HeatId).Max(x => x.Count());
        var stagePointsScale = PointsDistribution.CreateStageScale(maxHeatParticipantsCount);
        var championshipPointsScale = PointsDistribution.CreateChampionshipScale(maxHeatParticipantsCount);

        return raceStageResults
            .Select(result => new TotalResultItem(
                heatsMap[result.HeatId].Stage,
                IsFinalStage: false,
                result.HeatId,
                heatsMap[result.HeatId].Heat,
                result.ParticipantId,
                result.Participant ?? $"Participant {result.ParticipantId}",
                result.Position,
                stagePointsScale[result.Position],
                participantPerHeatWithBestLapExtraPoint[result.HeatId] == result.ParticipantId ? 1 : 0,
                penalties.GetValueOrDefault(result.HeatRunId, 0)))
            .GroupBy(result => result.HeatId)
            .ToDictionary(
                heatResults => heatResults.Key,
                heatResults => heatResults
                    .OrderByDescending(result => result.TotalPoints)
                    .ThenBy(result => result.PenaltyPoints)
                    .ThenBy(result => result.Position)
                    .Select((result, rank) => result with { ChampionshipPoints = championshipPointsScale[rank + 1] }))
            .SelectMany(heatResults => heatResults.Value)
            .GroupBy(result => result.ParticipantId)
            .ToDictionary(
                participantResults => new ParticipantId(participantResults.Key),
                IReadOnlyCollection<TotalResultItem> (participantResults) => participantResults.ToList());
    }

    private async Task<Dictionary<ParticipantId, TotalResultItem>> FinalStageResults(
        Guid raceId,
        RaceStageDto finalStage,
        FinalStageHeatsOrdering finalStageHeatsOrdering,
        CancellationToken cancellationToken)
    {
        var heatsMap = (finalStage.Heats ?? [])
            .ToDictionary(
                heat => heat.Id ?? heat.Index,
                heat => heat.Label ?? $"Heat {heat.Index}");

        var finalStageResults = (await Task.WhenAll((finalStage.Heats ?? [])
                .Where(heat => heat.Id.HasValue)
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(heatResults => heatResults)
            .ToArray();

        var penalties = (await Task.WhenAll(finalStageResults
                .Select(result => _agent.GetHeatRunDetails(result.HeatRunId, cancellationToken))))
            .ToDictionary(
                heatRun => heatRun.Id,
                heatRun => heatRun.Penalties.Sum(p => p.Penalty));

        var participantPerHeatWithBestLapExtraPoint = finalStageResults
            .Select(result => result.BestLapTimeRaw == 0 ? result with { BestLapTimeRaw = int.MaxValue } : result)
            .GroupBy(result => result.HeatId)
            .ToDictionary(
                heatGroup => heatGroup.Key,
                heatGroup => heatGroup.MinBy(r => r.BestLapTimeRaw)!.ParticipantId);

        var finalPointsScale = PointsDistribution.CreateFinalScale(finalStageResults.Length);

        return (finalStageHeatsOrdering == FinalStageHeatsOrdering.Desc
                ? finalStageResults.OrderByDescending(result => result.HeatId)
                : finalStageResults.OrderBy(result => result.HeatId))
            .ThenBy(result => result.Position)
            .Select((result, endToEndPosition) => result with { EndToEndStagePosition = endToEndPosition + 1 })
            .ToDictionary(
                result => new ParticipantId(result.ParticipantId),
                result => new TotalResultItem(
                    finalStage.Label ?? "Final",
                    IsFinalStage: true,
                    result.HeatId,
                    heatsMap[result.HeatId],
                    result.ParticipantId,
                    result.Participant ?? $"Participant {result.ParticipantId}",
                    result.Position,
                    finalPointsScale[result.EndToEndStagePosition],
                    participantPerHeatWithBestLapExtraPoint[result.HeatId] == result.ParticipantId ? 1 : 0,
                    penalties.GetValueOrDefault(result.HeatRunId, 0)));
    }
}
