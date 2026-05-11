using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Csv;

public static partial class CsvOutput
{
    public static async Task<Uri> WriteMultiStageQualificationResults(
        Guid raceId,
        IReadOnlyCollection<MultiStageQualificationResult> results,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_qualification_results_{TimeProvider.System.GetLocalNow():HHmmss}.csv";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);
        var culture = CultureInfo.CurrentCulture;
        var config = new CsvConfiguration(culture)
        {
            Delimiter = culture.TextInfo.ListSeparator,
            HasHeaderRecord = true,
        };
        var stagesCount = results.Max(x => x.Results.Count);

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, config);

        // Header
        csv.WriteField("Position");
        csv.WriteField("Participant");
        for (var i = 1; i <= stagesCount; i++)
            csv.WriteField($"Q{i}");
        csv.WriteField("Best lap");
        csv.WriteField("Agg lap");
        await csv.NextRecordAsync();

        // Rows
        var position = 1;
        foreach (var result in results)
        {
            csv.WriteField(position++);
            csv.WriteField(result.Participant);
            for (var i = 1; i <= stagesCount; i++)
                csv.WriteField((result.StageLapTime(i).IsMax ? new LapTime(120_000) : result.StageLapTime(i)).ToString());
            csv.WriteField((result.BestLapTime.IsMax ? new LapTime(120_000) : result.BestLapTime).ToString());
            csv.WriteField((result.AggregatedLapTime.IsMax ? new LapTime(120_000) : result.AggregatedLapTime).ToString());
            await csv.NextRecordAsync();
        }

        return new Uri(filePath);
    }
}
