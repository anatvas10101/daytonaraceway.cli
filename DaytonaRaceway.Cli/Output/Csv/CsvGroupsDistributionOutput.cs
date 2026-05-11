using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DaytonaRaceway.Cli.Output.Csv;

public static partial class CsvOutput
{
    public static async Task<Uri> WriteGroupsDistribution(
        Guid raceId,
        IDictionary<string, string> participantsGroupsDistribution,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_groups_distribution_{TimeProvider.System.GetLocalNow():HHmmss}.csv";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);
        var culture = CultureInfo.CurrentCulture;
        var config = new CsvConfiguration(culture)
        {
            Delimiter = culture.TextInfo.ListSeparator,
            HasHeaderRecord = true,
        };

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, config);

        // Header
        csv.WriteField("Group");
        csv.WriteField("Participant");
        await csv.NextRecordAsync();

        foreach (var (participant, group) in participantsGroupsDistribution)
        {
            csv.WriteField(group);
            csv.WriteField(participant);
            await csv.NextRecordAsync();
        }

        return new Uri(filePath);
    }
}
