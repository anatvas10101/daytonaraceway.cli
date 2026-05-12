using System.Text;
using DaytonaRaceway.Cli.Commands;
using Spectre.Console;

namespace DaytonaRaceway.Cli.Output.Console;

public static partial class AnsiConsoleOutput
{
    public static void PrintPointsScale(
        PointsScaleCommand.ScaleType scaleType,
        IReadOnlyDictionary<int, int> pointsScale)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]{scaleType}[/] points scale for [bold]{pointsScale.Count} participants[/]");
        AnsiConsole.WriteLine();
        
        const int positionPad = 5;
        const int pointsPad = 6;

        var headerString = new StringBuilder()
            .Append("Pos".PadRight(positionPad))
            .Append("Points".PadRight(pointsPad))
            .ToString();

        AnsiConsole.MarkupLine($"[bold]{headerString}[/]");

        foreach (var (position, points) in pointsScale)
        {
            var rowString = new StringBuilder()
                .Append(position.ToString().PadRight(positionPad))
                .Append(points.ToString().PadRight(pointsPad))
                .ToString();

            AnsiConsole.MarkupLine(rowString);
        }
    }
}
