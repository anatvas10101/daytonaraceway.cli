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

public sealed class QualificationResultsCommand : AsyncCommand<QualificationResultsCommand.Settings>
{
    public enum ResultsOrdering
    {
        Participant = 0,
        BestLap = 1,
        AggLap = 2,
    }

    public sealed class Settings : RaceCommandSettings, IAgentSettings
    {
        [CommandOption("-s|--stage <STAGE_ID>")]
        [Description("Stage ID. If omitted, results are aggregated across all stages of a command type.")]
        public int? Stage { get; init; }

        [CommandOption("-o|--order <ORDER>")]
        [Description("Ordering for multi-stage qualification results. Possible values are: BestLap, AggLap, Participant.")]
        [DefaultValue(ResultsOrdering.BestLap)]
        public ResultsOrdering Ordering { get; init; } = ResultsOrdering.BestLap;

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
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var handlingTask = settings.Stage.HasValue
            ? GetStageResults(settings.RaceId, settings.Stage.Value, settings, cancellationToken)
            : GetFullQualificationResults(settings.RaceId, settings, cancellationToken);

        await handlingTask;

        return 0;
    }

    private static async Task GetStageResults(
        Guid raceId,
        int stage,
        Settings settings,
        CancellationToken cancellationToken)
    {
        using var resultsAdapter = new QualificationResultsAdapter(settings);
        var results = await resultsAdapter.GetQualificationStageResults(raceId, stage, cancellationToken);

        var outputFile = settings.Format switch
        {
            OutputFormat.Csv =>
                await CsvOutput.WriteQualificationStageResults(raceId, stage, results, cancellationToken),
            OutputFormat.Html =>
                await HtmlOutput.WriteQualificationStageResults(raceId, stage, results, cancellationToken),
            _ => null,
        };

        if (outputFile != null)
        {
            AnsiConsole.MarkupLine($"Results: [link={outputFile.AbsoluteUri}]{outputFile.AbsoluteUri}[/]");
        }
        else
        {
            AnsiConsoleOutput.PrintQualificationStageResults(raceId, stage, results);
        }
    }

    private static async Task GetFullQualificationResults(
        Guid raceId,
        Settings settings,
        CancellationToken cancellationToken)
    {
        using var resultsAdapter = new QualificationResultsAdapter(settings);
        var results = await resultsAdapter.GetMultiStageQualificationResults(raceId, cancellationToken);
        var orderedResults = (settings.Ordering switch
            {
                ResultsOrdering.BestLap => results.OrderBy(r => r.BestLapTime)
                    .ThenBy(r => r.Participant),
                ResultsOrdering.AggLap => results.OrderBy(r => r.AggregatedLapTime)
                    .ThenBy(r => r.BestLapTime)
                    .ThenBy(r => r.Participant),
                ResultsOrdering.Participant => results.OrderBy(r => r.Participant),
                _ => results.OrderBy(_ => 1),
            })
            .ToArray();

        var outputFile = settings.Format switch
        {
            OutputFormat.Csv
                => await CsvOutput.WriteMultiStageQualificationResults(raceId, orderedResults, cancellationToken),
            OutputFormat.Html
                => await HtmlOutput.WriteMultiStageQualificationResults(raceId, orderedResults, cancellationToken),
            _ => null,
        };

        if (outputFile != null)
        {
            AnsiConsole.MarkupLine($"Results: [link={outputFile.AbsoluteUri}]{outputFile.AbsoluteUri}[/]");
        }
        else
        {
            AnsiConsoleOutput.PrintMultiStageQualificationResults(raceId, orderedResults);
        }
    }
}
