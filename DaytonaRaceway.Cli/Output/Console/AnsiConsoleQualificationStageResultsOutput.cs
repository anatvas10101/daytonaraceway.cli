using System.Text;
using DaytonaRaceway.Adapter.Models;
using Spectre.Console;

namespace DaytonaRaceway.Cli.Output.Console;

public static partial class AnsiConsoleOutput
{
    public static void PrintQualificationStageResults(
        Guid raceId,
        int stageId,
        IReadOnlyCollection<QualificationResult> results)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Qualification results[/] — race [yellow]{raceId}[/], stage [yellow]{stageId}[/]");
        AnsiConsole.WriteLine();

        var positionPad = 5;
        var participantPad = results.Max(x => x.Participant.Length) + 2;
        var kartPad = 6;
        var heatPad = 6;
        var blPad = Math.Max(results.Max(x => x.BestLapTime.ToString()?.Length) ?? -1, "Best lap".Length) + 2;
        var gapPad = Math.Max(results.Max(x => x.Gap.ToString()?.Length) ?? -1, "Gap".Length) + 2;
        var intervalPad = Math.Max(results.Max(x => x.Interval.ToString()?.Length) ?? -1, "Interval".Length) + 2;
        var percentagePad = 9;
        var lapsPad = 6;

        var headerString = new StringBuilder()
            .Append("Pos".PadRight(positionPad))
            .Append("Participant".PadRight(participantPad))
            .Append("Kart".PadRight(kartPad))
            .Append("Heat".PadRight(heatPad))
            .Append("Best lap".PadRight(blPad))
            .Append("Gap".PadRight(gapPad))
            .Append("Interval".PadRight(intervalPad))
            .Append("%".PadRight(percentagePad))
            .Append("Laps".PadRight(lapsPad))
            .ToString();

        AnsiConsole.MarkupLine($"[bold]{headerString}[/]");

        foreach (var result in results)
        {
            var rowString = new StringBuilder()
                .Append(result.Position.ToString().PadRight(positionPad))
                .Append(result.Participant.PadRight(participantPad))
                .Append(result.Kart?.PadRight(kartPad))
                .Append(result.Heat?.Replace("Heat", string.Empty).Trim().PadRight(heatPad))
                .Append(result.BestLapTime.ToString().PadRight(blPad))
                .Append(result.Gap.ToString().PadRight(gapPad))
                .Append(result.Interval.ToString().PadRight(intervalPad))
                .Append(result.Percent.ToString("F3").PadRight(percentagePad))
                .Append(result.CompletedLaps.ToString().PadRight(lapsPad))
                .ToString();

            AnsiConsole.MarkupLine(rowString);
        }
    }
}
