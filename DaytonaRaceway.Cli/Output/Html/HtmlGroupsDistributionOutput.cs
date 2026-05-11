using System.Text;
using System.Web;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    public static async Task<Uri> WriteGroupsDistribution(
        Guid raceId,
        IDictionary<string, string> participantsGroupsDistribution,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_groups_distribution_{TimeProvider.System.GetLocalNow():HHmmss}.html";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);

        var participantsByGroup = participantsGroupsDistribution
            .GroupBy(x => x.Value, x => x.Key)
            .OrderBy(x => x.Key.Length)
            .ThenBy(x => x.Key);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head><meta charset=\"utf-8\"><title>Groups distribution</title>");
        html.AppendLine("<style>body{display:flex;} table{margin: 0 10px;}</style></head>");
        html.AppendLine("<body>");
        foreach (var group in participantsByGroup)
        {
            html.AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">");
            html.AppendLine($"<thead><tr><th>Group {group.Key}</th></tr></thead>");
            html.AppendLine("<tbody>");
            foreach (var participant in group)
            {
                html.AppendLine($"<tr><td>{HttpUtility.HtmlEncode(participant)}</td></tr>");
            }
            html.AppendLine("</tbody></table>");
        }

        html.AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
