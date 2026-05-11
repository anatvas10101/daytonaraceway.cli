using System.Text;
using System.Web;
using DaytonaRaceway.Adapter.Models;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    public static async Task<Uri> WriteRaceStageResults(
        Guid raceId,
        string stage,
        IReadOnlyCollection<RaceResult> results,
        CancellationToken cancellationToken)
    {
        var filename = $"{raceId}_race_stage_{stage}_results_{TimeProvider.System.GetLocalNow():HHmmss}.html";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);

        var resultsByHeat = results.GroupBy(x => x.Heat);

        var html = BuildHtmlHead("Race Results")
            .AppendLine("<body>");
        foreach (var heatResults in resultsByHeat)
        {
            html.AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">")
                .AppendLine($"<thead><tr><th colspan=\"8\">{heatResults.Key}</th></tr></thead>")
                .AppendLine("<thead><tr>")
                .Append("<th>Pos</th>")
                .Append("<th>Participant</th>")
                .Append("<th>Kart</th>")
                .Append("<th>Laps</th>")
                .Append("<th>Gap</th>")
                .Append("<th>Interval</th>")
                .Append("<th>Best lap</th>")
                .Append("<th>Pts</th>")
                .AppendLine("</tr></thead>")
                .AppendLine("<tbody>");

            foreach (var result in heatResults)
            {
                html.AppendLine("<tr>")
                    .Append($"<td>{result.Position}</td>")
                    .Append($"<td>{HttpUtility.HtmlEncode(result.Participant)}</td>")
                    .Append($"<td>{HttpUtility.HtmlEncode(result.Kart)}</td>")
                    .Append($"<td>{result.CompletedLaps}</td>")
                    .Append($"<td>{HttpUtility.HtmlEncode(result.Gap.ToString())}</td>")
                    .Append($"<td>{HttpUtility.HtmlEncode(result.Interval.ToString())}</td>")
                    .Append($"<td>{HttpUtility.HtmlEncode(result.BestLapTime.ToString())}</td>")
                    .Append($"<td>{result.Points}</td>")
                    .AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
        }

        html.AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
