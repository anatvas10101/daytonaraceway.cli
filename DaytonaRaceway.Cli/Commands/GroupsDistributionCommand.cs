using System.ComponentModel;
using DaytonaRaceway.Adapter;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Cli.Output;
using DaytonaRaceway.Cli.Output.Csv;
using DaytonaRaceway.Cli.Output.Html;
using Spectre.Console;
using Spectre.Console.Cli;
using AnsiConsoleOutput = DaytonaRaceway.Cli.Output.Console.AnsiConsoleOutput;

namespace DaytonaRaceway.Cli.Commands;

public sealed class GroupsDistributionCommand : AsyncCommand<GroupsDistributionCommand.Settings>
{
    private const string GroupNamingLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    public enum PreSplitResultsOrdering
    {
        BestLap = 0,
        AggLap = 1,
    }

    public enum DistributionMode
    {
        Block = 0,
        Chequered = 1,
        Random = 2,
    }

    public sealed class Settings : RaceCommandSettings, IAgentSettings
    {
        [CommandOption("-c|--count <COUNT>", isRequired: true)]
        [Description("Number of groups to split participants to.")]
        public int Count { get; init; }

        [CommandOption("-m|--mode <MODE>")]
        [Description("Distribution mode of splitting into groups. Possible values are: Block, Chequered, Random.")]
        [DefaultValue(DistributionMode.Chequered)]
        public DistributionMode Mode { get; init; } = DistributionMode.Chequered;

        [CommandOption("-o|--order <ORDER>")]
        [Description("Sorting order of qualification results before splitting into groups. Possible values are: BestLap, AggLap.")]
        [DefaultValue(PreSplitResultsOrdering.BestLap)]
        public PreSplitResultsOrdering Ordering { get; init; } = PreSplitResultsOrdering.BestLap;

        [CommandOption("-f|--format <FORMAT>")]
        [Description("Output format for the results. Possible values are: Console, Html, Csv.")]
        [DefaultValue(OutputFormat.Console)]
        public OutputFormat Format { get; init; } = OutputFormat.Console;

        [CommandOption("--sourceip <IP>")]
        [Description("IP address of the API host.")]
        [DefaultValue(ApiAgent.DefaultIpHost)]
        public string SourceIp { get; init; } = ApiAgent.DefaultIpHost;

        [CommandOption("--port <PORT>", IsHidden = true)]
        [Description("Port of the API host.")]
        [DefaultValue(ApiAgent.DefaultPort)]
        public int Port { get; init; } = ApiAgent.DefaultPort;

        public override ValidationResult Validate()
            => Count switch
        {
            <= 0 => ValidationResult.Error("'--count' must be greater than 0."),
            >= 703 => ValidationResult.Error("This number of groups is not supported."),
            _ => ValidationResult.Success(),
        };
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        using var resultsAdapter = new QualificationResultsAdapter(settings);
        var qualificationResults =
            await resultsAdapter.GetMultiStageQualificationResults(settings.RaceId, cancellationToken);
        var orderedResults = (settings.Ordering switch
            {
                PreSplitResultsOrdering.BestLap => qualificationResults.OrderBy(r => r.BestLapTime)
                    .ThenBy(r => r.Participant),
                PreSplitResultsOrdering.AggLap => qualificationResults.OrderBy(r => r.AggregatedLapTime)
                    .ThenBy(r => r.BestLapTime)
                    .ThenBy(r => r.Participant),
                _ => qualificationResults.OrderBy(r => r.Participant),
            })
            .ToArray();

        var randomGroupsDistribution = settings.Mode == DistributionMode.Random
            ? RandomGroupsDistribution(settings.Count, orderedResults.Length)
            : [];

        var groupsDistribution = orderedResults
            .Select((r, i) => new
            {
                r.Participant,
                Group = settings.Mode switch
                {
                    DistributionMode.Block => GroupInSequentialOrder(i, settings.Count, orderedResults.Length),
                    DistributionMode.Chequered => GroupInChequeredOrder(i, settings.Count),
                    DistributionMode.Random => randomGroupsDistribution[i],
                    _ => throw new NotSupportedException($"{settings.Mode} is not supported."),
                },
            })
            .OrderBy(group =>
                group.Group.Length) // Trick for alphabetical order of single-letter and double-letter groups
            .ThenBy(group => group.Group)
            .ToDictionary(group => group.Participant, group => group.Group);

        var outputFile = settings.Format switch
        {
            OutputFormat.Csv
                => await CsvOutput.WriteGroupsDistribution(settings.RaceId, groupsDistribution, cancellationToken),
            OutputFormat.Html
                => await HtmlOutput.WriteGroupsDistribution(settings.RaceId, groupsDistribution, cancellationToken),
            _ => null,
        };

        if (outputFile != null)
        {
            AnsiConsole.MarkupLine(
                $"Groups distribution by [bold]{settings.Ordering}[/]: [link={outputFile.AbsoluteUri}]{outputFile.AbsoluteUri}[/]");
        }
        else
        {
            AnsiConsoleOutput.PrintGroupsDistribution(settings.RaceId, settings.Ordering, groupsDistribution);
        }

        await WinClipboard.SetTextAsync(
            string.Join(Environment.NewLine, groupsDistribution.Select(x => x.Key)),
            cancellationToken);

        return 0;
    }

    private static string GroupInSequentialOrder(int index, int groupsCount, int participantsCount)
    {
        var cleanGroupSize = participantsCount / groupsCount;
        var leftover = participantsCount % groupsCount;
        var isIndexOvercomeGroupsWithLeftover = index > leftover * cleanGroupSize + leftover;
        var adjustedIndex = isIndexOvercomeGroupsWithLeftover ? index - leftover : index;
        var adjustedGroupSize = participantsCount / groupsCount + (isIndexOvercomeGroupsWithLeftover ? 0 : 1);
        var letterIndex = adjustedIndex / adjustedGroupSize;
            
        return letterIndex >= 26
            ? $"{GroupNamingLetters[(letterIndex / 26 - 1) % 26]}{GroupNamingLetters[letterIndex % 26]}"
            : $"{GroupNamingLetters[letterIndex]}";
    }

    private static string GroupInChequeredOrder(int index, int groupsCount)
    {
        var letterIndex = index % groupsCount;
        return letterIndex >= 26
            ? $"{GroupNamingLetters[(letterIndex / 26 - 1) % 26]}{GroupNamingLetters[letterIndex % 26]}"
            : $"{GroupNamingLetters[letterIndex]}";
    }

    private static string[] RandomGroupsDistribution(int groupsCount, int participantsCount)
    {
        var cleanGroupSize = participantsCount / groupsCount;
        var leftover = participantsCount % groupsCount;
        return Enumerable.Range(0, groupsCount)
            .SelectMany(letterIndex =>
            {
                var label = letterIndex >= 26
                    ? $"{GroupNamingLetters[(letterIndex / 26 - 1) % 26]}{GroupNamingLetters[letterIndex % 26]}"
                    : $"{GroupNamingLetters[letterIndex]}";
                return Enumerable.Repeat(label, letterIndex < leftover ? cleanGroupSize + 1 : cleanGroupSize);
            })
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();
    }
}
