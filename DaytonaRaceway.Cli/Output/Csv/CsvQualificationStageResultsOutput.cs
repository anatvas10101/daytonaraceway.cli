using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Csv;

public static partial class CsvOutput
{
    public static async Task<Uri> WriteQualificationStageResults(
        Guid raceId,
        int stageId,
        IReadOnlyCollection<QualificationResult> results,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_qualification_stage_{stageId}_results_{TimeProvider.System.GetLocalNow():HHmmss}.csv";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);
        var culture = CultureInfo.CurrentCulture;
        var config = new CsvConfiguration(culture)
        {
            Delimiter = culture.TextInfo.ListSeparator,
            HasHeaderRecord = true,
        };

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, config);
        csv.Context.RegisterClassMap<QualificationResultCsvMap>();
        await csv.WriteRecordsAsync(results, cancellationToken);

        return new Uri(filePath);
    }

    private sealed class QualificationResultCsvMap : ClassMap<QualificationResult>
    {
        public QualificationResultCsvMap()
        {
            Map(r => r.Position).Name("Position");
            Map(r => r.Participant).Name("Participant");
            Map(r => r.Kart).Name("Kart");
            Map(r => r.Heat)
                .Name("Heat")
                .Convert(args => args.Value.Heat?.Replace("Heat", string.Empty).Trim());
            Map(r => r.BestLapTime)
                .Name("Best lap")
                .Convert(args => args.Value.BestLapTime.ToString());
            Map(r => r.Gap)
                .Name("Gap")
                .Convert(args => args.Value.Gap.ToString());
            Map(r => r.Interval)
                .Name("Interval")
                .Convert(args => args.Value.Interval.ToString());
            Map(r => r.Percent).Name("%");
            Map(r => r.CompletedLaps).Name("Laps");
        }
    }
}
