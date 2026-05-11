using System.Text;
using System.Web;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    public static async Task<Uri> WriteQualificationStageResults(
        Guid raceId,
        int stageId,
        IReadOnlyCollection<QualificationResult> results,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_qualification_stage_{stageId}_results_{TimeProvider.System.GetLocalNow():HHmmss}.html";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);

        var html = BuildHtmlHead("Qualification Results")
            .AppendLine("<body>")
            .AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">")
            .AppendLine("<thead><tr>")
            .Append("<th>Pos</th>")
            .Append("<th>Participant</th>")
            .Append("<th>Kart</th>")
            .Append("<th>Heat</th>")
            .Append("<th>Best lap</th>")
            .Append("<th>Gap</th>")
            .Append("<th>Interval</th>")
            .Append("<th>%</th>")
            .Append("<th>Laps</th>")
            .AppendLine("</tr></thead>")
            .AppendLine("<tbody>");

        foreach (var result in results)
        {
            html.AppendLine("<tr>")
                .Append($"<td>{result.Position}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Participant)}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Kart)}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Heat?.Replace("Heat", string.Empty).Trim())}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.BestLapTime.ToString())}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Gap.ToString())}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Interval.ToString())}</td>")
                .Append($"<td>{result.Percent:F3}</td>")
                .Append($"<td>{result.CompletedLaps}</td>")
                .AppendLine("</tr>");
        }

        html.AppendLine("</tbody></table>")
            .AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
