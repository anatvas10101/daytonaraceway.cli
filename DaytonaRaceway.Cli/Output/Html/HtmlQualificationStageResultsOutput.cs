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
        
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head><meta charset=\"utf-8\"><title>Qualification Results</title></head><body>");
        html.AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">");
        html.AppendLine("<thead><tr>");
        html.Append("<th>Position</th>");
        html.Append("<th>Participant</th>");
        html.Append("<th>Kart</th>");
        html.Append("<th>Heat</th>");
        html.Append("<th>Best lap</th>");
        html.Append("<th>Gap</th>");
        html.Append("<th>Interval</th>");
        html.Append("<th>%</th>");
        html.Append("<th>Laps</th>");
        html.AppendLine("</tr></thead>");

        html.AppendLine("<tbody>");

        foreach (var result in results)
        {
            html.AppendLine("<tr>");
            html.Append($"<td>{result.Position}</td>");
            html.Append($"<td>{HttpUtility.HtmlEncode(result.Participant)}</td>");
            html.Append($"<td>{HttpUtility.HtmlEncode(result.Kart)}</td>");
            html.Append($"<td>{HttpUtility.HtmlEncode(result.Heat?.Replace("Heat", string.Empty).Trim())}</td>");
            html.Append($"<td>{HttpUtility.HtmlEncode(result.BestLapTime.ToString())}</td>");
            html.Append($"<td>{HttpUtility.HtmlEncode(result.Gap.ToString())}</td>");
            html.Append($"<td>{HttpUtility.HtmlEncode(result.Interval.ToString())}</td>");
            html.Append($"<td>{result.Percent:F3}</td>");
            html.Append($"<td>{result.CompletedLaps}</td>");
            html.AppendLine("</tr>");
        }
        html.AppendLine("</tbody></table>");
        html.AppendLine("</body></html>");

        await File.WriteAllTextAsync(filePath, html.ToString(), Encoding.UTF8, cancellationToken);
        return new Uri(filePath);
    }
}
