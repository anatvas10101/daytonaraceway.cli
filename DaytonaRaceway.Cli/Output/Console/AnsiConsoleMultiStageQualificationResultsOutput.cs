using System.Text;
using DaytonaRaceway.Adapter.Models;
using Spectre.Console;

namespace DaytonaRaceway.Cli.Output.Console;

public static partial class AnsiConsoleOutput
{
    public static void PrintMultiStageQualificationResults(
        Guid raceId,
        IReadOnlyCollection<MultiStageQualificationResult> results)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Qualification results[/] — race [yellow]{raceId}[/]");
        AnsiConsole.WriteLine();

        var positionPad = 5;
        var participantPad = results.Max(x => x.Participant.Length) + 2;
        var lapTimePad = 10;

        var headerString = Enumerable.Range(1, results.Max(x => x.Results.Count))
            .Aggregate(
                new StringBuilder()
                    .Append("Pos".PadRight(positionPad))
                    .Append("Participant".PadRight(participantPad)),
                (agg, i) => agg.Append($"Q{i}".PadRight(lapTimePad)),
                finalBuild => finalBuild
                    .Append("Best lap".PadRight(lapTimePad))
                    .Append("Agg lap".PadRight(lapTimePad))
                    .ToString());

        AnsiConsole.MarkupLine($"[bold]{headerString}[/]");

        var position = 1;
        foreach (var result in results)
        {
            var rowString = Enumerable.Range(1, results.Max(x => x.Results.Count))
                .Aggregate(
                    new StringBuilder()
                        .Append(position.ToString().PadRight(positionPad))
                        .Append(result.Participant.PadRight(participantPad)),
                    (agg, i) => agg.Append(result.StageLapTime(i).ToString().PadRight(lapTimePad)),
                    finalBuild => finalBuild
                        .Append(result.BestLapTime.ToString().PadRight(lapTimePad))
                        .Append(result.AggregatedLapTime.ToString().PadRight(lapTimePad))
                        .ToString());

            AnsiConsole.MarkupLine(rowString);
            position++;
        }
    }
}
