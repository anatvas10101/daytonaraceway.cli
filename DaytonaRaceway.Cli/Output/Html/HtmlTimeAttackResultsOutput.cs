using System.Text;
using System.Web;
using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Cli.Commands;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    public static async Task<Uri> WriteTimeAttackResults(
        IReadOnlyCollection<TimeAttackResult> results,
        IReadOnlyCollection<TimeAttackKartResult> kartResults,
        TimeAttackResultsCommand.Settings settings,
        CancellationToken cancellationToken)
    {
        var filename = $"{settings.RaceId}_time_attack_results_{TimeProvider.System.GetLocalNow():HHmmss}.html";
        var filePath = Path.Combine(AppContext.BaseDirectory, filename);

        var distinctOrderedKarts = results
            .SelectMany(result => result.SessionResults)
            .Select(result => result.KartNumber)
            .Distinct()
            .OrderBy(k => k)
            .ToArray();

        var html = BuildHtmlHead("Time Attack Results")
            .AppendLine("<body>")
            .AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">")
            .AppendLine("<thead><tr>")
            .Append("<th>Pos</th>")
            .Append("<th>Participant</th>")
            .Append("<th>Avg Time</th>");

        foreach (var kart in distinctOrderedKarts)
        {
            html.Append($"<th>{kart}</th>");
        }

        html.AppendLine("</tr></thead>")
            .AppendLine("<tbody>");

        foreach (var result in results)
        {
            html.AppendLine("<tr>")
                .Append($"<td>{result.Position}</td>")
                .Append($"<td>{HttpUtility.HtmlEncode(result.Participant)}</td>")
                .Append($"<td><b>{HttpUtility.HtmlEncode(result.AverageLapTime.ToString())}</b></td>");

            foreach (var kart in distinctOrderedKarts)
            {
                var resultsByKart = result.SessionResults.Where(sr => sr.KartNumber == kart).ToArray();
                if (resultsByKart == null || resultsByKart.Length == 0)
                {
                    html.Append("<td></td>");
                }
                else if (resultsByKart.Length == 1)
                {
                    var formattedOutput = resultsByKart[0] switch
                    {
                        { IsBestLap: true, IsFirstHeat: true } => "<td><mark><u>{0}</u></mark></td>",
                        { IsBestLap: true } => "<td><mark>{0}</mark></td>",
                        { IsFirstHeat: true } => "<td><u>{0}</u></td>",
                        _ => "<td>{0}</td>",
                    };
                    html.Append(string.Format(formattedOutput, resultsByKart[0].BestLapTime));
                }
                else
                {
                    var fmt = new List<string>();
                    foreach (var r in resultsByKart)
                    {
                        var formattedOutput = r switch
                        {
                            { IsBestLap: true, IsFirstHeat: true } => "<mark><u>{0}</u></mark>",
                            { IsBestLap: true } => "<mark>{0}</mark>",
                            { IsFirstHeat: true } => "<u>{0}</u>",
                            _ => "{0}",
                        };

                        fmt.Add(string.Format(formattedOutput, r.BestLapTime));
                    }

                    html.Append($"<td>{string.Join("<br/>", fmt)}</td>");
                }
            }

            html.AppendLine("</tr>");
        }

        if (settings.IncludeKartResults)
        {
            var top3 = kartResults
                .OrderBy(result => result.TopNAvgLapTime.RawMs)
                .Take(3)
                .Select(x => x.KartNumber)
                .ToHashSet();

            html.AppendLine("<tr>")
                .Append($"<td colspan=3><b>TOP-{settings.N} Avg Time</b></td>");

            foreach (var kart in distinctOrderedKarts)
            {
                var kartResult = kartResults.FirstOrDefault(sr => sr.KartNumber == kart);
                if (kartResult == null)
                {
                    html.Append("<td></td>");
                    continue;
                }

                var formattedOutput = top3.Contains(kartResult.KartNumber)
                    ? "<td><b>{0}</b></td>"
                    : "<td>{0}</td>";
                html.Append(string.Format(formattedOutput, kartResult.TopNAvgLapTime));
            }

            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody></table>")
            .AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
