using System.Text;
using System.Web;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    public static async Task<Uri> WriteMultiStageQualificationResults(
        Guid raceId,
        IReadOnlyCollection<MultiStageQualificationResult> results,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_qualification_results_{TimeProvider.System.GetLocalNow():HHmmss}.html";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);
        var stagesCount = results.Max(x => x.Results.Count);

        var html = BuildHtmlHead("Qualification Results")
            .AppendLine("<body>")
            .AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">")
            .AppendLine("<thead><tr>")
            .Append("<th>Pos</th>")
            .Append("<th>Participant</th>");

        for (var i = 1; i <= stagesCount; i++)
            html.Append($"<th>Q{i}</th>");

        html.Append("<th>Best lap</th>")
            .Append("<th>Agg lap</th>")
            .AppendLine("</tr></thead>")
            .AppendLine("<tbody>");

        var position = 1;
        foreach (var result in results)
        {
            html.AppendLine("<tr>")
                .Append($"<td>{position++}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Participant)}</td>");

            for (var i = 1; i <= stagesCount; i++)
                html.Append($"<td>{HttpUtility.HtmlEncode(result.StageLapTime(i).ToString())}</td>");

            html.Append($"<td>{HttpUtility.HtmlEncode(result.BestLapTime.ToString())}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.AggregatedLapTime.ToString())}</td>")
                .AppendLine("</tr>");
        }

        html.AppendLine("</tbody></table>")
            .AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
