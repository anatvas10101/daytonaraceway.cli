using System.Text;
using System.Web;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    public static async Task<Uri> WriteTotalResults(
        Guid raceId,
        IReadOnlyCollection<TotalResults> results,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_results_{TimeProvider.System.GetLocalNow():HHmmss}.html";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);

        var stages = results.First().ResultsPerStage.Select(x => x.Key).ToArray();

        var html = BuildHtmlHead("Race Results")
            .AppendLine("<body>")
            .AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">")
            .AppendLine("<thead><tr>")
            .Append("<th rowspan=\"2\">Rank</th>")
            .Append("<th rowspan=\"2\">Participant</th>")
            .Append("<th rowspan=\"2\">Total pts</th>")
            .Append("<th rowspan=\"2\">Penalty pts</th>")
            .Append("<th rowspan=\"2\">Q Bonus</th>");

        foreach (var stage in stages)
        {
            html.Append($"<th colspan=\"5\">{stage}</th>");
        }

        html.AppendLine("</tr><tr>");
            
        foreach (var _ in stages)
        {
            html.Append("<th>Heat</th>")
                .Append("<th>Pos</th>")
                .Append("<th>Pts</th>")
                .Append("<th>Bonus</th>")
                .Append("<th>Penalty</th>");
        }
            
        html.AppendLine("</tr></thead>")
            .AppendLine("<tbody>");

        var rank = 1;
        foreach (var result in results)
        {
            html.AppendLine("<tr>")
                .Append($"<td>{rank++}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Participant)}</td>")
                .Append($"<td>{result.ResultsPerStage.Sum(x => x.Value.TotalPoints) + (result.QualificationExtraPoints ?? 0)}</td>")
                .Append($"<td>{result.ResultsPerStage.Sum(x => x.Value.PenaltyPoints)}</td>")
                .Append($"<td>{result.QualificationExtraPoints?.ToString()}</td>");

            foreach (var stage in stages)
            {
                html.Append($"<td>{HttpUtility.HtmlEncode(result.ResultsPerStage[stage].Heat.Replace("Heat", string.Empty).Trim())}</td>")
                    .Append($"<td>{result.ResultsPerStage[stage].Position}</td>")
                    .Append($"<td>{result.ResultsPerStage[stage].TotalPoints}</td>")
                    .Append($"<td>{result.ResultsPerStage[stage].ExtraPoints}</td>")
                    .Append($"<td>{result.ResultsPerStage[stage].PenaltyPoints}</td>");
            }

            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody></table>")
            .AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
