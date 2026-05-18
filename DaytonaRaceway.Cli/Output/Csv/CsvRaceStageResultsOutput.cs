using CsvHelper.Configuration;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Csv;

public static partial class CsvOutput
{
    public static async Task<Uri> WriteRaceStageResults(
        Guid raceId,
        string stage,
        IReadOnlyCollection<RaceResult> results,
        CancellationToken cancellationToken)
    {
        return await WriteCsvFileWithClassMapping<RaceResult, RaceResultCsvMap>(
            $"{raceId}_race_stage_{stage}_results_{TimeProvider.System.GetLocalNow():HHmmss}.csv",
            results,
            cancellationToken);
    }

    private sealed class RaceResultCsvMap : ClassMap<RaceResult>
    {
        public RaceResultCsvMap()
        {
            Map(r => r.Heat)
                .Name("Heat")
                .Convert(args => args.Value.Heat?.Replace("Heat", string.Empty).Trim());
            Map(r => r.Position).Name("Position");
            Map(r => r.Participant).Name("Participant");
            Map(r => r.Kart).Name("Kart");
            Map(r => r.CompletedLaps).Name("Laps");
            
            Map(r => r.Gap)
                .Name("Gap")
                .Convert(args => args.Value.Gap.ToString());
            Map(r => r.Interval)
                .Name("Interval")
                .Convert(args => args.Value.Interval.ToString());
            Map(r => r.BestLapTime)
                .Name("Best lap")
                .Convert(args => args.Value.BestLapTime.ToString());
            Map(r => r.Points).Name("Points")
                .Convert(args => (args.Value.Points + args.Value.ExtraPoints - args.Value.Penalty).ToString());
            Map(r => r.ExtraPoints).Name("Bonus");
            Map(r => r.Penalty).Name("Penalty");
        }
    }
}
