using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Agent.Dto;
using Microsoft.Extensions.Logging;

namespace DaytonaRaceway.Adapter;

public sealed class TotalResultsAdapter : IDisposable
{
    private readonly ApiAgent _agent;
    private readonly ILogger<TotalResultsAdapter> _logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger<TotalResultsAdapter>();

    public TotalResultsAdapter(IAgentSettings agentSettings)
    {
        _agent = new ApiAgent(agentSettings);
    }

    public void Dispose() => _agent.Dispose();
    
    public async Task<object> GetResults(Guid raceId, string finalHeatsOrder, CancellationToken cancellationToken)
    {
        var stages = await _agent.GetStages(raceId, cancellationToken);

        var raceStages = stages.Stages?.Where(s => s.Type == StageType.Race && !s.IsFinalStage).ToArray();
        var finalStage = stages.Stages?.SingleOrDefault(s => s.Type == StageType.Race && s.IsFinalStage);

        const int awardedForBestQualyResultCount = 3; // TODO:: const
        _logger.LogInformation("Determining best {N} qualification results", awardedForBestQualyResultCount);
        var topNQualyResults = (await TakeTopTotalQualificationBests(
            raceId,
            stages.Stages ?? [],
            awardedForBestQualyResultCount,
            cancellationToken))
                .Select((x, i) => (x.Key, new LapTime(x.Value), i + 1, awardedForBestQualyResultCount - i))
                .ToDictionary(x => x.Key, x => x);
        foreach (var (_, (participant, result, _, points)) in topNQualyResults)
        {
            _logger.LogInformation("{Participant} => {TotalLapTime}. Bonus {Points} points", participant, result, points);
        }

        // TODO:: Bonus point for fastest lap for each heat
        var raceStageResults = (await Task.WhenAll(raceStages
                !.SelectMany(s => s.Heats ?? [])
                .Where(h => h.Id.HasValue)
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(heatResults => heatResults)
            .ToArray();
        
        var stagePointsScale = PointsDistribution.CreateStageScale(raceStageResults.GroupBy(r => r.HeatId).Max(x => x.Count()));

        var races = raceStageResults
            .Join(
                raceStages!.SelectMany(x => x.Heats?.Select(h => new
                {
                    HeatId = h.Id,
                    Heat = h.Label ?? $"Heat {h.Index}",
                    Stage = x.Label ?? $"Stage {x.Index}"
                }) ?? []),
                rr => rr.HeatId,
                rs => rs.HeatId,
                (resp, obj) => new
                {
                    Participant = resp.Participant,
                    resp.Position,
                    obj.Heat,
                    obj.Stage,
                    Points = stagePointsScale[resp.Position]
                })
            .GroupBy(x => x.Participant!)
            .ToDictionary(
                x => x.Key,
                x => x.ToDictionary(v => (v.Stage, v.Heat), v => (v.Position, v.Points)));

        var finalStageResults = (await Task.WhenAll(finalStage!.Heats!
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(heatResults => heatResults)
            .ToArray();
        
        var finalPointsScale = PointsDistribution.CreateFinalScale(finalStageResults.Length);
        
        var finals = finalStageResults
            .Join(
                finalStage!.Heats?.Select(h => new { HeatId = h.Id, Heat = h.Label ?? $"Heat {h.Index}" }) ?? [],
                r => r.HeatId,
                h => h.HeatId,
                (r, h) => new
                {
                    Result = r,
                    Heat = h.Heat,
                    HeatOrder = finalHeatsOrder.IndexOf(h.Heat.Replace("Heat", string.Empty).Trim())
                })
            .OrderBy(r => r.HeatOrder)
            .ThenBy(r => r.Result.Position)
            .Select((result, endToEndPosition) => new{
                result.Result.Participant,
                result.Result.Position,
                result.Heat,
                Points = finalPointsScale[endToEndPosition + 1]
            })
            .OrderByDescending(x => x.Points)
            .ToArray();
        
        var totals = races.Join(
            finals,
            r => r.Key,
            f => f.Participant,
            (r, f) =>
            {
                r.Value.Add(("Final", f.Heat), (f.Position, f.Points));
                if (topNQualyResults.TryGetValue(r.Key, out var result))
                {
                    var (_, _, position, points) = result;
                    r.Value.Add(("Qualy", "Qualy"), (position, points));
                }
                else
                {
                    r.Value.Add(("Qualy", "Qualy"), (0, 0));
                }
                return new
                {
                    Participant = r.Key,
                    Results = r.Value,
                };
            })
            .OrderByDescending(x => x.Results.Sum(v => v.Value.Points))
            .ToDictionary(
                x => x.Participant,
                x => x.Results);
        
        foreach (var (participant, results) in totals)
        {
            Console.Write($"{participant,-24}");
            Console.Write("\t");
            foreach (var (heat, result) in results)
            {
                Console.Write($"{heat.Stage}|{heat.Heat} => {result.Position,-3}{result.Points,-3}");
            }
            Console.Write("\t");
            Console.Write(results.Values.Sum(x => x.Points));
            Console.WriteLine();
        }
        // TODO:: Participant | Heat | Position | Points for all non-final races
        // TODO:: Participant | Position | Points for final race

        return new object();
    }

    private async Task<Dictionary<string, int>> TakeTopTotalQualificationBests(
        Guid raceId,
        IReadOnlyCollection<RaceStageDto> stages,
        int count,
        CancellationToken cancellationToken)
    {
        return (await Task.WhenAll(stages
                .Where(s => s.Type == StageType.Qualification)
                .SelectMany(s => s.Heats ?? [])
                .Where(h => h.Id.HasValue)
                .Select(heat => _agent.GetHeatResults(raceId, heat.Id!.Value, cancellationToken))))
            .SelectMany(heatResults => heatResults)
            .Select(result => result.BestLapTimeRaw == 0 ? result with { BestLapTimeRaw = int.MaxValue } : result)
            .GroupBy(
                result => result.Participant,
                result => result)
            .ToDictionary(
                resultsGroup => resultsGroup.Key!,
                resultsGroup => resultsGroup.Any(r => r.BestLapTimeRaw == int.MaxValue)
                    ? int.MaxValue
                    : resultsGroup.Sum(x => x.BestLapTimeRaw))
            .OrderBy(participantTotalResult => participantTotalResult.Value)
            .Take(count)
            .ToDictionary(
                participantResult => participantResult.Key,
                participantResult => participantResult.Value);
    }
}
