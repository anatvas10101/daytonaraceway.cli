using System.Text;
using DaytonaRaceway.Adapter.Models;
using Spectre.Console;

namespace DaytonaRaceway.Cli.Output.Console;

public static partial class AnsiConsoleOutput
{
    public static void PrintRaceStageResults(
        Guid raceId,
        string stage,
        IReadOnlyCollection<RaceResult> results)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Race results[/] — race [yellow]{raceId}[/], stage [yellow]{stage}[/]");
        AnsiConsole.WriteLine();

        var heatPad = 6;
        var positionPad = 5;
        var participantPad = results.Max(x => x.Participant.Length) + 2;
        var kartPad = 6;
        var lapsPad = 6;
        var gapPad = Math.Max(results.Max(x => x.Gap.ToString()?.Length) ?? -1, "Gap".Length) + 2;
        var intervalPad = Math.Max(results.Max(x => x.Interval.ToString()?.Length) ?? -1, "Interval".Length) + 2;
        var blPad = Math.Max(results.Max(x => x.BestLapTime.ToString()?.Length) ?? -1, "Best lap".Length) + 2;
        var pointsPad = 5;

        var headerString = new StringBuilder()
            .Append("Heat".PadRight(heatPad))
            .Append("Pos".PadRight(positionPad))
            .Append("Participant".PadRight(participantPad))
            .Append("Kart".PadRight(kartPad))
            .Append("Laps".PadRight(lapsPad))
            .Append("Gap".PadRight(gapPad))
            .Append("Interval".PadRight(intervalPad))
            .Append("Best lap".PadRight(blPad))
            .Append("Pts".PadRight(pointsPad))
            .Append("Bon".PadRight(pointsPad))
            .Append("Pen".PadRight(pointsPad))
            .ToString();

        AnsiConsole.MarkupLine($"[bold]{headerString}[/]");

        string? currentHeat = null;
        foreach (var result in results)
        {
            if (currentHeat is not null && result.Heat != currentHeat)
                AnsiConsole.WriteLine();
            currentHeat = result.Heat;

            var rowString = new StringBuilder()
                .Append(result.Heat?.Replace("Heat", string.Empty).Trim().PadRight(heatPad))
                .Append(result.Position.ToString().PadRight(positionPad))
                .Append(result.Participant.PadRight(participantPad))
                .Append(result.Kart?.PadRight(kartPad))
                .Append(result.CompletedLaps.ToString().PadRight(lapsPad))
                .Append(result.Gap.ToString().PadRight(gapPad))
                .Append(result.Interval.ToString().PadRight(intervalPad))
                .Append(result.BestLapTime.ToString().PadRight(blPad))
                .Append((result.Points + result.ExtraPoints - result.Penalty).ToString().PadRight(pointsPad))
                .Append(result.ExtraPoints.ToString().PadRight(pointsPad))
                .Append(result.Penalty.ToString().PadRight(pointsPad))
                .ToString();

            AnsiConsole.MarkupLine(rowString);
        }
    }
}
