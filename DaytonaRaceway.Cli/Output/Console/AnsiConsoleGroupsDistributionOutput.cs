using DaytonaRaceway.Cli.Commands;
using Spectre.Console;

namespace DaytonaRaceway.Cli.Output.Console;

public static partial class AnsiConsoleOutput
{
    private static readonly Dictionary<char, string> GroupColors = new()
    {
        { 'A', "Cyan" },
        { 'B', "LightGreen" },
        { 'C', "Yellow" },
        { 'D', "Magenta" },
        { 'E', "Orange1" },
        { 'F', "DodgerBlue1" },
        { 'G', "IndianRed1" },
        { 'H', "Cyan" },
        { 'I', "LightGreen" },
        { 'J', "Yellow" },
        { 'K', "Magenta" },
        { 'L', "Orange1" },
        { 'M', "DodgerBlue1" },
        { 'N', "IndianRed1" },
        { 'O', "Cyan" },
        { 'P', "LightGreen" },
        { 'Q', "Yellow" },
        { 'R', "Magenta" },
        { 'S', "Orange1" },
        { 'T', "DodgerBlue1" },
        { 'U', "IndianRed1" },
        { 'V', "Cyan" },
        { 'W', "LightGreen" },
        { 'X', "Yellow" },
        { 'Y', "Magenta" },
        { 'Z', "Orange1" },
    };

    public static void PrintGroupsDistribution(
        Guid raceId,
        GroupsDistributionCommand.PreSplitResultsOrdering orderBy,
        IDictionary<string, string> participantsGroupsDistribution)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"Groups distribution by [bold]{orderBy}[/] - race [Yellow]{raceId}[/]");
        AnsiConsole.WriteLine();

        var participantsByGroup = participantsGroupsDistribution
            .GroupBy(x => x.Value, x => x.Key)
            .OrderBy(x => x.Key.Length)
            .ThenBy(x => x.Key);

        foreach (var group in participantsByGroup)
        {
            AnsiConsole.MarkupLine($"[bold][{GroupColors[group.Key.Last()]}]Group {group.Key}[/][/]");
            foreach (var participant in group)
            {
                AnsiConsole.MarkupLine($"[{GroupColors[group.Key.Last()]}]{participant}[/]");
            }
            AnsiConsole.WriteLine();
        }
    }
}
